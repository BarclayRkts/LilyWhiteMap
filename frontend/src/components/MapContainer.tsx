"use client";

import "mapbox-gl/dist/mapbox-gl.css";
import { useEffect, useRef } from "react";
import Map, { Marker, NavigationControl, type MapRef } from "react-map-gl/mapbox";
import type { LayerKey, MapItem } from "@/data/map-data";

const TOKEN = process.env.NEXT_PUBLIC_MAPBOX_TOKEN ?? "";
const colors: Record<LayerKey, string> = { players: "#f5eedb", clubs: "#e5ae52", europe: "#59c98c" };

type MapContainerProps = {
  items: MapItem[];
  activeLayers: Record<LayerKey, boolean>;
  selectedId: string;
  onSelect: (item: MapItem) => void;
};

export function MapContainer({ items, activeLayers, selectedId, onSelect }: MapContainerProps) {
  const mapRef = useRef<MapRef | null>(null);
  const visibleItems = items.filter((item) => activeLayers[item.layer]);
  const selectedItem = items.find((item) => item.id === selectedId);

  const tintMap = () => {
    const map = mapRef.current?.getMap();
    if (!map) return;

    const paintUpdates: Array<[string, string, unknown]> = [
      ["water", "fill-color", "#0b2138"],
      ["land", "background-color", "#122d45"],
      ["landcover", "fill-color", "#163651"],
      ["landuse", "fill-color", "#14324c"],
      ["admin-0-boundary", "line-color", "#31516a"],
      ["admin-1-boundary", "line-color", "#28465e"],
    ];

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
        mapStyle="mapbox://styles/mapbox/dark-v11"
        attributionControl={false}
        reuseMaps
        onLoad={tintMap}
      >
        <NavigationControl position="top-left" showCompass={false} />
        {visibleItems.map((item) => (
          <Marker key={item.id} longitude={item.coordinates[0]} latitude={item.coordinates[1]} anchor="center" onClick={(event) => { event.originalEvent.stopPropagation(); onSelect(item); }}>
            <button className={`map-pin ${selectedId === item.id ? "selected" : ""}`} style={{ background: colors[item.layer], boxShadow: `0 0 0 ${selectedId === item.id ? 4 : 2}px rgba(255,255,255,${selectedId === item.id ? ".24" : ".1"})` }} aria-label={`Select ${item.name}`} />
          </Marker>
        ))}
      </Map>
      <div className="map-legend">
        <span><i style={{ background: colors.players }} /> Player birthplace</span>
        <span><i style={{ background: colors.clubs }} /> Supporters club</span>
        <span><i style={{ background: colors.europe }} /> European away night</span>
      </div>
      <div className="scale"><span /> 2,000 km</div>
    </main>
  );
}
