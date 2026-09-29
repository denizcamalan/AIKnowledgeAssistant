export type ChatStreamEventHandlers = {
  onStarted?: (streamId: string) => void;
  onDelta?: (text: string) => void;
  onDone?: (payload: ChatStreamDonePayload) => void;
  onStopped?: (payload: { streamId: string; reason?: string }) => void;
  onError?: (message: string) => void;
};

export type ChatStreamDonePayload = {
  streamId?: string;
  model?: string;
  provider?: string;
  promptTokens?: number;
  completionTokens?: number;
  providerDurationMs?: number;
};

type StreamChatOptions = {
  baseUrl?: string;
  getAccessToken: () => string | null;
  message: string;
  systemMessage?: string;
  signal?: AbortSignal;
  handlers: ChatStreamEventHandlers;
};

export async function streamChatMessage(options: StreamChatOptions): Promise<void> {
  const baseUrl = options.baseUrl ?? "";
  const headers = new Headers({ "Content-Type": "application/json" });
  const token = options.getAccessToken();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${baseUrl}/api/chat/stream`, {
    method: "POST",
    headers,
    body: JSON.stringify({ message: options.message, systemMessage: options.systemMessage }),
    signal: options.signal,
  });

  if (!response.ok) {
    throw new Error(`Chat stream failed (${response.status}).`);
  }

  if (!response.body) {
    throw new Error("Chat stream response had no body.");
  }

  await consumeSse(response.body, options.handlers, options.signal);
}

export async function stopChatStream(options: {
  baseUrl?: string;
  getAccessToken: () => string | null;
  streamId: string;
}): Promise<void> {
  const baseUrl = options.baseUrl ?? "";
  const headers = new Headers({ "Content-Type": "application/json" });
  const token = options.getAccessToken();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${baseUrl}/api/chat/stream/stop`, {
    method: "POST",
    headers,
    body: JSON.stringify({ streamId: options.streamId }),
  });

  if (response.status === 404) {
    return;
  }

  if (!response.ok) {
    throw new Error(`Stop chat stream failed (${response.status}).`);
  }
}

export async function consumeSse(
  body: ReadableStream<Uint8Array>,
  handlers: ChatStreamEventHandlers,
  signal?: AbortSignal,
): Promise<void> {
  const reader = body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    if (signal?.aborted) {
      throw new DOMException("Aborted", "AbortError");
    }

    const { done, value } = await reader.read();
    if (done) {
      break;
    }

    buffer += decoder.decode(value, { stream: true });
    buffer = dispatchSseBlocks(buffer, handlers);
  }
}

function dispatchSseBlocks(buffer: string, handlers: ChatStreamEventHandlers): string {
  const blocks = buffer.split("\n\n");
  const remainder = blocks.pop() ?? "";

  for (const block of blocks) {
    if (!block.trim()) {
      continue;
    }

    let eventName = "message";
    const dataLines: string[] = [];
    for (const line of block.split("\n")) {
      if (line.startsWith("event:")) {
        eventName = line.slice("event:".length).trim();
      } else if (line.startsWith("data:")) {
        dataLines.push(line.slice("data:".length).trim());
      }
    }

    if (dataLines.length === 0) {
      continue;
    }

    const payload = JSON.parse(dataLines.join("\n")) as Record<string, unknown>;
    switch (eventName) {
      case "started":
        handlers.onStarted?.(String(payload.streamId));
        break;
      case "delta":
        handlers.onDelta?.(String(payload.text ?? ""));
        break;
      case "done":
        handlers.onDone?.(payload as ChatStreamDonePayload);
        break;
      case "stopped":
        handlers.onStopped?.({
          streamId: String(payload.streamId),
          reason: payload.reason ? String(payload.reason) : undefined,
        });
        break;
      case "error":
        handlers.onError?.(String(payload.message ?? "Stream error."));
        break;
      default:
        break;
    }
  }

  return remainder;
}
