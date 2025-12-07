// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  typescript: {
    tsConfig: {
      compilerOptions: {
        types: ['node'],
      },
    },
  },
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  // 1. Port Binding (Aspire 13.0 Integration)
  // Aspire automatically assigns a random port and sets the PORT environment variable.
  // Nuxt/Nitro picks this up automatically, but setting it explicitly is safer for debugging.
  devServer: {
    port: process.env.PORT ? parseInt(process.env.PORT) : 3000,
  },

  // 2. Service Discovery & Proxying
  // Handled by server middleware at runtime (server/api/[...path].ts)
  // This ensures ApiUrl environment variable is read at runtime, not build time
  // Make ApiUrl available at runtime
  runtimeConfig: {
    apiUrl: process.env.ApiUrl || 'http://localhost:5000',
  },
});
