/**
 * @file [...path].ts
 * @module Server.API.Proxy
 * @description
 * Universal proxy handler for forwarding /api/* requests to the .NET backend.
 * Implements the Backend for Frontend (BFF) pattern for authentication and API routing.
 */

export default defineEventHandler(async (event) => {
  let path = event.path.replace('/api/', '');
  
  // For OIDC callbacks, preserve the /api/ prefix
  if (event.path.includes('signin-oidc') || event.path.includes('signout-callback-oidc')) {
    path = event.path.substring(1);
  }

  const apiUrl = process.env.services__server__http__0 || process.env.services__server__https__0 || 'http://localhost:5105';
  const target = `${apiUrl}/${path}`;
  
  // Use PUBLIC_HOSTNAME in production, fallback to request host in dev
  const publicHost = process.env.PUBLIC_HOSTNAME || event.node.req.headers.host || 'localhost:3000';
  const publicProto = process.env.PUBLIC_PROTO || (process.env.NODE_ENV === 'production' ? 'https' : 'http');

  console.log('=== PROXY DEBUG ===');
  console.log('Environment:', process.env.NODE_ENV);
  console.log('Target:', target);
  console.log('Public Host:', publicHost);
  console.log('Public Proto:', publicProto);
  console.log('Original Path:', event.path);
  console.log('Forwarded Path:', path);
  console.log('Method:', event.method);
  console.log('===================');

  // Make the proxied request
  const response = await fetch(target, {
    method: event.method,
    headers: {
      ...Object.fromEntries(
        Object.entries(event.node.req.headers)
          .filter(([key]) => !['host', 'connection'].includes(key.toLowerCase()))
          .map(([key, value]) => [key, Array.isArray(value) ? value.join(', ') : value || ''])
      ),
      'X-Forwarded-Host': publicHost,
      'X-Forwarded-Proto': publicProto,
      'X-Forwarded-For': event.node.req.socket.remoteAddress || '::1',
      'X-Public-Host': publicHost,      // ADD THIS - custom header Envoy won't touch
      'X-Public-Proto': publicProto,    // ADD THIS - custom header Envoy won't touch
      'Cookie': event.node.req.headers.cookie || '',
    },
    body: ['GET', 'HEAD'].includes(event.method!) ? undefined : await readRawBody(event),
    redirect: 'manual', // Don't follow redirects
  });

  // Copy status
  event.node.res.statusCode = response.status;

  // Copy headers, rewriting Location if needed
  response.headers.forEach((value, key) => {
    // Skip headers that shouldn't be forwarded
    if (['transfer-encoding', 'connection'].includes(key.toLowerCase())) {
      return;
    }

    // Rewrite Location header if it contains internal URL
    if (key.toLowerCase() === 'location') {
      let rewrittenLocation = value;
      
      // Replace internal Azure Container Apps URLs
      if (value.includes('.internal.') || value.includes('.azurecontainerapps.io')) {
        // Extract the path from the internal URL
        const internalUrlPattern = /https?:\/\/[^\/]*(?:\.internal\.[^\/]+|\.azurecontainerapps\.io[^\/]*)(\/.*)?/;
        const match = value.match(internalUrlPattern);
        
        if (match) {
          let backendPath = match[1] || '/';
          
          // Add /api/ prefix if the path doesn't already have it
          // Since all browser requests come through /api/* but backend paths don't have /api/
          if (!backendPath.startsWith('/api/') && !backendPath.startsWith('/api?')) {
            backendPath = '/api' + backendPath;
          }
          
          rewrittenLocation = `${publicProto}://${publicHost}${backendPath}`;
        }
        
        console.log('=== REWRITING LOCATION ===');
        console.log('Original:', value);
        console.log('Rewritten:', rewrittenLocation);
        console.log('==========================');
      }
      
      event.node.res.setHeader(key, rewrittenLocation);
      return;
    }

    event.node.res.setHeader(key, value);
  });

  // Return body
  if (response.body) {
    const arrayBuffer = await response.arrayBuffer();
    return new Uint8Array(arrayBuffer);
  }

  return null;
});