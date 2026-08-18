import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Forward all /api requests to the .NET backend during development.
      // This avoids CORS issues — the browser sees everything on localhost:5173.
      '/api': {
        target: 'http://localhost:5108',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
