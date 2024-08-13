/** @type {import('next').NextConfig} */
const nextConfig = {
    async rewrites() {
        return [
            {
                source: '/',
                destination: '/home'
            }
        ]
    },
    serverRuntimeConfig: {

    },
    output: 'standalone',
    env: {
        NEXTJS_APPINFO_ENV: process.env.NEXTJS_APPINFO_ENV,
        NEXTJS_APPINFO_VERSION: process.env.NEXTJS_APPINFO_VERSION
    }
}

module.exports = nextConfig
