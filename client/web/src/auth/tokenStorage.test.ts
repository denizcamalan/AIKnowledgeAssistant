import { describe, expect, it } from "vitest";
import { clearSession, loadSession, saveSession, type StoredSession } from "./tokenStorage";

describe("tokenStorage", () => {
  it("round-trips a session and drops an expired token", () => {
    const storage = memoryStorage();
    const session: StoredSession = {
      accessToken: "token",
      expiresAtUtc: "2099-01-01T00:00:00.000Z",
      user: { userId: "1", email: "user@demo.local", displayName: "Demo", roles: ["User"] },
    };

    saveSession(session, storage);
    expect(loadSession(storage, Date.parse("2080-01-01T00:00:00.000Z"))).toEqual(session);

    expect(loadSession(storage, Date.parse("2100-01-01T00:00:00.000Z"))).toBeNull();
    expect(storage.getItem("aka.session")).toBeNull();
  });

  it("clears a corrupt payload", () => {
    const storage = memoryStorage();
    storage.setItem("aka.session", "{not-json");
    expect(loadSession(storage)).toBeNull();
    expect(storage.getItem("aka.session")).toBeNull();
  });

  it("removes the session on logout", () => {
    const storage = memoryStorage();
    saveSession(
      {
        accessToken: "token",
        expiresAtUtc: "2099-01-01T00:00:00.000Z",
        user: { userId: "1", email: "user@demo.local", displayName: "Demo", roles: ["User"] },
      },
      storage,
    );

    clearSession(storage);
    expect(loadSession(storage)).toBeNull();
  });
});

function memoryStorage(): Storage {
  const values = new Map<string, string>();
  return {
    get length() {
      return values.size;
    },
    clear() {
      values.clear();
    },
    getItem(key: string) {
      return values.get(key) ?? null;
    },
    key(index: number) {
      return [...values.keys()][index] ?? null;
    },
    removeItem(key: string) {
      values.delete(key);
    },
    setItem(key: string, value: string) {
      values.set(key, value);
    },
  };
}
