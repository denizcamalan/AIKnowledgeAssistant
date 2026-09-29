import type { UserProfile } from "../api/types";

const storageKey = "aka.session";

export type StoredSession = {
  accessToken: string;
  expiresAtUtc: string;
  user: UserProfile;
};

type TokenStorage = Pick<Storage, "getItem" | "setItem" | "removeItem">;

export function saveSession(session: StoredSession, storage: TokenStorage = localStorage): void {
  storage.setItem(storageKey, JSON.stringify(session));
}

export function loadSession(storage: TokenStorage = localStorage, now = Date.now()): StoredSession | null {
  const raw = storage.getItem(storageKey);
  if (!raw) {
    return null;
  }

  try {
    const session = JSON.parse(raw) as StoredSession;
    if (!session.accessToken || !session.expiresAtUtc || Number.isNaN(Date.parse(session.expiresAtUtc))) {
      storage.removeItem(storageKey);
      return null;
    }

    if (Date.parse(session.expiresAtUtc) <= now) {
      storage.removeItem(storageKey);
      return null;
    }

    return session;
  } catch {
    storage.removeItem(storageKey);
    return null;
  }
}

export function clearSession(storage: TokenStorage = localStorage): void {
  storage.removeItem(storageKey);
}
