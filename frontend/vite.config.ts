import { defineConfig } from 'vite'
import path from "path";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { // remember update tsconfig.node.json, tsconfig.app.json
      "@dn": path.resolve(__dirname, "./src"),
      "@dn-shared": path.resolve(__dirname, "./src/shared"),
      "@dn-services": path.resolve(__dirname, "./src/services"),
      "@dn-pages": path.resolve(__dirname, "./src/pages"),
    },
  },
});
