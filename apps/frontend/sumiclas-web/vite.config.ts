import {defineConfig, loadEnv} from 'vite';
import react from '@vitejs/plugin-react';
import {fileURLToPath, URL} from 'node:url';

// https://vitejs.dev/config/
export default defineConfig(({mode}) => {
    const env = loadEnv(mode, process.cwd(), '');

    return {
        plugins: [react()],
        server: {
            port: 4004,
            strictPort: true,
            proxy: {
                "/api": {
                    target: env.API_PROXY_TARGET || "https://localhost:7064",
                    changeOrigin: true,
                    secure: false,
                    rewrite: (path: string) => path.replace(/^\/api/, "")
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
    };
})
