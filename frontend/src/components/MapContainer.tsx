"use client";

import "mapbox-gl/dist/mapbox-gl.css";
import { useEffect, useRef } from "react";
import Map, { Marker, NavigationControl, type MapRef } from "react-map-gl/mapbox";
import type { LayerKey, MapItem } from "@/data/map-data";

const TOKEN = process.env.NEXT_PUBLIC_MAPBOX_TOKEN ?? "";
const colors: Record<LayerKey, string> = { players: "#f5eedb", clubs: "#e5ae52", europe: "#59c98c" };
const lightModeColors: Record<LayerKey, string> = { players: "#1f3b5b", clubs: "#d0922a", europe: "#3b9b77" };

type MapContainerProps = {
  items: MapItem[];
  activeLayers: Record<LayerKey, boolean>;
  selectedId: string;
  theme: "dark" | "light";
  onSelect: (item: MapItem) => void;
};

export function MapContainer({ items, activeLayers, selectedId, theme, onSelect }: MapContainerProps) {
  const mapRef = useRef<MapRef | null>(null);
  const visibleItems = items.filter((item) => activeLayers[item.layer]);
  const selectedItem = items.find((item) => item.id === selectedId);

  const tintMap = () => {
    const map = mapRef.current?.getMap();
    if (!map) return;

    const paintUpdates = theme === "light"
      ? [
        ["water", "fill-color", "#dfeff9"],
        ["land", "background-color", "#eef5f5"],
        ["landcover", "fill-color", "#e7f0e4"],
        ["landuse", "fill-color", "#edf1f5"],
        ["admin-0-boundary", "line-color", "#b5c5d6"],
        ["admin-1-boundary", "line-color", "#c5d3df"],
      ] as const
      : [
        ["background", "background-color", "#08192b"],
        ["water", "fill-color", "#0b2138"],
        ["land", "background-color", "#0f2740"],
        ["landcover", "fill-color", "#122d3f"],
        ["landuse", "fill-color", "#0e2b44"],
        ["water-shadow", "fill-color", "#081e33"],
        ["admin-0-boundary", "line-color", "#31516a"],
        ["admin-1-boundary", "line-color", "#28465e"],
      ] as const;

    for (const [layerId, property, value] of paintUpdates) {
      if (map.getLayer(layerId)) {
        map.setPaintProperty(layerId, property, value);
      }
    }
  };

  useEffect(() => {
    if (!selectedItem || !mapRef.current) return;
    mapRef.current.flyTo({
      center: selectedItem.coordinates,
      zoom: selectedItem.layer === "players" ? 2.8 : 4,
      duration: 1200,
      essential: true,
    });
  }, [selectedItem]);

  return (
    <main className="map-panel">
      <Map
        ref={mapRef}
        mapboxAccessToken={TOKEN}
        initialViewState={{
          longitude: selectedItem?.coordinates[0] ?? 10,
          latitude: selectedItem?.coordinates[1] ?? 22,
          zoom: selectedItem ? (selectedItem.layer === "players" ? 2.8 : 4) : 1.15,
        }}
        mapStyle={theme === "light" ? "mapbox://styles/mapbox/light-v11" : "mapbox://styles/mapbox/dark-v11"}
        attributionControl={false}
        reuseMaps
        onLoad={tintMap}
      >
        <NavigationControl position="top-left" showCompass={false} />
        {visibleItems.map((item) => {
          const dotColor = theme === "light" ? lightModeColors[item.layer] : colors[item.layer];
          const boxShadow = theme === "light"
            ? `0 0 0 ${selectedId === item.id ? 4 : 2}px ${selectedId === item.id ? "rgba(31, 59, 91, 0.26)" : "rgba(31, 59, 91, 0.12)"}`
            : `0 0 0 ${selectedId === item.id ? 4 : 2}px ${selectedId === item.id ? "rgba(255,255,255,.24)" : "rgba(255,255,255,.1)"}`;
          return (
            <Marker key={item.id} longitude={item.coordinates[0]} latitude={item.coordinates[1]} anchor="center" onClick={(event) => { event.originalEvent.stopPropagation(); onSelect(item); }}>
              <button
                className={`map-pin ${selectedId === item.id ? "selected" : ""}`}
                style={{
                  background: dotColor,
                  boxShadow,
                }}
                aria-label={`Select ${item.name}`}
              />
            </Marker>
          );
        })}
      </Map>
      <div className="map-legend">
        <span><i style={{ background: theme === "light" ? lightModeColors.players : colors.players }} /> Player birthplace</span>
        <span><i style={{ background: theme === "light" ? lightModeColors.clubs : colors.clubs }} /> Supporters club</span>
        <span><i style={{ background: theme === "light" ? lightModeColors.europe : colors.europe }} /> European away night</span>
      </div>
      <div className="scale"><span /> 2,000 km</div>
    </main>
  );
}
