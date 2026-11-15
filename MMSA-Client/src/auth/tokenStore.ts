type Listener = (token: string | null) => void;

let accessToken: string | null = null;
const listeners = new Set<Listener>();

export const tokenStore = {
  get: (): string | null => accessToken,

  set: (token: string | null): void => {
    accessToken = token;
    listeners.forEach((listener) => listener(token));
  },

  subscribe: (listener: Listener): (() => void) => {
    listeners.add(listener);
    return () => {
      listeners.delete(listener);
    };
  },
};
