/**
 * @file [...path].ts
 * @module Server.API.Proxy
 * @description
 * Universal proxy handler for forwarding /api/* requests to the .NET backend.
 * Implements the Backend for Frontend (BFF) pattern for authentication and API routing.
 *
 * Architecture:
 * 1. Browser makes requests to Nuxt at localhost:3000/api/*
 * 2. Nuxt proxies these to .NET backend at localhost:5105/*
 * 3. Browser receives cookies set for localhost:3000 domain
 * 4. Authentication redirects are passed through to the browser (not followed by proxy)
 *
 * @remarks
 * The X-Forwarded-* headers are critical for authentication flows.
 * .NET's ForwardedHeadersMiddleware reads these headers to determine the original
 * request host/port/scheme, which is used to generate correct OIDC redirect URIs.
 *
 * The redirect: 'manual' option prevents the proxy from following authentication
 * redirects, allowing the browser to handle them directly and preserve the user flow.
 *
 * Special handling for OIDC callbacks:
 * - signin-oidc and signout-callback-oidc paths preserve the /api/ prefix
 * - This matches the backend's CallbackPath configuration (/api/signin-oidc)
 */
import { proxyRequest } from 'h3';

/**
 * Handles all /api/* route requests and proxies them to the backend.
 *
 * Special path handling:
 * - OIDC callback paths (signin-oidc, signout-callback-oidc) preserve the /api/ prefix
 * - All other paths strip the /api/ prefix before forwarding
 *
 * @param event - H3 event object containing request details
 * @returns Proxied response from the backend API
 */
export default defineEventHandler(async (event) => {
  let path = event.path.replace('/api/', '');

  // For OIDC callbacks, send the full path including /api/ prefix
  // because the backend's CallbackPath is configured as /api/signin-oidc
  if (event.path.includes('signin-oidc') || event.path.includes('signout-callback-oidc')) {
    path = event.path.substring(1); // Remove leading slash, keep /api/
  }

  const apiUrl = process.env.services__server__http__0 || 'http://localhost:5105';
  const target = `${apiUrl}/${path}`;

  const originalHost = event.node.req.headers.host || 'localhost:3000';
  
  // Detect protocol from incoming request or environment
  // In production (Azure Container Apps), traffic comes through HTTPS
  const isProduction = process.env.NODE_ENV === 'production';
  
  // Check the original request's protocol
  // Azure Container Apps sets x-forwarded-proto header
  const incomingProto = event.node.req.headers['x-forwarded-proto'] as string;
  const forwardedProto = incomingProto || (isProduction ? 'https' : 'http');

  console.log('=== PROXY DEBUG ===');
  console.log('Original Path:', event.path);
  console.log('Forwarded Path:', path);
  console.log('Target:', target);
  console.log('Original Host:', originalHost);
  console.log('Method:', event.method);
  console.log('Environment:', isProduction ? 'Production' : 'Development');
  console.log('Forwarded Proto:', forwardedProto);
  console.log('Incoming x-forwarded-proto:', incomingProto);
  console.log('===================');

  // Make a test fetch first to see what backend returns
  if (path === 'weatherforecast') {
    try {
      const testResponse = await fetch(target, {
        method: 'GET',
        headers: {
          'X-Forwarded-Host': originalHost,
          'X-Forwarded-Proto': forwardedProto,
          'X-Forwarded-For': event.node.req.socket.remoteAddress || '::1',
        },
      });
      
      console.log('=== BACKEND RESPONSE DEBUG ===');
      console.log('Status:', testResponse.status);
      console.log('StatusText:', testResponse.statusText);
      console.log('Headers:', Object.fromEntries(testResponse.headers.entries()));
      
      const bodyText = await testResponse.text();
      console.log('Body (first 500 chars):', bodyText.substring(0, 500));
      console.log('==============================');
      
      // Return the response we captured
      return new Response(bodyText, {
        status: testResponse.status,
        headers: testResponse.headers,
      });
    } catch (err) {
      console.log('=== BACKEND FETCH ERROR ===');
      console.log('Error:', err);
      console.log('===========================');
    }
  }

  return proxyRequest(event, target, {
    fetchOptions: {
      redirect: 'manual',
    },
    headers: {
      'X-Forwarded-Host': originalHost,
      'X-Forwarded-Proto': forwardedProto,
      'X-Forwarded-For': event.node.req.socket.remoteAddress || '::1',
    },
  });
});
