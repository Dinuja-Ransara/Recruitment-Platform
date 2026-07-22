import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// The dev server proxies /api to the ASP.NET Core backend so the browser sees a
// single origin during development. CORS is still configured on the API for the
// production build, where the two are served separately.
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5138',
        changeOrigin: true,
      },
    },
  },
})
