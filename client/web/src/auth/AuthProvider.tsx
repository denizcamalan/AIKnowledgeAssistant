import { createContext, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { ApiError, createAppApiClient, type ApiClient } from "../api/client";
import type { UserProfile } from "../api/types";
import { clearSession, loadSession, saveSession, type StoredSession } from "./tokenStorage";

type AuthContextValue = {
  user: UserProfile | null;
  ready: boolean;
  sessionWarning: string | null;
  api: ApiClient;
  getAccessToken: () => string | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<StoredSession | null>(null);
  const [ready, setReady] = useState(false);
  const [sessionWarning, setSessionWarning] = useState<string | null>(null);
  const tokenRef = useRef<string | null>(null);
  tokenRef.current = session?.accessToken ?? tokenRef.current;

  const api = useMemo(() => createAppApiClient(() => tokenRef.current), []);

  useEffect(() => {
    const stored = loadSession();
    if (!stored) {
      tokenRef.current = null;
      setReady(true);
      return;
    }

    tokenRef.current = stored.accessToken;
    let cancelled = false;

    api
      .me()
      .then((profile) => {
        if (cancelled) {
          return;
        }

        const next = { ...stored, user: profile };
        saveSession(next);
        setSession(next);
        setSessionWarning(null);
      })
      .catch((error: unknown) => {
        if (cancelled) {
          return;
        }

        if (error instanceof ApiError && error.status === 401) {
          clearSession();
          tokenRef.current = null;
          setSession(null);
          setSessionWarning(null);
          return;
        }

        setSession(stored);
        setSessionWarning("Could not refresh the profile. Showing the saved session.");
      })
      .finally(() => {
        if (!cancelled) {
          setReady(true);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [api]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      ready,
      sessionWarning,
      api,
      getAccessToken: () => tokenRef.current,
      async login(email, password) {
        const result = await api.login(email, password);
        tokenRef.current = result.accessToken;
        const stored: StoredSession = {
          accessToken: result.accessToken,
          expiresAtUtc: result.expiresAtUtc,
          user: result.user,
        };
        saveSession(stored);

        try {
          const profile = await api.me();
          const confirmed: StoredSession = { ...stored, user: profile };
          saveSession(confirmed);
          setSession(confirmed);
          setSessionWarning(null);
        } catch (error) {
          clearSession();
          tokenRef.current = null;
          setSession(null);
          throw error;
        }
      },
      logout() {
        clearSession();
        tokenRef.current = null;
        setSession(null);
        setSessionWarning(null);
      },
    }),
    [api, ready, session, sessionWarning],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);
  if (!value) {
    throw new Error("useAuth must be used within AuthProvider");
  }

  return value;
}
