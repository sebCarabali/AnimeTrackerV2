# Build context: repo root (needs pnpm-workspace.yaml and contracts/ for gen:core-client).
# docker build -f apps/bff/Dockerfile -t animetracker-bff .

FROM node:24-alpine AS base
RUN corepack enable
WORKDIR /repo

FROM base AS deps
COPY pnpm-workspace.yaml pnpm-lock.yaml package.json ./
COPY apps/bff/package.json apps/bff/package.json
RUN pnpm install --filter @animetracker/bff --frozen-lockfile

FROM deps AS build
COPY contracts/ contracts/
COPY apps/bff apps/bff
RUN pnpm --filter @animetracker/bff run build

FROM base AS prod-deps
COPY pnpm-workspace.yaml pnpm-lock.yaml package.json ./
COPY apps/bff/package.json apps/bff/package.json
RUN pnpm install --filter @animetracker/bff --frozen-lockfile --prod

FROM base AS runtime
ENV NODE_ENV=production
COPY --from=prod-deps /repo/apps/bff/node_modules apps/bff/node_modules
COPY --from=prod-deps /repo/node_modules node_modules
COPY --from=build /repo/apps/bff/dist apps/bff/dist
COPY apps/bff/package.json apps/bff/package.json

WORKDIR /repo/apps/bff
USER node
EXPOSE 3000
CMD ["node", "dist/main.js"]
