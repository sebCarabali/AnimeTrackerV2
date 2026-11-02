# apps/spa — Angular SPA

**Propósito:** Aplicación de página única construida con Angular. Provee la interfaz de usuario principal: gestión de la biblioteca personal, dashboard, calendario de emisiones y búsqueda del catálogo.

**Dueño:** Frontend team

## Stack

- Angular 22 (standalone components, enrutamiento, SCSS)
- TypeScript estricto
- RxJS
- Vitest (test runner por defecto de `@angular/build:unit-test`)
- Cliente HTTP tipado generado con [ng-openapi-gen](https://github.com/cyclosproject/ng-openapi-gen) desde `contracts/public/bff.v1.yaml`

## Arranque local

```bash
# Desde la raíz del monorepo
pnpm install
pnpm --filter @animetracker/spa run gen:api   # genera el cliente tipado (no se versiona)
pnpm --filter @animetracker/spa run start     # ng serve
```

## Scripts

| Script | Descripción |
|---|---|
| `start` | `ng serve` |
| `build` | `ng build` (producción por defecto) |
| `lint` | `ng lint` (angular-eslint) |
| `test` | `ng test` |
| `test:coverage` | `ng test --coverage` (umbral 80 % líneas/statements/funciones/branches, ver `angular.json`) |
| `gen:api` | Regenera los servicios Angular tipados en `src/app/core/api/generated` desde `contracts/public/bff.v1.yaml` |

## Estructura

```
src/app/
  app.ts, app.routes.ts, app.config.ts   Raíz, rutas y configuración de la aplicación
  layout/shell/                          Layout con navegación principal (Shell)
  pages/                                 Páginas con carga diferida (dashboard, biblioteca, buscar, calendario, not-found)
  core/api/generated/                    Cliente tipado generado (gitignored, se regenera con `gen:api`)
```

La URL base del cliente HTTP (`ApiConfiguration.rootUrl`) se inyecta desde `src/environments/environment*.ts` (`apiBaseUrl`), nunca queda fija en el código generado.

Ver el README raíz para instrucciones completas del entorno local.
