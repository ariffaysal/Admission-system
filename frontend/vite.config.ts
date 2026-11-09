import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/classes': 'http://localhost:8080',
      '/holds': 'http://localhost:8080',
      '/me': 'http://localhost:8080',
    },
  },
})
