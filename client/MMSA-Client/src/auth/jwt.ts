export type AuthUser = {
  id: string;
  email: string;
  name: string;
  roles: string[];
};

const decodeClaims = (token: string): Record<string, any> | null => {
  try {
    const payload = token.split(".")[1];

    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64 + "=".repeat((4 - (base64.length % 4)) % 4);

    const json = decodeURIComponent(
      atob(padded)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );

    return JSON.parse(json);
  } catch {
    return null;
  }
};

export const decodeUser = (token: string): AuthUser | null => {
  const claims = decodeClaims(token);
  if (!claims) return null;

  const role = claims.role;
  return {
    id: claims.sub,
    email: claims.email,
    name: claims.name,
    roles: Array.isArray(role) ? role : role ? [role] : [],
  };
};

export const isTokenFresh = (token: string | null, marginSeconds = 30): boolean => {
  if (!token) return false;
  const exp = decodeClaims(token)?.exp;
  return typeof exp === "number" && exp * 1000 - Date.now() > marginSeconds * 1000;
};
