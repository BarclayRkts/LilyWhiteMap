"use client";

import { useEffect } from "react";
import { getOrCreateVisitorId } from "@/lib/visitor";

/**
 * Mounted once in the root layout. Ensures an anonymized visitor id exists in
 * localStorage as early as possible so it's available for the first API call.
 * Renders nothing.
 */
export function VisitorTracker() {
  useEffect(() => {
    getOrCreateVisitorId();
  }, []);

  return null;
}
