import { AuthProvider, useAuth } from "./auth/AuthProvider";
import { AppShell } from "./components/AppShell";
import { LoginScreen } from "./screens/LoginScreen";

export function App() {
  return (
    <AuthProvider>
      <Gate />
    </AuthProvider>
  );
}

function Gate() {
  const { user, ready } = useAuth();

  if (!ready) {
    return (
      <main className="login-layout">
        <p className="muted">Checking session…</p>
      </main>
    );
  }

  if (!user) {
    return <LoginScreen />;
  }

  return <AppShell />;
}
