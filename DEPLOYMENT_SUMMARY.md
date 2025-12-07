# Deployment & Configuration Summary

This document summarizes the changes made while preparing this Aspire + Nuxt project for Azure Container Apps deployment and CI/CD.

## Key Actions Completed

- Added Azure Container Apps support to the AppHost:
  - Called the environment API (`AddAzureContainerAppEnvironment("env")`) in the app host builder to enable Azure deployment hooks.

- Fixed local runtime errors and naming conflicts:
  - Removed incorrect `PublishAsAzureContainerApp(...)` usage and resolved a naming conflict with the `env` variable so build/publish runs do not throw `Unsupported value type System.Boolean`.

- Initialized azd and provisioned resources locally:
  - Ran `azd init` and `azd up` from the `apphost` folder.
  - Resources provisioned include a Resource Group, Azure Container Registry, Log Analytics workspace, and a Container Apps Environment.
  - Deployed the server (.NET) project; endpoints from the successful run:
    - API Endpoint: https://server.blackocean-a955d935.centralus.azurecontainerapps.io/
    - Aspire Dashboard: https://aspire-dashboard.ext.blackocean-a955d935.centralus.azurecontainerapps.io

- Configured CI/CD (GitHub Actions):
  - Ran `azd pipeline config` which created `.github/workflows/azure-dev.yml` and set up OIDC federated credentials and pipeline repo variables/secrets.
  - Committed and pushed the workflow to trigger the pipeline.

- Azure configuration files moved/committed to repo root for CI:
  - `azure.yaml` (moved to repo root and updated to point to `./apphost/apphost.csproj`).
  - `.azure/` directory (environment-specific config) was copied into repo root and committed.

- Frontend containerization & runtime proxy fix:
  - Added `webapp/Dockerfile` (multi-stage build for Nuxt/Nitro using `pnpm` and `node:22-alpine`).
  - Created a runtime server middleware `webapp/server/api/[...path].ts` to proxy `/api/*` requests to the API using `process.env.ApiUrl` at runtime (fixes the production-only proxy problem where build-time proxy config was empty).
  - Updated `webapp/nuxt.config.ts` to remove build-time proxy rule and rely on the runtime proxy middleware and `runtimeConfig.apiUrl`.

## Important Files Added / Modified

- `azure.yaml` (repo root) — maps the apphost project to Azure Container Apps
- `.azure/` — environment config files created by `azd`
- `.github/workflows/azure-dev.yml` — GitHub Actions workflow (created by `azd pipeline config`)
- `webapp/Dockerfile` — Dockerfile for building the Nuxt frontend image (multi-stage)
- `webapp/server/api/[...path].ts` — Nitro server route to proxy `/api/*` at runtime
- `webapp/nuxt.config.ts` — adjusted to use runtime configuration instead of a build-time proxy

## Why the runtime proxy fix was required

- Nuxt/Nitro `routeRules.proxy` is evaluated at build time, so `process.env.ApiUrl` was empty when the Docker image was built. That caused frontend requests to the API to fail in production (the component rendered but returned no data).
- The server middleware reads `process.env.ApiUrl` at request time and proxies calls to the deployed .NET API, ensuring the same behavior in both dev and production.

## Next Recommended Steps

1. Confirm the GitHub Actions workflow run completes successfully (Actions tab).
2. If you want the frontend traffic to be served from a single domain, consider adding a reverse-proxy or routing rules in Azure Container Apps or fronting with Azure Front Door.
3. Optionally update `apphost/AppHost.cs` to include the `AddDockerfile`/`AddNodeApp` registration for the frontend so the build/deploy pipeline automatically picks up the frontend as a service (if you prefer explicit resource mapping in `azure.yaml`).
4. Add monitoring/health checks to both the API and frontend Container Apps and configure alerts in Azure Monitor.

## Quick commands used during the process

```powershell
cd apphost
azd init
azd up
cd ..
azd pipeline config
git add azure.yaml .azure .github/workflows webapp/Dockerfile
git commit -m "Add azure config and frontend Dockerfile"
git push
```

---

If you'd like, I can now:
- Add `AddDockerfile("frontend", "../webapp")` to `apphost/AppHost.cs` and push that change so the frontend is part of the `azure.yaml` mapping, or
- Help tweak the GitHub Actions workflow to build and deploy both services explicitly.

File created by the assistant as requested: `DEPLOYMENT_SUMMARY.md`
