"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { adminFetch, clearAdminToken, getAdminToken } from "@/lib/api";
import type {
  TrafficSummary,
  PagedResult,
  RecentActivityItem,
  RouteCount,
  ReferrerCount,
} from "@/lib/analytics-types";
import { MetricCard } from "@/components/admin/MetricCard";
import { RecentActivityTable } from "@/components/admin/RecentActivityTable";
import { TopList } from "@/components/admin/TopList";

const PAGE_SIZE = 25;

export default function AdminDashboardPage() {
  const router = useRouter();
  const [authChecked, setAuthChecked] = useState(false);
  const [summary, setSummary] = useState<TrafficSummary | null>(null);
  const [recent, setRecent] = useState<PagedResult<RecentActivityItem> | null>(null);
  const [topRoutes, setTopRoutes] = useState<RouteCount[]>([]);
  const [topReferrers, setTopReferrers] = useState<ReferrerCount[]>([]);
  const [page, setPage] = useState(1);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async (pageToLoad: number) => {
    try {
      const [summaryRes, recentRes, routesRes, referrersRes] = await Promise.all([
        adminFetch("/admin/analytics/summary"),
        adminFetch(`/admin/analytics/recent?page=${pageToLoad}&pageSize=${PAGE_SIZE}`),
        adminFetch("/admin/analytics/top-routes?take=10"),
        adminFetch("/admin/analytics/top-referrers?take=10"),
      ]);

      if (summaryRes.status === 401 || recentRes.status === 401) {
        clearAdminToken();
        router.push("/admin/login");
        return;
      }

      if (!summaryRes.ok || !recentRes.ok || !routesRes.ok || !referrersRes.ok) {
        throw new Error("One or more analytics requests failed.");
      }

      setSummary(await summaryRes.json());
      setRecent(await recentRes.json());
      setTopRoutes(await routesRes.json());
      setTopReferrers(await referrersRes.json());
      setError(null);
    } catch {
      setError("Failed to load analytics. Retrying shortly...");
    }
  }, [router]);

  useEffect(() => {
    if (!getAdminToken()) {
      router.push("/admin/login");
      return;
    }
    setAuthChecked(true);
  }, [router]);

  useEffect(() => {
    if (!authChecked) return;
    void loadData(page);
    const interval = setInterval(() => {
      void loadData(page);
    }, 30_000);
    return () => clearInterval(interval);
  }, [authChecked, page, loadData]);

  if (!authChecked) {
    return null;
  }

  const totalPages = recent ? Math.max(1, Math.ceil(recent.totalCount / PAGE_SIZE)) : 1;

  return (
    <main className="min-h-screen bg-neutral-950 px-6 py-8 text-neutral-100">
      <div className="mx-auto max-w-6xl space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold">Traffic Analytics</h1>
          <button
            onClick={() => {
              clearAdminToken();
              router.push("/admin/login");
            }}
            className="rounded-md border border-neutral-700 px-3 py-1.5 text-sm text-neutral-300 hover:bg-neutral-800"
          >
            Sign Out
          </button>
        </div>

        {error && <p className="text-sm text-red-400">{error}</p>}

        <section className="grid grid-cols-2 gap-4 md:grid-cols-4">
          <MetricCard label="Views Today" value={summary?.totalViewsToday ?? "—"} />
          <MetricCard label="Views (7 days)" value={summary?.totalViewsLast7Days ?? "—"} />
          <MetricCard label="Views All-Time" value={summary?.totalViewsAllTime ?? "—"} />
          <MetricCard label="Avg Response" value={summary ? `${summary.averageResponseTimeMs}ms` : "—"} />
          <MetricCard label="Unique Visitors Today" value={summary?.uniqueVisitorsToday ?? "—"} />
          <MetricCard label="Unique Visitors (7 days)" value={summary?.uniqueVisitorsLast7Days ?? "—"} />
          <MetricCard label="Unique Visitors All-Time" value={summary?.uniqueVisitorsAllTime ?? "—"} />
          <MetricCard label="Bot/Scan Requests (7 days)" value={summary?.suspiciousRequestsLast7Days ?? "—"} />
        </section>

        <section className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <TopList title="Top Routes" items={topRoutes.map((r) => ({ label: r.path, views: r.views }))} />
          <TopList title="Top Referrers" items={topReferrers.map((r) => ({ label: r.referrer, views: r.views }))} />
        </section>

        <section>
          <h2 className="mb-3 text-lg font-semibold">Recent Activity</h2>
          <RecentActivityTable
            items={recent?.items ?? []}
            page={page}
            totalPages={totalPages}
            onPageChange={setPage}
          />
        </section>
      </div>
    </main>
  );
}
