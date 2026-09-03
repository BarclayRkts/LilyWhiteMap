import type { MapItem } from "@/data/map-data";

export function SidebarRight({ selected }: { selected: MapItem }) {
  return (
    <aside className="sidebar sidebar-right">
      <div className="selected-label">Selected - {selected.layer === "players" ? "Player birthplace" : selected.layer === "clubs" ? "Supporters club" : "European away night"}</div>
      <div className="monogram" style={{ color: selected.accent }}>{selected.monogram}</div>
      <h1>{selected.name}</h1>
      <p className="location">{selected.location}</p>
      <div className="detail-stats">
        {selected.stats.map(([label, value]) => <div key={label}><span>{label}</span><strong>{value}</strong></div>)}
      </div>
    </aside>
  );
}
