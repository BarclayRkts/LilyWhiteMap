export interface TrafficSummary {
  totalViewsAllTime: number;
  totalViewsToday: number;
  totalViewsLast7Days: number;
  uniqueVisitorsToday: number;
  uniqueVisitorsLast7Days: number;
  uniqueVisitorsAllTime: number;
  averageResponseTimeMs: number;
}

export interface RecentActivityItem {
  id: number;
  timestampUtc: string;
  path: string;
  queryString: string | null;
  ipAddress: string | null;
  referrer: string | null;
  visitorId: string;
  responseTimeMs: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface RouteCount {
  path: string;
  views: number;
}

export interface ReferrerCount {
  referrer: string;
  views: number;
}
