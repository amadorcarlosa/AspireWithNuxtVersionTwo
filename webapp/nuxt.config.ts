/**
 * @file nuxt.config.ts
 * @module Configuration.Nuxt
 * @description
 * Nuxt 3 application configuration for .NET Aspire integration.
 * Configures runtime port binding, service discovery, and development environment settings.
 *
 * Key responsibilities:
 * - Port binding: Fixed port 3000 for development (isProxied: false in Aspire)
 * - Service discovery: Exposes backend API URL via runtime config
 * - Development security: Allows self-signed certificates in non-production environments
 *
 * @see https://nuxt.com/docs/api/configuration/nuxt-config
 */

// Allow self-signed certs in development (must be before any imports that use fetch)
if (process.env.NODE_ENV !== 'production') {
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
}

/**
 * Nuxt application configuration optimized for .NET Aspire orchestration.
 *
 * This configuration enables seamless integration with Aspire 13.0+ by:
 * 1. Binding to fixed port 3000 in development (matches AppHost isProxied: false)
 * 2. Providing runtime service discovery for backend APIs
 * 3. Supporting development workflows with self-signed certificates
 */
export default defineNuxtConfig({
  /**
   * TypeScript configuration for Node.js type support.
   * Ensures Node.js types are available for server-side code.
   */
  typescript: {
    tsConfig: {
      compilerOptions: {
        types: ['node'],
      },
    },
  },

  /** Nuxt compatibility date for stable feature set */
  compatibilityDate: '2025-07-15',

  /** Enable Vue DevTools for debugging and inspection */
  devtools: { enabled: true },

  /**
   * 1. Port Binding (Aspire 13.0 Integration)
   *
   * Fixed port for development with isProxied: false in AppHost.
   * Host 0.0.0.0 binds to all interfaces (IPv4 and IPv6) to ensure
   * accessibility from both localhost and 127.0.0.1.
   */
  devServer: {
    port: 3000,
    host: '0.0.0.0',
  },

  /**
   * 2. Service Discovery & Proxying
   *
   * Backend API URL configuration for runtime service discovery.
   * Handled by server middleware at runtime (server/api/[...path].ts).
   * This ensures ApiUrl environment variable is read at runtime, not build time.
   *
   * @property apiUrl - Backend API endpoint URL from Aspire service discovery
   */
  runtimeConfig: {
    apiUrl: process.env.ApiUrl || 'https://localhost:7275',
  },
});