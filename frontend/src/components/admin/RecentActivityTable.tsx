import type { RecentActivityItem } from "@/lib/analytics-types";

export function RecentActivityTable({
  items,
  page,
  totalPages,
  onPageChange,
}: {
  items: RecentActivityItem[];
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}) {
  return (
    <div className="rounded-xl border border-neutral-800 bg-neutral-900">
      <div className="overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-neutral-800 text-neutral-400">
            <tr>
              <th className="px-4 py-3 font-medium">Time (UTC)</th>
              <th className="px-4 py-3 font-medium">Path</th>
              <th className="px-4 py-3 font-medium">Visitor</th>
              <th className="px-4 py-3 font-medium">IP</th>
              <th className="px-4 py-3 font-medium">Referrer</th>
              <th className="px-4 py-3 font-medium">Response</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.id} className="border-b border-neutral-800/60 text-neutral-200">
                <td className="whitespace-nowrap px-4 py-2">{new Date(item.timestampUtc).toLocaleString()}</td>
                <td className="px-4 py-2 font-mono text-xs">{item.path}{item.queryString}</td>
                <td className="px-4 py-2 font-mono text-xs text-neutral-400">{item.visitorId.slice(0, 8)}</td>
                <td className="px-4 py-2 text-neutral-400">{item.ipAddress ?? "—"}</td>
                <td className="max-w-[220px] truncate px-4 py-2 text-neutral-400">{item.referrer ?? "—"}</td>
                <td className="px-4 py-2 text-neutral-400">{item.responseTimeMs}ms</td>
              </tr>
            ))}
            {items.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-6 text-center text-neutral-500">
                  No activity recorded yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between border-t border-neutral-800 px-4 py-3 text-sm text-neutral-400">
        <span>Page {page} of {Math.max(totalPages, 1)}</span>
        <div className="space-x-2">
          <button
            onClick={() => onPageChange(page - 1)}
            disabled={page <= 1}
            className="rounded-md border border-neutral-700 px-3 py-1 disabled:opacity-40"
          >
            Prev
          </button>
          <button
            onClick={() => onPageChange(page + 1)}
            disabled={page >= totalPages}
            className="rounded-md border border-neutral-700 px-3 py-1 disabled:opacity-40"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}
