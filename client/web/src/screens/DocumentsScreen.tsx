import { useCallback, useEffect, useState } from "react";
import { ApiError } from "../api/client";
import type { DocumentSummary } from "../api/types";
import { useAuth } from "../auth/AuthProvider";

export function DocumentsScreen() {
  const { api } = useAuth();
  const [documents, setDocuments] = useState<DocumentSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      setDocuments(await api.listDocuments());
    } catch (caught) {
      setDocuments(null);
      setError(caught instanceof ApiError ? caught.message : "Could not load documents.");
    }
  }, [api]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <section className="panel">
      <header className="panel-header">
        <div>
          <h2>Documents</h2>
          <p className="muted">Loaded from GET /api/documents.</p>
        </div>
        <button type="button" onClick={() => void load()}>
          Refresh
        </button>
      </header>

      {error ? (
        <p className="error" role="alert">
          {error}
        </p>
      ) : null}

      {documents === null && !error ? <p className="muted">Loading documents…</p> : null}

      {documents?.length === 0 ? <p className="muted">No documents yet. Upload one with the API, then refresh.</p> : null}

      {documents && documents.length > 0 ? (
        <ul className="document-list">
          {documents.map((document) => (
            <li key={document.id}>
              <strong>{document.displayName}</strong>
              <span>{document.originalFileName}</span>
              <span>{formatStatus(document.status)}</span>
              <span>{formatBytes(document.sizeBytes)}</span>
              <time dateTime={document.createdAtUtc}>{formatWhen(document.createdAtUtc)}</time>
            </li>
          ))}
        </ul>
      ) : null}
    </section>
  );
}

function formatStatus(status: number): string {
  return status === 0 ? "Uploaded" : `Status ${status}`;
}

function formatBytes(size: number): string {
  if (size < 1024) {
    return `${size} B`;
  }

  return `${(size / 1024).toFixed(1)} KB`;
}

function formatWhen(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString();
}
