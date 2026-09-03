"use client";

import { List, Map, Moon, Search, Shield, SunMedium } from "lucide-react";

type NavbarProps = {
  view: "map" | "list";
  onViewChange: (view: "map" | "list") => void;
  query: string;
  onQueryChange: (query: string) => void;
  theme: "dark" | "light";
  onThemeToggle: () => void;
};

export function Navbar({ view, onViewChange, query, onQueryChange, theme, onThemeToggle }: NavbarProps) {
  return (
    <header className="dashboard-header">
      <div className="brand">
        <div className="brand-mark"><Shield size={18} strokeWidth={1.8} /></div>
        <div>
          <div className="brand-name">The Lilywhite Map</div>
          <div className="brand-subtitle">Global footprint archive</div>
        </div>
      </div>
      <label className="search-bar">
        <Search size={14} />
        <input
          value={query}
          onChange={(event) => onQueryChange(event.target.value)}
          placeholder="Search players, countries, or clubs"
          aria-label="Search players, countries, or clubs"
        />
      </label>
      <div className="header-actions">
        <button className="theme-toggle" type="button" onClick={onThemeToggle} aria-label={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}>
          {theme === "dark" ? <SunMedium size={14} /> : <Moon size={14} />}
          <span>{theme === "dark" ? "Light" : "Dark"}</span>
        </button>
        <div className="view-toggle" aria-label="View selection">
          <button className={view === "map" ? "active" : ""} onClick={() => onViewChange("map")}><Map size={13} /> Map</button>
          <button className={view === "list" ? "active" : ""} onClick={() => onViewChange("list")}><List size={13} /> List</button>
        </div>
      </div>
    </header>
  );
}
