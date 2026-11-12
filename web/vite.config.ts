import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Dev only: forward API calls from :5173 to the ASP.NET API (http profile).
    proxy: {
      '/health': 'http://localhost:5166',
    },
  },
})
