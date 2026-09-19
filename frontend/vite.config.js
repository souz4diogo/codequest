import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Porta 5173 = origem liberada no CORS do backend (ver Program.cs).
export default defineConfig({
  plugins: [react()],
  server: { port: 5173 },
  test: {
    environment: "jsdom",
    setupFiles: "./src/tests/setup.js",
    globals: true,
  },
});
