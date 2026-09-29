import { useState, type FormEvent } from "react";
import { ApiError } from "../api/client";
import { useAuth } from "../auth/AuthProvider";

export function LoginScreen() {
  const { login } = useAuth();
  const [email, setEmail] = useState("user@demo.local");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [pending, setPending] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setPending(true);
    try {
      await login(email.trim(), password);
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : "Could not reach the API.");
    } finally {
      setPending(false);
    }
  }

  return (
    <main className="login-layout">
      <form className="card login-card" onSubmit={onSubmit}>
        <p className="eyebrow">AI Knowledge Assistant</p>
        <h1>Sign in</h1>
        <p className="muted">Use a demo account. The API returns a bearer token stored in this browser.</p>

        <label htmlFor="email">Email</label>
        <input
          id="email"
          name="email"
          type="email"
          autoComplete="username"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          required
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          name="password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          required
        />

        {error ? (
          <p className="error" role="alert">
            {error}
          </p>
        ) : null}

        <button type="submit" disabled={pending}>
          {pending ? "Signing in…" : "Sign in"}
        </button>
        <p className="hint">Demo: user@demo.local / User123! or admin@demo.local / Admin123!</p>
      </form>
    </main>
  );
}
