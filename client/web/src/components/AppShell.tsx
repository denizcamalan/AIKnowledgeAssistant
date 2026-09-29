import { useState } from "react";
import { useAuth } from "../auth/AuthProvider";
import { ChatScreen } from "../screens/ChatScreen";
import { DocumentsScreen } from "../screens/DocumentsScreen";

type View = "documents" | "chat";

export function AppShell() {
  const { user, logout, sessionWarning } = useAuth();
  const [view, setView] = useState<View>("documents");

  return (
    <div className="shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">AI Knowledge Assistant</p>
          <p className="who">{user?.displayName}</p>
        </div>
        <nav aria-label="Primary">
          <button type="button" aria-current={view === "documents" ? "page" : undefined} onClick={() => setView("documents")}>
            Documents
          </button>
          <button type="button" aria-current={view === "chat" ? "page" : undefined} onClick={() => setView("chat")}>
            Chat
          </button>
        </nav>
        <button type="button" className="ghost" onClick={logout}>
          Sign out
        </button>
      </header>

      {sessionWarning ? (
        <p className="warning" role="status">
          {sessionWarning}
        </p>
      ) : null}

      <main>{view === "documents" ? <DocumentsScreen /> : <ChatScreen />}</main>
    </div>
  );
}
