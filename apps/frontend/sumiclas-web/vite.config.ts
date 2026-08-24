import {defineConfig} from 'vite';
import react from '@vitejs/plugin-react';
import {fileURLToPath, URL} from 'node:url';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    server: {
        port: 4004,
        strictPort: true,
        proxy: {
            "/api/": {
                target: "http://localhost:4000",
                changeOrigin: true,
                secure: true
            }
        }
    },
    optimizeDeps: {
        exclude: ['lucide-react'],
    },
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
    }
})