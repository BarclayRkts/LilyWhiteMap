import { API_URL } from "./config";
import { getOrCreateVisitorId, VISITOR_ID_HEADER } from "./visitor";

/**
 * Wrapper around fetch() that talks to the .NET backend, always including:
 * - credentials, so the backend's HttpOnly visitor-id cookie is sent/received, and
 * - the X-Visitor-Id header, as a cookie-policy-resilient fallback for traffic analytics.
 */
export function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers);
  const visitorId = getOrCreateVisitorId();
  if (visitorId) {
    headers.set(VISITOR_ID_HEADER, visitorId);
  }

  return fetch(`${API_URL}${path}`, {
    ...init,
    credentials: "include",
    headers,
  });
}

const ADMIN_TOKEN_KEY = "lwm_admin_token";

export function getAdminToken(): string | null {
  if (typeof window === "undefined") return null;
  return window.localStorage.getItem(ADMIN_TOKEN_KEY);
}

export function setAdminToken(token: string): void {
  window.localStorage.setItem(ADMIN_TOKEN_KEY, token);
}

export function clearAdminToken(): void {
  window.localStorage.removeItem(ADMIN_TOKEN_KEY);
}

/** Same as apiFetch, but attaches the admin bearer token. Throws if no token is present. */
export function adminFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const token = getAdminToken();
  if (!token) {
    throw new Error("Not authenticated");
  }

  const headers = new Headers(init.headers);
  headers.set("Authorization", `Bearer ${token}`);

  return apiFetch(path, { ...init, headers });
}
