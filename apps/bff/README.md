# apps/bff — NestJS BFF

**Propósito:** Backend For Frontend construido con NestJS. Actúa como API Gateway y agregador entre la SPA/extensión y el núcleo .NET. Maneja la sesión de usuario y adapta los payloads para cada cliente.

**Dueño:** Backend team

## Stack

- NestJS (Node.js + TypeScript)
- pnpm

## Arranque local

```bash
# Desde la raíz del monorepo
pnpm install
pnpm --filter @animetracker/bff dev
```

Ver el README raíz para instrucciones completas del entorno local.
