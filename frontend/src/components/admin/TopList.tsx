export function TopList({ title, items }: { title: string; items: { label: string; views: number }[] }) {
  return (
    <div className="rounded-xl border border-neutral-800 bg-neutral-900 p-5">
      <h3 className="mb-3 text-sm font-semibold text-white">{title}</h3>
      <ul className="space-y-2">
        {items.map((item) => (
          <li key={item.label} className="flex items-center justify-between text-sm">
            <span className="truncate pr-2 text-neutral-300" title={item.label}>{item.label}</span>
            <span className="shrink-0 font-mono text-neutral-400">{item.views}</span>
          </li>
        ))}
        {items.length === 0 && <li className="text-sm text-neutral-500">No data yet.</li>}
      </ul>
    </div>
  );
}
