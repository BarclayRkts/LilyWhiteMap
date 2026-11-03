/**
 * Client-persisted visitor identifier used to correlate page views in the
 * admin analytics dashboard. 
 */
const STORAGE_KEY = "lwm_visitor_id";

export function getOrCreateVisitorId(): string {
  if (typeof window === "undefined") {
    return "";
  }

  try {
    const existing = window.localStorage.getItem(STORAGE_KEY);
    if (existing) {
      return existing;
    }

    const id = typeof crypto.randomUUID === "function"
      ? crypto.randomUUID().replace(/-/g, "")
      : Math.random().toString(36).slice(2) + Date.now().toString(36);

    window.localStorage.setItem(STORAGE_KEY, id);
    return id;
  } catch {
    // localStorage can throw in private-browsing/blocked-storage contexts; tracking is best-effort.
    return "";
  }
}

export const VISITOR_ID_HEADER = "X-Visitor-Id";
