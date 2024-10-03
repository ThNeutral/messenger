import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // '/tasks': {
      //   target: 'http://localhost:3000',
      //   changeOrigin: true
      // },
      // '/register': 'http://localhost:3000'
      
    }
  }
})
