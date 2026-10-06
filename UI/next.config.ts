import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  // Lets the Playwright e2e build (.next-e2e) live alongside dev/prod builds.
  distDir: process.env.NEXT_DIST_DIR || ".next",
  env: {

  },
};

export default nextConfig;
