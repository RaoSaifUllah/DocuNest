import { loadEnv } from 'vite';
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), 'DOCUNEST_');
  return {
    plugins: [react(), tailwindcss()],
    server: {
      strictPort: true,
      port: 5173,
      proxy: {
        '/api': { target: env.DOCUNEST_API_TARGET ?? 'http://127.0.0.1:5080' },
        '/health': { target: env.DOCUNEST_API_TARGET ?? 'http://127.0.0.1:5080' },
      },
    },
    build: { sourcemap: false },
    test: {
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      include: ['src/**/*.test.{ts,tsx}'],
      clearMocks: true,
    },
  };
});
