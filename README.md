# AnimeTracker

Plataforma para gestionar el consumo de anime, con auto-tracking mediante extensión de navegador y calendario de emisiones en tiempo real.

---

## Requisitos previos

| Herramienta | Versión mínima | Notas |
|-------------|---------------|-------|
| **Node.js** | 24.x LTS | Usar `.nvmrc` con `nvm use` |
| **.NET 10 SDK** | 10.0.x | Ver `global.json` para la versión exacta |
| **Docker** | 27.x | Incluye Docker Compose v2 |
| **pnpm** | 12.x | `npm install -g pnpm` |

> El archivo `.nvmrc` en la raíz fija la versión de Node LTS.  
> El archivo `global.json` en la raíz fija la versión del .NET 10 SDK.

---

## Estructura del monorepo

```
animetracker/
├── apps/
│   ├── spa/          # Angular SPA — interfaz de usuario principal
│   ├── bff/          # NestJS BFF — API Gateway y agregador
│   └── extension/    # Extensión de navegador — auto-tracker
├── services/
│   └── core/         # .NET 8 Monolito Modular — lógica de negocio
├── contracts/        # Contratos OpenAPI (API-first)
├── infra/            # Docker Compose, Kubernetes, scripts de CI/CD
└── tools/
    └── repo-checks/  # Pruebas de estructura del repositorio
```

---

## Cómo arrancar el entorno local

### 1. Clonar e instalar dependencias JS

```bash
git clone <url-del-repo>
cd animetracker

# Seleccionar la versión de Node correcta (requiere nvm)
nvm use

# Instalar todas las dependencias del workspace
pnpm install
```

### 2. Levantar los servicios de infraestructura

```bash
docker compose -f infra/compose/docker-compose.local.yml up -d
```

Esto inicia PostgreSQL y Redis en contenedores locales.

### 3. Arrancar el backend .NET

```bash
cd services/core
dotnet restore
dotnet run --project src/AnimeTracker.Api
```

El API estará disponible en `https://localhost:5001`.

### 4. Arrancar el BFF

```bash
pnpm --filter @animetracker/bff dev
```

### 5. Arrancar la SPA

```bash
pnpm --filter @animetracker/spa dev
```

La SPA estará disponible en `http://localhost:4200`.

---

## Convención de ramas

| Rama | Uso |
|------|-----|
| `main` | Rama principal estable; siempre deployable a staging |
| `feat/<scope>/<descripción>` | Nueva funcionalidad |
| `fix/<scope>/<descripción>` | Corrección de errores |
| `chore/<descripción>` | Tareas de mantenimiento |

Ejemplos: `feat/bff/health-check`, `fix/core/tracking-race-condition`

---

## Convención de commits — Conventional Commits

Todos los commits deben seguir el formato [Conventional Commits](https://www.conventionalcommits.org/):

```
<tipo>(<ámbito>): <descripción corta en imperativo>

[cuerpo opcional]

[pie opcional: BREAKING CHANGE, refs, closes]
```

### Tipos permitidos

| Tipo | Cuándo usarlo |
|------|---------------|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de un bug |
| `docs` | Solo documentación |
| `style` | Formato; sin cambio de lógica |
| `refactor` | Refactor sin feat ni fix |
| `test` | Añadir o corregir pruebas |
| `chore` | Mantenimiento (build, deps, CI) |
| `ci` | Cambios en la configuración de CI |
| `perf` | Mejora de rendimiento |

### Ámbitos permitidos

`spa` · `bff` · `ext` · `core` · `contracts` · `infra` · `ci` · `docs`

### Ejemplos válidos

```
feat(bff): agrega health check endpoint
fix(core): corrige race condition en tracking module
chore(ci): configura GitHub Actions para monorepo
docs: actualiza instrucciones de arranque en README
```

> El hook de Git instalado por Husky rechazará automáticamente los commits que no sigan este formato.

---

## Pruebas de estructura

```bash
cd tools/repo-checks
pnpm test
```

---

## CI — Integración continua

El pipeline corre en **GitHub Actions** (`.github/workflows/ci.yml`) en cada PR hacia `main`.

### Cómo funciona

1. El job `detect` calcula los proyectos afectados por los archivos cambiados usando `tools/ci/affected.ts`.
2. Se lanza una matriz de jobs (`run-job`) solo para los proyectos afectados.
3. El job `ci-ok` agrega el resultado; es el **único check requerido** en la protección de rama.

### Reproducir cada job localmente

| Job | Comando |
|-----|---------|
| `repo-checks` | `pnpm --filter @animetracker/repo-checks test` |
| `core` | `cd services/core && dotnet test AnimeTracker.sln` |
| `core` (cobertura) | `cd services/core && dotnet test AnimeTracker.sln --collect:"XPlat Code Coverage"` |
| `bff` | `pnpm --filter @animetracker/bff test` *(disponible en HU-00.03)* |
| `spa` | `pnpm --filter @animetracker/spa test` *(disponible en HU-00.04)* |
| `extension` | `pnpm --filter @animetracker/extension test` *(disponible en HU-00.05)* |
| Afectados (local) | `cd tools/ci && node --input-type=module -e "import {affectedJobs} from './affected.js'; console.log(affectedJobs(process.argv.slice(1)))"` |

### Cobertura

- Umbral: **80 % de líneas en código nuevo** (sin umbral global inicial).
- El reporte de cobertura del job `core` se sube como artefacto `coverage-core`.

### Protección de rama `main` (configurar en GitHub una vez)

> Settings → Branches → Add rule `main`
> - Required status check: `ci-ok`
> - Require branches to be up to date: ✓
> - Require 1 approving review: ✓
> - Include administrators: ✓

---

## Licencia

Privado — todos los derechos reservados.
