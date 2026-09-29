import { describe, expect, it, vi } from "vitest";
import { ApiError, createApiClient } from "./client";

describe("createApiClient", () => {
  it("posts login credentials without an Authorization header", async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      jsonResponse({
        accessToken: "token",
        expiresAtUtc: "2099-01-01T00:00:00Z",
        user: { userId: "1", email: "user@demo.local", displayName: "User", roles: ["User"] },
      }),
    );
    vi.stubGlobal("fetch", fetchMock);

    const client = createApiClient({
      baseUrl: "http://api.test",
      getAccessToken: () => null,
    });

    await client.login("user@demo.local", "User123!");

    expect(fetchMock).toHaveBeenCalledOnce();
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("http://api.test/api/auth/login");
    const headers = new Headers(init.headers);
    expect(headers.get("Authorization")).toBeNull();
    expect(headers.get("Content-Type")).toBe("application/json");
    expect(init.body).toBe(JSON.stringify({ email: "user@demo.local", password: "User123!" }));
  });

  it("sends the bearer token on protected calls", async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      jsonResponse({ userId: "1", email: "user@demo.local", displayName: "User", roles: ["User"] }),
    );
    vi.stubGlobal("fetch", fetchMock);

    const client = createApiClient({
      baseUrl: "http://api.test",
      getAccessToken: () => "access-token",
    });

    await client.me();

    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(new Headers(init.headers).get("Authorization")).toBe("Bearer access-token");
  });

  it("loads the document list from the API", async () => {
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse([]));
    vi.stubGlobal("fetch", fetchMock);

    const client = createApiClient({
      baseUrl: "",
      getAccessToken: () => "access-token",
    });

    await client.listDocuments();

    const [url] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toBe("/api/documents");
  });

  it("surfaces ProblemDetails and a 401 fallback", async () => {
    const problem = vi.fn().mockResolvedValue(
      jsonResponse({ title: "Unauthorized", detail: "Invalid credentials." }, 401),
    );
    vi.stubGlobal("fetch", problem);

    const client = createApiClient({ baseUrl: "", getAccessToken: () => null });
    await expect(client.login("user@demo.local", "nope")).rejects.toMatchObject({
      name: "ApiError",
      status: 401,
      message: "Invalid credentials.",
    } satisfies Partial<ApiError>);

    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("", { status: 401 })));
    await expect(client.me()).rejects.toMatchObject({
      status: 401,
      message: "Sign-in failed or the session is no longer valid.",
    });
  });
});

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}
