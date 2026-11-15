import axios from "axios";
import BASE_URL from "../config";
import { tokenStore } from "./tokenStore";
import { isTokenFresh } from "./jwt";
import { broadcastLogout, broadcastToken, withRefreshLock } from "./authSync";

export type AccessTokenResponse = {
  AccessToken: string;
  TokenType: string;
  ExpiresAtUtc: string;
};

const authClient = axios.create({
  baseURL: BASE_URL + "Auth/",
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

export const login = async (email: string, password: string): Promise<void> => {
  const { data } = await authClient.post<AccessTokenResponse>("login", { Email: email, Password: password });
  tokenStore.set(data.AccessToken);
  broadcastToken(data.AccessToken);
};

export const register = async (email: string, password: string): Promise<void> => {
  const { data } = await authClient.post<AccessTokenResponse>("register", { Email: email, Password: password });
  tokenStore.set(data.AccessToken);
  broadcastToken(data.AccessToken);
};

let refreshPromise: Promise<string | null> | null = null;

const refreshInsideLock = async (staleToken: string | null): Promise<string | null> => {
  const current = tokenStore.get();
  if (current && current !== staleToken && isTokenFresh(current)) return current;

  try {
    const { data } = await authClient.post<AccessTokenResponse>("refresh");
    tokenStore.set(data.AccessToken);
    broadcastToken(data.AccessToken);
    return data.AccessToken;
  } catch {
    tokenStore.set(null);
    return null;
  }
};

export const refreshAccessToken = (): Promise<string | null> => {
  if (!refreshPromise) {
    const staleToken = tokenStore.get();
    refreshPromise = withRefreshLock(() => refreshInsideLock(staleToken)).finally(() => {
      refreshPromise = null;
    });
  }
  return refreshPromise;
};

export const logout = async (): Promise<void> => {
  try {
    await authClient.post("logout");
  } finally {
    tokenStore.set(null);
    broadcastLogout();
  }
};

export const getErrorMessages = (error: unknown): string[] => {
  if (axios.isAxiosError(error)) {
    const status = error.response?.status;
    if (status === 429) return ["Забагато спроб. Спробуйте пізніше."];

    const data: any = error.response?.data;
    if (Array.isArray(data?.errors)) return data.errors;
    if (data?.errors && typeof data.errors === "object") {
      return Object.values(data.errors).flat() as string[];
    }
    if (!error.response) return ["Сервер недоступний."];
  }
  return ["Щось пішло не так. Спробуйте ще раз."];
};
