import { useEffect, useState, type FormEvent } from "react";
import { ApiError } from "../api/client";
import type { DocumentSummary } from "../api/types";
import { useAuth } from "../auth/AuthProvider";

type ChatMessage = {
  id: string;
  role: "user" | "assistant";
  text: string;
};

export function ChatScreen() {
  const { api, user } = useAuth();
  const [documents, setDocuments] = useState<DocumentSummary[] | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [draft, setDraft] = useState("");

  useEffect(() => {
    let cancelled = false;
    api
      .listDocuments()
      .then((loaded) => {
        if (!cancelled) {
          setDocuments(loaded);
          setLoadError(null);
        }
      })
      .catch((caught: unknown) => {
        if (!cancelled) {
          setDocuments(null);
          setLoadError(caught instanceof ApiError ? caught.message : "Could not load documents for chat.");
        }
      });

    return () => {
      cancelled = true;
    };
  }, [api]);

  function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const text = draft.trim();
    if (!text) {
      return;
    }

    const documentCount = documents?.length ?? 0;
    const context =
      loadError !== null
        ? "Document context could not be loaded from the API."
        : `The API currently lists ${documentCount} document${documentCount === 1 ? "" : "s"}.`;

    setDraft("");
    setMessages((current) => [
      ...current,
      { id: crypto.randomUUID(), role: "user", text },
      {
        id: crypto.randomUUID(),
        role: "assistant",
        text: `${context} Grounded streaming answers are not implemented yet; this shell only keeps the question in component state.`,
      },
    ]);
  }

  return (
    <section className="chat-layout">
      <aside className="panel context-panel">
        <h2>Context</h2>
        <p className="muted">Signed in as {user?.displayName ?? "unknown"} via GET /api/account/me.</p>
        {loadError ? (
          <p className="error" role="alert">
            {loadError}
          </p>
        ) : null}
        {documents === null && !loadError ? <p className="muted">Loading documents…</p> : null}
        {documents?.length === 0 ? <p className="muted">No documents to cite yet.</p> : null}
        {documents && documents.length > 0 ? (
          <ul className="context-list">
            {documents.map((document) => (
              <li key={document.id}>{document.displayName}</li>
            ))}
          </ul>
        ) : null}
      </aside>

      <div className="panel chat-panel">
        <header className="panel-header">
          <div>
            <h2>Chat</h2>
            <p className="muted">Questions stay in this screen until the streaming API exists.</p>
          </div>
        </header>

        <div className="transcript" aria-live="polite">
          {messages.length === 0 ? <p className="muted">Ask a question about your documents.</p> : null}
          {messages.map((message) => (
            <p key={message.id} className={`bubble ${message.role}`}>
              {message.text}
            </p>
          ))}
        </div>

        <form className="composer" onSubmit={onSubmit}>
          <label className="sr-only" htmlFor="question">
            Question
          </label>
          <input
            id="question"
            name="question"
            value={draft}
            onChange={(event) => setDraft(event.target.value)}
            placeholder="Ask a question"
          />
          <button type="submit">Send</button>
        </form>
      </div>
    </section>
  );
}
