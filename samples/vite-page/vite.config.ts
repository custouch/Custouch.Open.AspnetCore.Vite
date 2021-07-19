import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import legacy from '@vitejs/plugin-legacy'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue(),
    legacy({
      targets: ['defaults', 'not IE 11']
    })],
    base:"/static/",
  build: {
    // generate manifest.json in outDir
    manifest: true,
    rollupOptions: {
      // overwrite default .html entry
      input: '/src/main.ts'
    },
    emptyOutDir: true
  }
})
