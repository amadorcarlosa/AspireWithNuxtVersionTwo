# syntax=docker/dockerfile:1

# Pinned base image for reproducible builds
ARG NODE_IMAGE=node:22-alpine@sha256:3404205afbfa99ffb663ec5ac28be64bd789541816885c75939c7d24dce06fa2

# ───────────────────────────────────────────────────────────────
# Build stage
# ───────────────────────────────────────────────────────────────
FROM ${NODE_IMAGE} AS builder

# Enable corepack for pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

WORKDIR /app

# Copy package manifests first (better layer caching)
COPY package.json pnpm-lock.yaml ./

# Install dependencies
RUN pnpm install --frozen-lockfile

# Copy source code
COPY . .

# Build the application
RUN pnpm build

# ───────────────────────────────────────────────────────────────
# Production stage
# ───────────────────────────────────────────────────────────────
FROM ${NODE_IMAGE} AS runner

WORKDIR /app

# Copy only the built output (minimal image)
COPY --from=builder /app/.output /app/.output

# Set environment variables
ENV NODE_ENV=production
ENV HOST=0.0.0.0
ENV PORT=3000

# Use nonroot user (node:22-alpine already creates uid 1000 'node')
USER node

# Expose the port
EXPOSE 3000

# Start the server
CMD ["node", ".output/server/index.mjs"]