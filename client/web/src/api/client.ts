import type { DocumentSummary, LoginResponse, ProblemDetailsBody, UserProfile } from "./types";

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

export type ApiClient = {
  login: (email: string, password: string) => Promise<LoginResponse>;
  me: () => Promise<UserProfile>;
  listDocuments: () => Promise<DocumentSummary[]>;
};

type CreateApiClientOptions = {
  baseUrl?: string;
  getAccessToken: () => string | null;
};

export function createApiClient(options: CreateApiClientOptions): ApiClient {
  const baseUrl = options.baseUrl ?? "";

  async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
    const headers = new Headers(init.headers);
    if (init.body && !(init.body instanceof FormData) && !headers.has("Content-Type")) {
      headers.set("Content-Type", "application/json");
    }

    const token = options.getAccessToken();
    if (token) {
      headers.set("Authorization", `Bearer ${token}`);
    }

    const response = await fetch(`${baseUrl}${path}`, { ...init, headers });
    if (!response.ok) {
      throw new ApiError(response.status, await readErrorMessage(response));
    }

    if (response.status === 204) {
      return undefined as T;
    }

    return (await response.json()) as T;
  }

  return {
    login(email, password) {
      return request<LoginResponse>("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });
    },
    me() {
      return request<UserProfile>("/api/account/me");
    },
    listDocuments() {
      return request<DocumentSummary[]>("/api/documents");
    },
  };
}

export function createAppApiClient(getAccessToken: () => string | null): ApiClient {
  return createApiClient({
    baseUrl: import.meta.env.VITE_API_BASE_URL ?? "",
    getAccessToken,
  });
}

async function readErrorMessage(response: Response): Promise<string> {
  try {
    const body = (await response.json()) as ProblemDetailsBody;
    return body.detail || body.title || fallbackStatus(response.status);
  } catch {
    return fallbackStatus(response.status);
  }
}

function fallbackStatus(status: number): string {
  if (status === 401) {
    return "Sign-in failed or the session is no longer valid.";
  }

  return `Request failed (${status}).`;
}
