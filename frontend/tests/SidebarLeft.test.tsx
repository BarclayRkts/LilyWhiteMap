import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { SidebarLeft } from "@/components/SidebarLeft";

describe("SidebarLeft", () => {
  it("shows the live player count and toggles a layer", () => {
    const onToggle = vi.fn();
    render(
      <SidebarLeft
        activeLayers={{ players: true, clubs: true, europe: false }}
        onToggle={onToggle}
        playerCount={1818}
      />,
    );

    expect(screen.getByText("1818")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: /European away nights/i }));
    expect(onToggle).toHaveBeenCalledWith("europe");
  });
});
