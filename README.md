# AnimeTracker

Plataforma para gestionar el consumo de anime, con auto-tracking mediante extensión de navegador y calendario de emisiones en tiempo real.

---

## Requisitos previos

| Herramienta | Versión mínima | Notas |
|-------------|---------------|-------|
| **Node.js** | 24.x LTS | Usar `.nvmrc` con `nvm use` |
| **.NET 8 SDK** | 8.0.x | Ver `global.json` para la versión exacta |
| **Docker** | 27.x | Incluye Docker Compose v2 |
| **pnpm** | 12.x | `npm install -g pnpm` |

> El archivo `.nvmrc` en la raíz fija la versión de Node LTS.  
> El archivo `global.json` en la raíz fija la versión del .NET 8 SDK.

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

## Licencia

Privado — todos los derechos reservados.
