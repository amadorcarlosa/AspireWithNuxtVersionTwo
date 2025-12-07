// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  typescript: {
    tsConfig: {
      compilerOptions: {
        types: ["node"]
      }
    }
  },
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  // 1. Port Binding (Aspire 13.0 Integration)
  // Aspire automatically assigns a random port and sets the PORT environment variable.
  // Nuxt/Nitro picks this up automatically, but setting it explicitly is safer for debugging.
  devServer: {
   port: process.env.PORT ? parseInt(process.env.PORT) : 3000
  },

  // 2. Service Discovery & Proxying
  // Aspire 13.0 injects simplified variables like "API_HTTPS" when you use .WithReference(api)
  nitro: {
    routeRules: {
      '/api/**': {
        // This directs calls from your Vue frontend to your .NET backend
        // seamlessly in both Dev and Production (Docker).
        proxy: process.env.ApiUrl 
          ? `${process.env.ApiUrl}/**` 
          : 'http://localhost:5000/**' // Fallback if not running in Aspire
      }
    }
  }
})