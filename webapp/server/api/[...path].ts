export default defineEventHandler(async (event) => {
  const path = event.path.replace('/api/', '');
  const apiUrl = process.env.ApiUrl || 'http://localhost:5000';

  const target = `${apiUrl}/${path}`;

  return proxyRequest(event, target);
});
