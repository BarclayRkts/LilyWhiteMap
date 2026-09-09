"use client";

import { useEffect, useMemo, useState } from "react";
import { mapItems, LayerKey, getTottenhamYears } from "@/data/map-data";
import { API_URL } from "@/lib/config";
import { Navbar } from "./Navbar";
import { SidebarLeft } from "./SidebarLeft";
import { SidebarRight } from "./SidebarRight";
import { MapContainer } from "./MapContainer";

export function DashboardLayout() {
  const [players, setPlayers] = useState<typeof mapItems>([]);
  const [apiError, setApiError] = useState<string | null>(null);
  const [view, setView] = useState<"map" | "list">("map");
  const [theme, setTheme] = useState<"dark" | "light">("dark");
  const [query, setQuery] = useState("");
  const [activeLayers, setActiveLayers] = useState<Record<LayerKey, boolean>>({ players: true, clubs: true, europe: true });
  const [selectedId, setSelectedId] = useState("harry-kane");

  useEffect(() => {
    const savedTheme = window.localStorage.getItem("lilywhite-theme");
    if (savedTheme === "light" || savedTheme === "dark") {
      setTheme(savedTheme);
    }
  }, []);

  useEffect(() => {
    window.localStorage.setItem("lilywhite-theme", theme);
  }, [theme]);
  const allItems = useMemo(() => [...players, ...mapItems.filter((item) => item.layer !== "players")], [players]);
  const selected = allItems.find((item) => item.id === selectedId) ?? allItems[0];
  const filteredItems = useMemo(() => allItems
    .filter((item) => `${item.name} ${item.location}`.toLowerCase().includes(query.toLowerCase()) && activeLayers[item.layer])
    .sort((a, b) => {
      const aUnavailable = /unavailable/i.test(a.location);
      const bUnavailable = /unavailable/i.test(b.location);
      return Number(aUnavailable) - Number(bUnavailable);
    }), [allItems, query, activeLayers]);

  useEffect(() => {
    fetch(`${API_URL}/players`)
      .then((response) => {
        if (!response.ok) throw new Error(`Player service returned ${response.status}`);
        return response.json();
      })
      .then((apiPlayers: Array<{ id: string; name: string; location: string; longitude: number; latitude: number; monogram: string; position: string; headshot: string | null; isEstimatedLocation: boolean; years?: string | null }>) => {
        const playerItems = apiPlayers.map((player) => ({
          id: `player-${player.id}`,
          layer: "players" as const,
          name: player.name,
          location: player.location,
          coordinates: [player.longitude || -0.0166, player.latitude || 51.6043] as [number, number],
          monogram: player.monogram,
          accent: "#f5eedb",
          stats: [["Position", player.position], ["Tottenham years", player.years ?? getTottenhamYears(player.name)], ["Source", "THFCDB archive"], ["Club", "Tottenham Hotspur"], ["Record", "All-time archive"]] as Array<[string, string]>,
          description: `${player.name} is listed in the all-time Tottenham Hotspur people archive from THFCDB. The source provides country data; this marker uses a stable map position until a verified birthplace coordinate is available.`,
        }));
        setPlayers(playerItems);
        if (playerItems.length > 0) {
          setSelectedId((current) => playerItems.some((player) => player.id === current)
            ? current
            : playerItems.find((player) => player.id === "player-harry-kane")?.id ?? playerItems[0].id);
        }
      })
      .catch((error: Error) => setApiError(error.message));
  }, []);

  const toggleLayer = (layer: LayerKey) => setActiveLayers((current) => ({ ...current, [layer]: !current[layer] }));
  const chooseItem = (id: string) => { setSelectedId(id); setView("map"); };

  return (
    <div className={`dashboard ${theme === "light" ? "theme-light" : ""}`}>
      <Navbar
        view={view}
        onViewChange={setView}
        query={query}
        onQueryChange={setQuery}
        theme={theme}
        onThemeToggle={() => setTheme((current) => current === "dark" ? "light" : "dark")}
      />
      <div className="dashboard-grid">
        <SidebarLeft activeLayers={activeLayers} onToggle={toggleLayer} playerCount={players.length} />
        {view === "map" ? <MapContainer items={filteredItems} activeLayers={activeLayers} selectedId={selected.id} theme={theme} onSelect={(item) => setSelectedId(item.id)} /> : (
          <main className="list-panel">
            <div className="section-kicker">Archive index</div>
            {apiError && <p className="api-status">Live player data unavailable: {apiError}</p>}
            <div className="archive-list">
              {filteredItems.map((item) => <button key={item.id} onClick={() => chooseItem(item.id)}><span className="color-dot" style={{ background: item.accent }} /><span><strong>{item.name}</strong><small className="location-label" data-unavailable={/unavailable/i.test(item.location) || undefined}>{item.location}</small></span><span className="list-arrow">View</span></button>)}
              {filteredItems.length === 0 && <p className="empty">No archive entries match your search.</p>}
            </div>
          </main>
        )}
        <SidebarRight selected={selected} />
      </div>
    </div>
  );
}
