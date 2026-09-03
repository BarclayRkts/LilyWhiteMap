import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { Navbar } from "@/components/Navbar";

describe("Navbar", () => {
  it("updates the search query and changes views", () => {
    const onQueryChange = vi.fn();
    const onViewChange = vi.fn();
    const onThemeToggle = vi.fn();
    render(
      <Navbar
        view="map"
        onViewChange={onViewChange}
        query=""
        onQueryChange={onQueryChange}
        theme="dark"
        onThemeToggle={onThemeToggle}
      />,
    );

    fireEvent.change(screen.getByRole("textbox"), { target: { value: "England" } });
    fireEvent.click(screen.getByRole("button", { name: /list/i }));

    expect(onQueryChange).toHaveBeenCalledWith("England");
    expect(onViewChange).toHaveBeenCalledWith("list");
  });
});
