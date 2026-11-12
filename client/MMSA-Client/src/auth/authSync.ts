import { tokenStore } from "./tokenStore";

type AuthMessage = { type: "token"; token: string } | { type: "logout" };

const CHANNEL_NAME = "mmsa-auth";
const REFRESH_LOCK = "mmsa-refresh";

const channel: BroadcastChannel | null = typeof BroadcastChannel === "undefined" ? null : new BroadcastChannel(CHANNEL_NAME);

export const broadcastToken = (token: string) => channel?.postMessage({ type: "token", token } satisfies AuthMessage);

export const broadcastLogout = () => channel?.postMessage({ type: "logout" } satisfies AuthMessage);

export const listenToOtherTabs = (): (() => void) => {
  if (!channel) return () => undefined;

  const handleMessage = (event: MessageEvent<AuthMessage>) => {
    const message = event.data;
    if (message?.type === "token" && typeof message.token === "string") tokenStore.set(message.token);
    if (message?.type === "logout") tokenStore.set(null);
  };

  channel.addEventListener("message", handleMessage);
  return () => channel.removeEventListener("message", handleMessage);
};

export const withRefreshLock = <T>(task: () => Promise<T>): Promise<T> => {
  const locks = typeof navigator !== "undefined" ? navigator.locks : undefined;
  return locks?.request ? locks.request(REFRESH_LOCK, task) : task();
};
