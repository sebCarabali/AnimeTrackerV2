# HU-00.04 · SPA Angular inicial

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-4-spa-angular-inicial` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 3 pts |
| Sprint | sprint-2 |
| Estado | done |
| Depende de | HU-00.01, HU-00.07, HU-00.09 |
| Bloquea | EP-01 (UI), EP-02 (UI), EP-03 (UI), EP-04, EP-05 (UI) |

## Historia

**Como** desarrollador frontend, **quiero** una aplicación Angular con enrutamiento, layout base y cliente API generado, **para** construir las pantallas sobre una base común.

## Contexto

- ARD §2: Angular con servicios reactivos (RxJS) y TypeScript estricto.
- ARD §6: la SPA aplica el desfase horario con pipes; el layout debe considerar desde ya la configuración de locale.
- El cliente de la API pública se genera desde `contracts/public`.

## Criterios de aceptación

```gherkin
Característica: SPA Angular base

  Escenario: CA1 — Layout con navegación principal
    Dado la SPA en ejecución con "ng serve"
    Cuando abro la raíz "/"
    Entonces se redirige a "/dashboard"
    Y veo un layout con enlaces a Biblioteca, Buscar, Calendario y Dashboard

  Escenario: CA2 — Rutas con páginas vacías y carga diferida
    Dado el layout principal
    Cuando navego a "/biblioteca", "/buscar", "/calendario" o "/dashboard"
    Entonces se muestra la página correspondiente con su título
    Y cada página se carga de forma diferida (lazy)

  Escenario: CA3 — Ruta inexistente
    Dado la SPA
    Cuando navego a "/no-existe"
    Entonces veo una página "No encontrado" con un enlace al dashboard

  Escenario: CA4 — Servicios tipados generados desde el contrato público
    Dado el contrato "contracts/public/bff.v1.yaml"
    Cuando ejecuto "pnpm --filter spa gen:api"
    Entonces se generan servicios Angular tipados en "apps/spa/src/app/core/api/generated"
    Y la URL base se inyecta por configuración de entorno, no está fija en el código

  Escenario: CA5 — Calidad en CI
    Dado un PR que modifica "apps/spa"
    Cuando corre CI
    Entonces se ejecutan ESLint y las pruebas unitarias de la SPA
    Y si alguna falla el merge queda bloqueado
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA1 | Componente | `ShellComponent muestra los 4 enlaces de navegación` | Vitest/Jest + TestBed |
| 2 | CA1 | Router | `"/" redirige a "/dashboard"` | `RouterTestingHarness` |
| 3 | CA2 | Router | `cada ruta renderiza su página con título` (prueba parametrizada) | `RouterTestingHarness` |
| 4 | CA3 | Router | `ruta desconocida muestra NotFoundPage` | `RouterTestingHarness` |
| 5 | CA4 | Unitaria | `API_BASE_URL se provee desde environment` | TestBed |

Ejemplo (rojo):

```ts
it.each([
  ['/biblioteca', 'Biblioteca'],
  ['/buscar', 'Buscar'],
  ['/calendario', 'Calendario'],
  ['/dashboard', 'Dashboard'],
])('navega a %s y muestra "%s"', async (url, title) => {
  const harness = await RouterTestingHarness.create();
  await harness.navigateByUrl(url);
  expect(harness.routeNativeElement?.querySelector('h1')?.textContent).toContain(title);
});
```

## Tareas

- [x] T1 Generar app Angular en `apps/spa` (standalone components, `strict: true`, SCSS).
- [x] T2 (CA1, CA2, CA3) `app.routes.ts` con `loadComponent` por página, redirección y comodín `**`.
- [x] T3 (CA1) `ShellComponent` con barra de navegación accesible (`nav`, `aria-current`).
- [x] T4 (CA4) Script `gen:api` (ng-openapi-gen u openapi-generator `typescript-angular`) + token `API_BASE_URL`.
- [x] T5 (CA5) Configurar angular-eslint y el runner de pruebas; registrar el job en el pipeline de HU-00.09.
- [x] T6 Registrar `provideHttpClient(withInterceptorsFromDi())` como base para el interceptor de correlación (HU-00.11).

## Notas técnicas y decisiones

- Runner de pruebas: el que traiga por defecto la versión de Angular instalada (Vitest en las versiones recientes); si se prefiere Jest, documentarlo en el README de la app.
- Las páginas son placeholders; el diseño visual llega con cada épica.

## Fuera de alcance

- Autenticación y guards (EP-01).
- Diseño visual y sistema de componentes.

## Definición de Hecho

- [x] DoD común cumplida.
- [x] `ng build --configuration production` sin errores.
- [x] Lighthouse de accesibilidad ≥ 90 en el layout vacío. (100/100, ver Dev Agent Record)

## Notas de desarrollo (Dev Notes)

- **Angular 22 / vitest por defecto**: `ng new --test-runner=vitest` (default en Angular CLI 22) usa el builder `@angular/build:unit-test`. El flag `--strict` no escribe literalmente `"strict": true` en `tsconfig.json`; en su lugar añade `noImplicitOverride`, `noPropertyAccessFromIndexSignature`, `noImplicitReturns`, `noFallthroughCasesInSwitch` sobre el modo estricto de TS ya activo por defecto. Verificado con una prueba manual (`noImplicitAny` y `strictNullChecks` activos) — ver Dev Agent Record.
- **Nombre del paquete**: `@animetracker/spa` (no `spa`), consistente con `@animetracker/bff`. El Gherkin de CA4 usa `pnpm --filter spa gen:api` como referencia ilustrativa; el comando real usado en CI y README es `pnpm --filter @animetracker/spa run gen:api`.
- **Cliente API (CA4)**: se eligió `ng-openapi-gen` en vez de `openapi-typescript` (usado en BFF) porque genera servicios Angular (`HttpClient`) y una clase `ApiConfiguration` con `rootUrl`, exactamente lo que pide el AC ("servicios Angular tipados" + "URL base inyectada por configuración de entorno"). El contrato público (`contracts/public/bff.v1.yaml`) hoy solo expone `/health` y `/health/ready`; el servicio `Health` generado crecerá junto con el contrato en historias futuras (no se generó código de negocio porque el contrato no lo define todavía).
- `ApiConfiguration.rootUrl` se provee con el helper generado `provideApiConfiguration(environment.apiBaseUrl)` en `app.config.ts`. `environment.ts` (producción) usa `apiBaseUrl: ''` (mismo origen, reverse proxy Caddy/Traefik por la decisión Q5); `environment.development.ts` usa `http://localhost:3000` (coincide con el server de desarrollo del contrato).
- El cliente generado (`src/app/core/api/generated/`) no se versiona (gitignored), igual que `apps/bff/src/generated/`. Se regenera con `pnpm --filter @animetracker/spa run gen:api`. En CI es un paso explícito antes de lint/build/test (no un hook `pre*` de npm), porque CA4 lo describe como un comando independiente.
- **ESLint**: se usó el esquema oficial `ng add @angular-eslint/schematics` (flat config `eslint.config.js`) en vez de replicar a mano la config de `bff`; es el camino soportado por el propio Angular CLI y evita desalinearse con futuras versiones de Angular.
- **Cobertura (CA5 / DoD 80 %)**: el builder `@angular/build:unit-test` solo expone threshold/include/exclude de cobertura vía opciones de `angular.json` (`coverageThresholds`, `coverageInclude`, `coverageExclude`), no vía flags de `ng test`. Se configuró ahí, replicando el patrón `collectCoverageFrom` + `coverageThreshold` de `apps/bff/jest.config.js`. Cobertura real: 100 % statements/funciones/líneas, 98.57 % branches (10/10 tests).
- **`@vitest/coverage-v8`**: hubo que fijar la versión explícitamente (`^5.0.0`) como devDependency de `apps/spa`; sin ella pnpm resolvía una versión antigua (2.1.9) usada por otro paquete del workspace (`tools/ci`), incompatible con `vitest@5.x` y causaba un crash del reporter de cobertura.
- `pnpm-workspace.yaml`: se aprobaron los scripts de build de `@parcel/watcher`, `lmdb` y `msgpackr-extract` (dependencias nativas del dev-server de Angular/esbuild), siguiendo el patrón ya existente de `esbuild: true`.
- Fuera de alcance explícito (ver historia): autenticación/guards y diseño visual. Los textos de página son placeholders.

## Dev Agent Record

**Archivos creados:**
- `apps/spa/` completo: `angular.json`, `package.json`, `tsconfig*.json`, `eslint.config.js`, `ng-openapi-gen.json`, `src/app/{app.ts,app.html,app.scss,app.routes.ts,app.config.ts}`, `src/app/layout/shell/*`, `src/app/pages/{dashboard,biblioteca,buscar,calendario,not-found}/*`, `src/environments/*`, `src/app/core/api/generated/*` (gitignored).
- Pruebas: `app.spec.ts`, `app.routes.spec.ts` (CA1-CA3), `app.config.spec.ts` (CA4), `layout/shell/shell.spec.ts` (CA1).

**Archivos modificados:**
- `.github/workflows/ci.yml`: job `spa` reemplaza el stub por pasos reales (`gen:api`, `lint`, `build`, `test:coverage`).
- `.gitignore`: añadido `apps/spa/src/app/core/api/generated/` y `.angular/`.
- `pnpm-workspace.yaml`: `allowBuilds` para `@parcel/watcher`, `lmdb`, `msgpackr-extract`.
- `apps/spa/README.md`: instrucciones reales de arranque, scripts y estructura.

**Comandos de prueba y resultado:**
```
pnpm --filter @animetracker/spa run lint            → All files pass linting.
pnpm --filter @animetracker/spa run build            → OK, 5 lazy chunks (dashboard/biblioteca/buscar/calendario/not-found)
pnpm --filter @animetracker/spa run build --configuration production → OK (misma salida, es la config por defecto)
pnpm --filter @animetracker/spa run test:coverage    → 4 archivos de prueba, 10/10 tests, 100 % stmts, 98.57 % branches, 100 % funciones, 100 % líneas
pnpm --filter @animetracker/ci test                  → 18/18 (sin regresión)
pnpm --filter @animetracker/repo-checks test          → 33/33 (sin regresión)
node ... js-yaml.load(ci.yml)                         → parsea correctamente (invariante de CLAUDE.md)
npx lighthouse http://localhost:4321 --only-categories=accessibility (contra el build de producción servido estáticamente) → 100/100
```

**Cobertura:** 100 % statements, 98.57 % branches, 100 % funciones, 100 % líneas (umbral 80 % configurado como gate en `angular.json` → `coverageThresholds`).

## Code Review
- Fecha: 2026-11-02
- Resultado: APROBADA
- Cobertura de líneas (código cambiado): 100 % (statements 100 %, branches 98.57 %, funciones 100 %, líneas 100 %; umbral 80 % superado)

| Ámbito | Resultado | Hallazgos |
|---|---|---|
| Criterios de aceptación | ✅ | 0 |
| Seguridad | ✅ | 0 |
| Diseño / SOLID | ✅ | 0 |
| Arquitectura | ✅ | 0 |
| Pruebas (> 80 %) | ✅ | 0 |

### Hallazgos
_Ninguno ≥ MEDIA._

### Sugerencias (bajo el umbral)
- [BAJA][Aceptación] CA5 (calidad en CI) no tiene una prueba automatizada que confirme que el job `spa` se dispara para cambios bajo `apps/spa/` (solo verificado manualmente, vía Dev Agent Record) — `tools/ci/affected.ts:18`, `.github/workflows/ci.yml:243-262` — Sugerencia: agregar un caso en `affected.spec.ts` que cubra el prefijo `apps/spa/` → job `spa`.
- [BAJA][Diseño] Paths de ruta duplicados como literales entre `app.routes.ts` y `shell.ts` (DRY) — `apps/spa/src/app/app.routes.ts`, `apps/spa/src/app/layout/shell/shell.ts:16-21` — Sugerencia: extraer un `ROUTE_PATHS` compartido o derivar `navLinks` de `routes`.
- [BAJA][Diseño] `Shell.navLinks` hardcodeado en el componente (OCP) — `apps/spa/src/app/layout/shell/shell.ts:16-21` — Sugerencia: si crece la complejidad (roles, i18n, flags), mover a un token/servicio de navegación.
- [BAJA][Diseño] Páginas sin `ChangeDetectionStrategy.OnPush` — `apps/spa/src/app/pages/*/*.ts` — Sugerencia: fijar `OnPush` como convención desde esta historia base para que las páginas reales la hereden.
- [BAJA][Pruebas] Sin prueba E2E real (Playwright/Cypress) que levante la app servida, como describe el Gherkin de CA1 ("Dado la SPA en ejecución con `ng serve`"); hoy se cubre con `RouterTestingHarness` (integración TestBed) — `apps/spa/src/app/app.routes.spec.ts` — Sugerencia: aceptable para este alcance (skeleton); considerar un E2E mínimo en una historia futura de EP-01 con contenido real.
- [BAJA][Pruebas] Rama sin cubrir en `app.ts:10` (branch 90 %) es instrumentación v8 sobre el decorador `@Component`, no lógica de negocio — `apps/spa/src/app/app.ts:4-10` — Sin acción requerida.
