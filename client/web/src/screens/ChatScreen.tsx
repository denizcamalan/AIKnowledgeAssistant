import { useEffect, useRef, useState, type FormEvent } from "react";
import { streamChatMessage, stopChatStream } from "../api/chatStream";
import { ApiError } from "../api/client";
import type { DocumentSummary } from "../api/types";
import { useAuth } from "../auth/AuthProvider";

type ChatMessage = {
  id: string;
  role: "user" | "assistant";
  text: string;
  streaming?: boolean;
  error?: boolean;
};

export function ChatScreen() {
  const { api, user, getAccessToken } = useAuth();
  const [documents, setDocuments] = useState<DocumentSummary[] | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [draft, setDraft] = useState("");
  const [isStreaming, setIsStreaming] = useState(false);
  const [streamError, setStreamError] = useState<string | null>(null);
  const activeStreamIdRef = useRef<string | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);

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

  useEffect(() => {
    return () => {
      void cleanupStream();
    };
  }, []);

  async function cleanupStream() {
    abortControllerRef.current?.abort();
    abortControllerRef.current = null;
    const streamId = activeStreamIdRef.current;
    activeStreamIdRef.current = null;
    if (streamId) {
      try {
        await stopChatStream({ getAccessToken, streamId });
      } catch {
        // Best-effort cleanup when navigating away.
      }
    }
  }

  async function onStop() {
    const streamId = activeStreamIdRef.current;
    if (streamId) {
      await stopChatStream({ getAccessToken, streamId });
    }
    abortControllerRef.current?.abort();
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const text = draft.trim();
    if (!text || isStreaming) {
      return;
    }

    const assistantId = crypto.randomUUID();
    setDraft("");
    setStreamError(null);
    setIsStreaming(true);
    setMessages((current) => [
      ...current,
      { id: crypto.randomUUID(), role: "user", text },
      { id: assistantId, role: "assistant", text: "", streaming: true },
    ]);

    const controller = new AbortController();
    abortControllerRef.current = controller;
    activeStreamIdRef.current = null;

    try {
      await streamChatMessage({
        getAccessToken,
        message: text,
        signal: controller.signal,
        handlers: {
          onStarted: (streamId) => {
            activeStreamIdRef.current = streamId;
          },
          onDelta: (delta) => {
            setMessages((current) =>
              current.map((message) =>
                message.id === assistantId ? { ...message, text: message.text + delta } : message,
              ),
            );
          },
          onDone: () => {
            setMessages((current) =>
              current.map((message) =>
                message.id === assistantId ? { ...message, streaming: false } : message,
              ),
            );
          },
          onStopped: () => {
            setMessages((current) =>
              current.map((message) =>
                message.id === assistantId ? { ...message, streaming: false } : message,
              ),
            );
          },
          onError: (message) => {
            setStreamError(message);
            setMessages((current) =>
              current.map((item) =>
                item.id === assistantId
                  ? { ...item, text: message, streaming: false, error: true }
                  : item,
              ),
            );
          },
        },
      });
    } catch (caught: unknown) {
      if (caught instanceof DOMException && caught.name === "AbortError") {
        setMessages((current) =>
          current.map((message) =>
            message.id === assistantId ? { ...message, streaming: false } : message,
          ),
        );
      } else {
        const message = caught instanceof Error ? caught.message : "Chat stream failed.";
        setStreamError(message);
        setMessages((current) =>
          current.map((item) =>
            item.id === assistantId ? { ...item, text: message, streaming: false, error: true } : item,
          ),
        );
      }
    } finally {
      setIsStreaming(false);
      activeStreamIdRef.current = null;
      abortControllerRef.current = null;
    }
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
            <p className="muted">Answers stream live from POST /api/chat/stream (SSE).</p>
          </div>
          {isStreaming ? (
            <button type="button" className="secondary-button" onClick={() => void onStop()}>
              Stop
            </button>
          ) : null}
        </header>

        {streamError ? (
          <p className="error" role="alert">
            {streamError}
          </p>
        ) : null}

        <div className="transcript" aria-live="polite">
          {messages.length === 0 ? <p className="muted">Ask a question about your documents.</p> : null}
          {messages.map((message) => (
            <p key={message.id} className={`bubble ${message.role}${message.error ? " error" : ""}`}>
              {message.text}
              {message.streaming ? <span className="streaming-cursor">▍</span> : null}
            </p>
          ))}
        </div>

        <form className="composer" onSubmit={(event) => void onSubmit(event)}>
          <label className="sr-only" htmlFor="question">
            Question
          </label>
          <input
            id="question"
            name="question"
            value={draft}
            onChange={(event) => setDraft(event.target.value)}
            placeholder="Ask a question"
            disabled={isStreaming}
          />
          <button type="submit" disabled={isStreaming}>
            Send
          </button>
        </form>
      </div>
    </section>
  );
}
