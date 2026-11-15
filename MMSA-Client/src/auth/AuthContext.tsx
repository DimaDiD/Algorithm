import { createContext, FC, ReactNode, useContext, useEffect, useMemo, useState } from "react";
import { AuthUser, decodeUser } from "./jwt";
import { tokenStore } from "./tokenStore";
import { login, logout, refreshAccessToken, register } from "./authApi";
import { EDITOR_ROLES } from "./roles";
import { listenToOtherTabs } from "./authSync";

type AuthContextValue = {
  user: AuthUser | null;
  isInitialized: boolean;
  canEdit: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const userFromToken = (token: string | null) => (token ? decodeUser(token) : null);

export const AuthProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(() => userFromToken(tokenStore.get()));
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => tokenStore.subscribe((token) => setUser(userFromToken(token))), []);

  useEffect(() => listenToOtherTabs(), []);

  useEffect(() => {
    refreshAccessToken().finally(() => setIsInitialized(true));
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isInitialized,
      canEdit: !!user && user.roles.some((role) => EDITOR_ROLES.includes(role)),
      login,
      register,
      logout,
    }),
    [user, isInitialized]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside <AuthProvider>");
  }
  return context;
};
