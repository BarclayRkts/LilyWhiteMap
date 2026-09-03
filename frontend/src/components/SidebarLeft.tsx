"use client";

import { layerMeta, LayerKey } from "@/data/map-data";

type SidebarLeftProps = {
  activeLayers: Record<LayerKey, boolean>;
  onToggle: (layer: LayerKey) => void;
  playerCount: number;
};

export function SidebarLeft({ activeLayers, onToggle, playerCount }: SidebarLeftProps) {
  return (
    <aside className="sidebar sidebar-left">
      <section>
        <div className="section-kicker">Layers</div>
        <div className="layer-list">
          {layerMeta.map((layer) => (
            <button className="layer-row" key={layer.key} onClick={() => onToggle(layer.key)} aria-pressed={activeLayers[layer.key]}>
              <span className="layer-label"><span className="color-dot" style={{ background: layer.color }} />{layer.label}</span>
              <span className="layer-actions"><span className="count">{layer.key === "players" && playerCount > 0 ? playerCount : layer.count}</span><span className={`switch ${activeLayers[layer.key] ? "on" : ""}`}><span /></span></span>
            </button>
          ))}
        </div>
      </section>
      <section className="archive-stats">
        <div className="section-kicker">Archive stats</div>
        <div className="stat-list">
          <div><span>Countries reached</span><strong>41</strong></div>
          <div><span>Continents</span><strong>6</strong></div>
          <div><span>Earliest record</span><strong>1908</strong></div>
          <div><span>Furthest club</span><strong className="highlight">Wellington, NZ</strong></div>
        </div>
      </section>
    </aside>
  );
}
