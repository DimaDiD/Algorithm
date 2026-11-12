const CLIENT_ID_KEY = "mmsa.clientId";

const createId = (): string => {
  if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") return crypto.randomUUID();
  const bytes = new Uint8Array(16);
  crypto.getRandomValues(bytes);
  bytes[6] = (bytes[6] & 0x0f) | 0x40;
  bytes[8] = (bytes[8] & 0x3f) | 0x80;
  const hex = Array.from(bytes, (byte) => byte.toString(16).padStart(2, "0")).join("");
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
};

let memoryId: string | null = null;

export const resetClientId = (): string => {
  const created = createId();
  memoryId = created;
  try {
    localStorage.setItem(CLIENT_ID_KEY, created);
  } catch {}
  return created;
};

export const getClientId = (): string => {
  try {
    const stored = localStorage.getItem(CLIENT_ID_KEY);
    if (stored) return stored;
    const created = createId();
    localStorage.setItem(CLIENT_ID_KEY, created);
    return created;
  } catch {
    memoryId = memoryId ?? createId();
    return memoryId;
  }
};
