import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  base: '/frontend/',
  server: {
    proxy: {
      '/backend/api': {
        target: 'http://localhost:5071',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/backend\/api/, '/api')
      }
    }
  },
  build: {
    outDir: 'dist'
  }
})
