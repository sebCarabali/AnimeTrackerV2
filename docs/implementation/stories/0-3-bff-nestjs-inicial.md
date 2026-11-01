# HU-00.03 · Proyecto BFF NestJS inicial

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-3-bff-nestjs-inicial` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 3 pts |
| Sprint | sprint-2 |
| Estado | review |
| Depende de | HU-00.01, HU-00.07, HU-00.12 |
| Bloquea | HU-00.06, HU-00.11, EP-01, EP-04, HU-06.06 |

## Historia

**Como** desarrollador full-stack, **quiero** un BFF NestJS configurado con liveness, validación de configuración e integración tipada con core.v1.yaml, **para** desacoplar la SPA y extensión del core .NET, permitiendo evolución independiente y garantizando latencia <200ms en ruta rápida de tracking.

## Contexto

- ARD §5: el BFF actúa como **API Gateway y agregador de datos** (simplifica llamadas complejas al .NET), **maneja sesión y seguridad** (JWTs/HttpOnly cookies), y expone **ruta rápida para la extensión** (`/api/v1/track` con SLA <200ms) para registrar progreso de visualización sin acoplamiento directo con core.
- ARD §6: Contratos OpenAPI definen toda comunicación BFF ↔ .NET (contracts/internal) — el cliente se genera automáticamente, no se escribe a mano.
- Esta historia desacopla la SPA y extensión del core .NET, habilitando evolución independiente y escalabilidad futura de módulos.

## Criterios de aceptación

```gherkin
Característica: BFF NestJS listo para desarrollar

  Escenario: CA1 — Liveness con estado del núcleo disponible
    Dado el BFF arrancado y el núcleo .NET disponible
    Cuando llamo "GET /health"
    Entonces responde 200
    Y el cuerpo incluye "status": "ok" y "dependencies.core": "up"

  Escenario: CA2 — Liveness con el núcleo caído
    Dado el BFF arrancado y el núcleo .NET sin responder
    Cuando llamo "GET /health"
    Entonces responde 200
    Y el cuerpo incluye "dependencies.core": "down"
    Y "GET /health/ready" responde 503

  Escenario: CA3 — Cliente tipado generado desde el contrato interno
    Dado el contrato "contracts/internal/core.v1.yaml"
    Cuando ejecuto "pnpm --filter bff gen:core-client"
    Entonces se genera un cliente HTTP tipado en "apps/bff/src/generated/core"
    Y el BFF usa ese cliente para llamar a "/health" del núcleo
    Y el código generado no se edita a mano (está marcado y excluido del lint)

  Escenario: CA4 — Configuración por entorno con fallo rápido
    Dado el entorno "staging"
    Cuando el BFF arranca sin la variable obligatoria "CORE_BASE_URL"
    Entonces el proceso termina con código distinto de 0
    Y el mensaje de error nombra "CORE_BASE_URL"

  Escenario: CA5 — Prefijo y versionado de la API
    Dado el BFF arrancado
    Cuando reviso las rutas registradas
    Entonces las rutas de negocio usan el prefijo "/api/v1"
    Y "/health" queda fuera del prefijo
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA1 | E2E de módulo | `GET /health → 200 con core up` (núcleo simulado) | Jest + Supertest + nock |
| 2 | CA2 | E2E de módulo | `GET /health → 200 con core down` y `GET /health/ready → 503` | Jest + Supertest + nock |
| 3 | CA3 | Unitaria | `CoreHealthIndicator usa el cliente generado` (mock del cliente) | Jest + @nestjs/testing |
| 4 | CA4 | Unitaria | `validateEnv lanza error que nombra la variable faltante` | Jest |
| 5 | CA5 | E2E de módulo | `rutas de negocio responden bajo /api/v1` | Supertest |

Ejemplo (rojo):

```ts
describe('GET /health', () => {
  it('reporta core down sin fallar la liveness', async () => {
    nock(process.env.CORE_BASE_URL!).get('/health').reply(503);

    const res = await request(app.getHttpServer()).get('/health');

    expect(res.status).toBe(200);
    expect(res.body.dependencies.core).toBe('down');
  });
});
```

## Tareas

- [x] T1 Generar proyecto NestJS en `apps/bff` (TypeScript estricto, ESLint, Prettier).
- [x] T2 (CA4) `ConfigModule.forRoot` con esquema de validación (zod o Joi): `NODE_ENV`, `PORT`, `CORE_BASE_URL`, `CORS_ORIGINS`.
- [x] T3 (CA3) Script `gen:core-client` apuntando a `contracts/internal` — herramienta: **openapi-fetch** (decisión local documentada; pendiente formalización en ADR-003 §Decisión antes de sprint-2 planning. Alternativas descartadas: `openapi-generator-cli` — demasiado verboso para un BFF TypeScript estricto; `swagger-typescript-api` — menos ergonómico con generics y sin soporte nativo de fetch).
- [x] T4 (CA1, CA2, DoD) `HealthModule` con `@nestjs/terminus`: `/health` (liveness) y `/health/ready` (readiness).
  - [x] T4a (DoD) Verificar conformidad de la implementación con `contracts/public/bff.v1.yaml`: `GET /health` ya definido ✓; agregar `GET /health/ready` (503 cuando core down, esquema `Problem`) antes de hacer el PR.
- [x] T5 (CA5) `app.setGlobalPrefix('api/v1', { exclude: ['health', 'health/ready'] })`.
- [x] T6 Dockerfile multietapa del BFF (usado en HU-00.06).
- [x] T7 (ADR-003 §5) Instalar y configurar `jest-openapi` en `apps/bff`: añadir `setupFilesAfterEnv` apuntando a `contracts/public/bff.v1.yaml` para que cada test E2E de módulo valide automáticamente las respuestas contra el contrato.
- [x] T8 Pruebas unitarias de `CoreHealthIndicator` (CA3) y pruebas E2E de módulo de `/health`, `/health/ready` (CA1, CA2) y del prefijo `/api/v1` (CA5).
- [x] T9 Corregir hallazgos de code review (2026-11-01):
  - [x] Entrecomillar o reformular el `name:` del step "Build bff" en `ci.yml` (rompe el parseo YAML del workflow completo).
  - [x] Anclar el patrón `'(?i)test.*'` del allowlist de `.gitleaks.toml` a rutas reales de test.
  - [x] Excluir `devDependencies` de la imagen runtime del `Dockerfile` del BFF.
  - [x] Agregar `.env`/`.env.*` a `.dockerignore` (raíz).
  - [x] Loguear la excepción real en `CoreHealthIndicator.check()` antes de `indicator.down()`.
  - [x] Fijar `Content-Type: application/problem+json` en la respuesta 503 de `GET /health/ready`.
- [x] T10 Corregir hallazgos de code review — iteración 2 (2026-11-01):
  - [x] Anclar los patrones restantes del allowlist de `.gitleaks.toml` (`__fixtures__`, `fixtures/`, `testresults/`) a convenciones reales de ruta (igual que `__tests__/`, `tests/`, `.spec.ts$`, `.test.ts$`).
  - [x] Documentar el cambio estructural `[[allowlist]]` → `[allowlist]` (sin `regexTarget = "path"`) introducido en T9 y validar el allowlist con un secreto de prueba plantado en una ruta exenta y una no exenta.
  - [x] Resolver el acoplamiento de `readiness()` con el pipeline de interceptores de Nest (mover la construcción de la respuesta RFC 9457 a un `ExceptionFilter`, o documentar explícitamente la limitación antes de implementar HU-00.11).

## Notas técnicas y decisiones

- **Liveness vs. readiness:** el CA original pide 200 con el estado del núcleo; para no reiniciar el BFF cuando cae .NET, `/health` siempre devuelve 200 y `/health/ready` devuelve 503 si el núcleo no responde. Los orquestadores usan cada uno para su propósito.
- El cliente generado se versiona en git o se genera en `prebuild`; se recomienda **generar en `prebuild`** para que CI detecte contratos desactualizados.

## Dev Notes

> **Dependencia pendiente (readiness):** HU-00.12a y HU-00.12b deben estar mergeadas en `dev` antes de 2026-11-15 para que validateEnv() esté disponible sin reescritura en T2. Ambas están en PRs (estado review, fin sprint-1: 2026-11-13). Readiness re-evaluará automáticamente al mergearlas.

> **Nota:** Q5 (plataforma staging) debe resolverse antes de sprint-2 planning para desbloquear HU-00.10. Sin impacto directo en HU-00.03, pero afecta roadmap R0.

> **Pre-T2 spike ConfigModule (readiness 2026-11-03):** Patrón de integración confirmado desde HU-00.12b §Notas técnicas: `ConfigModule.forRoot({ validate: () => { validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV']); return process.env; } })`. `validateEnv()` en `apps/bff/src/common/config.ts` es función pura; compatible con NestJS ConfigModule sin modificación. Ejecutar spike de 15 min al inicio de T2 para confirmar con entorno real (Joi/Zod). Riesgo: bajo.

> **Enmienda pendiente a ADR-003 §Decisión (ejecutar en PR de T3):** Añadir al punto 5 de `docs/adr/ADR-003-contratos-primero.md`:
> - _"BFF: se usa **openapi-fetch** como generador de cliente tipado a partir de `contracts/internal/core.v1.yaml`. El script `pnpm --filter bff gen:core-client` genera el cliente en `apps/bff/src/generated/core`; el código generado no se edita a mano y queda excluido del lint. Alternativas descartadas: `openapi-generator-cli` (excesivamente verboso para un BFF TypeScript estricto) y `swagger-typescript-api` (menos ergonómico con generics y sin soporte nativo de fetch)."_
> - Actualizar `jest-openapi` en §5 para reflejar que la configuración se realiza en `apps/bff` con `setupFilesAfterFramework` (T7 de esta historia), no en la creación del proyecto base.
> ✓ **Aplicado** (2026-11-01): ambos puntos agregados a `docs/adr/ADR-003-contratos-primero.md` (punto 8 nuevo + punto 5 actualizado). Corrección: la clave real de Jest es `setupFilesAfterEnv`, no `setupFilesAfterFramework` (verificado en el código fuente de `jest-config`; `setupFilesAfterFramework` no existe en Jest 29).

### Decisiones de implementación (2026-11-01)

- **NestJS v11 (no v12):** `@nestjs/core`/`common`/`testing` v12.x eliminaron por completo el build CommonJS (`"type": "module"`, sin `require()` posible). El resto del proyecto (ts-jest, `apps/bff/package.json` sin `"type": "module"`) y el resto del monorepo JS asumen CommonJS; migrar todo el BFF a ESM (resolución `NodeNext`, extensiones `.js` en imports relativos, Jest en modo ESM) era un cambio de alcance mucho mayor que esta historia. Se fijó `@nestjs/*` en la línea `^11.x` (CJS, estable, con soporte activo) y `@nestjs/config@^4.0.4` / `@nestjs/terminus@^11.1.1` (compatibles con `@nestjs/common` `^11`).
- **`nock` fijado en `^14.0.17` (no `^15.x`):** `nock@15` también es ESM-only (`"type": "module"`, sin build CJS) y rompe la carga de los specs de Jest (`SyntaxError: Cannot use import statement outside a module`). `nock@14` sigue siendo CJS.
- **Cliente core con `node-fetch` en vez de `fetch` global:** `openapi-fetch` construye las peticiones con el `Request`/`fetch` globales (Undici). `nock` solo parchea `http.request`/`http.ClientRequest`, por lo que no intercepta peticiones hechas con el `fetch` nativo de Node — las pruebas E2E con `nock` (tal como pide la Estrategia TDD) nunca verían las llamadas al núcleo simulado. Se agregó `src/core/node-fetch-adapter.ts`, un adaptador que redespacha la petición a través de `node-fetch` (que sí usa `http`/`https`), manteniendo el mismo código en producción y en pruebas. Limitación conocida: el adaptador solo serializa cuerpo para métodos distintos de GET/HEAD; como hoy el único uso es `GET /health`, esa rama queda sin cobertura (documentado, no bloqueante).
- **`HealthIndicatorService` (no `HealthIndicator`) de `@nestjs/terminus`:** la clase base `HealthIndicator` fue retirada; la API vigente es `HealthIndicatorService.check(key).up()/.down()`.
- **Forma de la respuesta no delegada a `HealthCheckService`:** `HealthCheckService.check()` de Terminus lanza `ServiceUnavailableException` con su propio sobre `{status, info, error, details}`, que no coincide con `HealthStatus`/`Problem` de `contracts/public/bff.v1.yaml` (definidos en HU-00.07, antes de que existiera el BFF). `HealthController` usa `HealthIndicatorService` directamente y construye el cuerpo a mano para cumplir el contrato exactamente (validado por `jest-openapi` en los tests E2E).
- **`validateEnv` solo exige `CORE_BASE_URL`, `PORT`, `NODE_ENV`** (no `CORS_ORIGINS`): sigue el patrón confirmado en el spike de readiness (Dev Notes, HU-00.12b). Ningún CA de esta historia ejercita CORS; se difiere a cuando la SPA (HU-00.04) necesite consumir el BFF desde otro origen.
- **`main.ts` vs `bootstrap.ts`:** `configureApp()` (el `setGlobalPrefix`) se extrajo a `src/bootstrap.ts` para que las pruebas E2E lo reutilicen sin importar `main.ts`, cuyo top-level ejecuta `bootstrap().catch(() => process.exit(1))` como side-effect al cargarse (necesario para CA4, pero letal para un test runner si se importa).

## Fuera de alcance

- Autenticación y sesión (EP-01).
- Correlación y trazas (HU-00.11).

## Definición de Hecho

- [x] DoD común cumplida.
- [x] Cobertura de pruebas del BFF: **80% líneas en código nuevo (T1–T6, T9)** medido con Jest coverage (umbral HU-00.09). Resultado: 100% statements/functions/lines, 83.33% branches (umbral `coverageThreshold` de `jest.config.js` cumplido).
- [x] `GET /health` documentado en `contracts/public`. `GET /health/ready` agregado al mismo contrato (T4a).
- [x] Desacoplamiento validado: Opción A confirmada en readiness (2026-11-03) — sin CA6 propio; HU-06.06 (tracking <200ms) y HU-04.03 (agregador) validarán el SLA cuando se implementen. Sin acción adicional en esta historia.

## Dev Agent Record

### Archivos creados

- `apps/bff/src/main.ts`, `src/bootstrap.ts`, `src/app.module.ts`, `src/app.controller.ts`, `src/app.service.ts`
- `apps/bff/src/core/core-client.ts`, `core-client.module.ts`, `node-fetch-adapter.ts`
- `apps/bff/src/health/health.module.ts`, `health.controller.ts`, `core-health.indicator.ts`, `health-status.types.ts`, `core-unavailable.exception.ts`, `problem-json.filter.ts`
- `apps/bff/src/types/jest-openapi.d.ts`
- `apps/bff/src/__tests__/core-health-indicator.spec.ts`, `health.e2e.spec.ts`, `api-prefix.e2e.spec.ts`
- `apps/bff/jest.env.ts`, `jest.setup.ts`, `nest-cli.json`, `tsconfig.build.json`, `eslint.config.mjs`, `.prettierrc.json`, `.prettierignore`, `Dockerfile`
- `.dockerignore` (raíz)
- `apps/bff/src/generated/core/schema.d.ts` (generado por `gen:core-client`, no versionado — ver `.gitignore`)

### Archivos modificados

- `apps/bff/package.json` (dependencias NestJS/openapi-fetch/jest-openapi/nock/node-fetch, scripts `build`/`dev`/`lint`/`gen:core-client`)
- `apps/bff/tsconfig.json` (decorators, `module: commonjs`, target ES2021)
- `apps/bff/jest.config.js` (`setupFiles`, `setupFilesAfterEnv`, exclusiones de cobertura)
- `contracts/public/bff.v1.yaml` (+ `GET /health/ready`, T4a)
- `docs/adr/ADR-003-contratos-primero.md` (enmienda §Decisión: punto 8 openapi-fetch + punto 5 jest-openapi)
- `.github/workflows/ci.yml` (job `bff`: lint + build + test:coverage reales en vez del stub)
- `.gitignore` (`apps/bff/src/generated/`, `*.tsbuildinfo`)

### Comandos de prueba y resultado

```
pnpm --filter @animetracker/bff run gen:core-client   # OK
pnpm --filter @animetracker/bff run build              # OK (nest build, tsc)
pnpm --filter @animetracker/bff run lint                # OK, 0 problemas
pnpm --filter @animetracker/bff run test:coverage        # 4 suites, 15 tests — todos ✓
```

Cobertura (`src/**/*.ts`, excluye `generated/`, `main.ts`, `*.d.ts`): **100% statements, 80% branches, 100% functions, 100% lines** (umbral `coverageThreshold` 80% en las 4 métricas — cumplido).

Verificación manual del binario compilado (`node dist/main.js`):
- Sin `CORE_BASE_URL`/`PORT`/`NODE_ENV` → log con el nombre de las variables faltantes y `process.exit(1)` (CA4).
- Con env válido y núcleo caído → `GET /health` 200 `dependencies.core":"down"`; `GET /health/ready` 503 (CA1/CA2).
- `GET /api/v1` 200 (ruta de negocio bajo el prefijo); `GET /health` fuera del prefijo (CA5).

También se re-ejecutaron sin regresiones: `pnpm --filter @animetracker/repo-checks test` (33/33) y `pnpm --filter @animetracker/ci test` (18/18), y `spectral lint` sobre ambos contratos (0 errores).

### T9 — Corrección de hallazgos de code review (2026-11-01)

**Archivos modificados:**
- `.github/workflows/ci.yml` (name del step `bff` entrecomillado)
- `.gitleaks.toml` (allowlist anclado a rutas reales)
- `apps/bff/Dockerfile` (etapa `prod-deps` separada; `runtime` ya no copia devDependencies)
- `.dockerignore` (raíz; excluye `.env`/`.env.*`)
- `apps/bff/src/health/core-health.indicator.ts` (`Logger` inyectado, loguea la excepción real)
- `apps/bff/src/health/health.controller.ts` (`readiness()` con `@Res()` manual; fija `Content-Type: application/problem+json` en el 503)
- `apps/bff/src/__tests__/core-health-indicator.spec.ts` (+1 test: excepción no-`Error`)
- `apps/bff/src/__tests__/health.e2e.spec.ts` (+1 assert: header `Content-Type` real en el 503)

**Comandos y resultado:**
```
node -e "require('js-yaml').load(fs.readFileSync('.github/workflows/ci.yml'))"   # YAML OK (4 jobs parseados)
gitleaks detect --source . --config .gitleaks.toml --no-git                       # no leaks found
pnpm --filter @animetracker/bff run lint                                          # OK, 0 problemas
pnpm --filter @animetracker/bff run build                                         # OK
pnpm --filter @animetracker/bff run test:coverage                                 # 4 suites, 16 tests — todos ✓
pnpm --filter @animetracker/repo-checks test                                      # 33/33 ✓ (sin regresión)
pnpm --filter @animetracker/ci test                                               # 18/18 ✓ (sin regresión)
```

Cobertura tras T9 (`src/**/*.ts`, excluye `generated/`, `main.ts`, `*.d.ts`): **100% statements, 83.33% branches, 100% functions, 100% lines** (umbral 80% — cumplido; branches sube de 80% a 83.33% por la nueva rama cubierta en `CoreHealthIndicator`).

**Nota:** no se validó el build de la imagen Docker (`docker build -f apps/bff/Dockerfile .`) porque el daemon de Docker Desktop no estaba disponible en el entorno de desarrollo; el cambio del `Dockerfile` sigue el mismo patrón ya usado por la etapa `deps` existente (`pnpm install --filter ... --frozen-lockfile`, agregando `--prod`). Recomendado verificar el build de imagen en CI/HU-00.06.

### T10 — Corrección de hallazgos de code review — iteración 2 (2026-11-01)

**Archivos creados:**
- `apps/bff/src/health/core-unavailable.exception.ts` (excepción tipada para el 503 de readiness, carga el `Problem` RFC 9457)
- `apps/bff/src/health/problem-json.filter.ts` (`ProblemJsonExceptionFilter`, `@Catch(CoreUnavailableException)`)

**Archivos modificados:**
- `.gitleaks.toml` (3 patrones de allowlist anclados a convenciones reales de ruta; comentario documentando el cambio estructural `[[allowlist]]` → `[allowlist]` de T9 y su justificación)
- `apps/bff/src/health/health.controller.ts` (`readiness()` ya no usa `@Res()`; lanza `CoreUnavailableException` y usa `@UseFilters(ProblemJsonExceptionFilter)`)

**Comandos y resultado:**
```
gitleaks detect --source . --config .gitleaks.toml --no-git            # no leaks found (baseline, sin secretos plantados)

# Validación empírica del allowlist (secreto private-key PEM plantado y removido tras la prueba):
#   exentos (0 leaks):     apps/bff/src/__tests__/fixture.ts, apps/bff/src/__fixtures__/secret.ts,
#                          apps/bff/src/fixtures/secret.ts, TestResults/secret.ts
#   no exentos (1 leak c/u): apps/bff/src/attestation.ts, apps/bff/src/legacy-fixtures/secret.ts,
#                            apps/bff/src/nottestresults/secret.ts
gitleaks detect --source . --config .gitleaks.toml --no-git -v         # 3 leaks found (señuelos) — confirmado, ancla funciona

pnpm --filter @animetracker/bff run lint                                # OK, 0 problemas
pnpm --filter @animetracker/bff run build                               # OK
pnpm --filter @animetracker/bff run test:coverage                       # 4 suites, 16 tests — todos ✓
pnpm --filter @animetracker/repo-checks test                            # 33/33 ✓ (sin regresión)
pnpm --filter @animetracker/ci test                                     # 18/18 ✓ (sin regresión)
```

Cobertura tras T10 (`src/**/*.ts`, excluye `generated/`, `main.ts`, `*.d.ts`): **100% statements, 83.33% branches, 100% functions, 100% lines** (umbral 80% — cumplido; sin cambio respecto a T9, los archivos nuevos quedan 100% cubiertos por los mismos tests E2E existentes).

## Readiness Review
- Fecha: 2026-11-01 (iteración 5: re-validación post story-gaps iteración 5)
- Resultado: require-review (2)
- Técnico (Dev & Architect): 2 — Negocio (PM): 1
- Próxima acción: Aplicar enmienda ADR-003 §Decisión en el PR de T3 (no bloquea T1 ni T2; contenido exacto en Dev Notes).

### Hallazgos técnicos (2026-11-01: Revisor Técnico — iteración 5)
- [REVISAR] **ADR-003 §Decisión no enmendado en el archivo**: `ADR-003-contratos-primero.md` §5 no documenta openapi-fetch. El texto exacto de la enmienda está en Dev Notes (líneas 112-115). Por acuerdo de readiness, se aplica en el PR de T3; no bloquea T1 ni T2. — `docs/adr/ADR-003-contratos-primero.md:32-35`, `0-3-bff-nestjs-inicial.md:112-115`
- [INFO] Dependencias técnicas resueltas: HU-00.01 ✓, HU-00.07 ✓, HU-00.12b ✓, HU-00.12a ✓ (todos `done`). — `sprint_status.yml:122-147`
- [INFO] `apps/bff/src/common/config.ts` disponible desde HU-00.12b; `contracts/internal/core.v1.yaml` listo para gen:core-client (GET /health con CoreHealthStatus). — `apps/bff/src/common/config.ts`, `contracts/internal/core.v1.yaml:14-28`
- [INFO] GET /health/ready ausente en bff.v1.yaml — tarea explícita de T4a, no prerequisito. — `contracts/public/bff.v1.yaml:14-34`
- [INFO] CAs CA1–CA5 verificables, matriz TDD completa (T1–T7), estimación 3 pts razonable (~4.5h). — `0-3-bff-nestjs-inicial.md:63-98`

### Hallazgos de negocio (2026-11-01: Revisor de Producto — iteración 5)
- [INFO] Valor de negocio trazable a PRD §4, PRD §6, ARD §5. Ruta crítica del producto: desbloquea HU-00.06, HU-00.11, EP-01, EP-04, HU-06.06. — `0-3-bff-nestjs-inicial.md:17-23`
- [INFO] CAs Gherkin verificables sin solapamiento. Fuera de alcance explícito: EP-01, HU-00.11. — `0-3-bff-nestjs-inicial.md:27-61,116-118`
- [INFO] Dependencias de negocio resueltas. Las 8 acciones de readiness están `[x]`. — `sprint_status.yml:122-147`, `0-3-bff-nestjs-inicial.md:148-157`
- [INFO] DoD verificable: 80% Jest, T4a cubre /health y /health/ready en contracts/public, SLA <200ms Opción A documentada. — `0-3-bff-nestjs-inicial.md:121-126`
- [INFO] Estimación 3 pts coherente con T1-T7 (~4.5h). Enmienda ADR-003 es gobernanza técnica, no riesgo de negocio. — `0-3-bff-nestjs-inicial.md:90-98`

### Acciones para quedar Ready
- [x] **Antes del 2026-11-15:** Completar merge de HU-00.12a y HU-00.12b a `dev`. ✓ Commits 12ddcf4 (HU-00.12b) y bc9062c (HU-00.12a) merged.
- [x] **Antes de Sprint-2 Planning:** Documentar decisión openapi-fetch en T3 (local) o ADR-003. ✓ Documentado en T3 con alternativas descartadas; formalización ADR-003 pendiente en sprint-2 planning.
- [x] **Antes de iniciar T2:** Documentar patrón ConfigModule + validateEnv en Dev Notes. ✓ Dev Notes actualizado (readiness 2026-11-03).
- [x] **Sprint-2 Planning:** Enriquecer DoD con métricas concretas. ✓ DoD actualizado (líneas 116-120).
- [x] **Sprint-2 Planning (Opcional):** Confirmar HU-06.06 + HU-04.03 validan SLA o agregar CA6. ✓ Opción A confirmada (2026-11-03): sin CA6, seguimiento en EP-04/EP-06.
- [x] **Antes de iniciar T1 (brecha ADR):** Decidir si HU-00.03 incluye `jest-openapi` (ADR-003 §5): (a) agregar T7 para instalar/configurar jest-openapi en el BFF, o (b) actualizar ADR-003 moviendo la responsabilidad a HU-00.11. ✓ Decisión: Opción A — T7 agregada a HU-00.03 (2026-11-01).
- [x] **Antes de iniciar T4 (DoD):** Agregar sub-tarea a T4 (o crear T7) para documentar `GET /health` en `contracts/public/bff.v1.yaml`. ✓ T4a agregada a T4: verifica GET /health (ya en contrato) y agrega GET /health/ready antes del PR (2026-11-01).
- [x] **Antes de iniciar T3 (ADR):** Enmendar `ADR-003-contratos-primero.md` §Decisión para formalizar openapi-fetch como herramienta de generación del cliente BFF. ✓ Contenido exacto de la enmienda documentado en Dev Notes (2026-11-01); el developer la aplica en el PR de T3.

### Resolución de gaps — 2026-11-03 (iteración 3)
- Gaps resueltos (story-gaps): 3 — openapi-fetch T3, ConfigModule Dev Notes, Opción A DoD
- Nuevos gaps identificados: 2 — jest-openapi ADR, DoD /health sin tarea
- Estado: require-review (2)

### Resolución de gaps — 2026-11-01 (iteración 4)
- Gaps resueltos: 2 — (1) DoD /health: T4a agregada; (2) jest-openapi ADR-003 §5: T7 agregada (Opción A)
- Gaps escalados: 0

### Resolución de gaps — 2026-11-01 (iteración 5)
- Gaps resueltos: 1 — enmienda ADR-003: contenido exacto documentado en Dev Notes; se aplica en PR de T3
- Gaps escalados: 0
- Dependencias registradas: 0
- Cambios realizados en la historia: § Dev Notes (texto enmienda ADR-003 §Decisión)
- Resultado re-validación: require-review (2) — ADR-003 no enmendado en archivo; deferred a PR de T3. T1 y T2 no bloqueados.

## Code Review
- Fecha: 2026-11-01
- Resultado: CAMBIOS REQUERIDOS (iteración 2). Iteración 1 (2026-11-01) tuvo 6 hallazgos ≥ MEDIA, corregidos por T9 y verificados de forma independiente en esta iteración (ver checkmarks `✓`). Iteración 2 (re-ejecución de `/story-review` tras T9) encontró 3 hallazgos nuevos ≥ MEDIA (T10 pendiente).
- Cobertura de líneas (código cambiado): 100 % statements/functions/lines, **83.33 %** branches (umbral `coverageThreshold` de `jest.config.js` 80 % cumplido con margen). 16/16 tests passed, 4 suites.

| Ámbito | Resultado (iteración 2) | Hallazgos |
|---|---|---|
| Criterios de aceptación | ❌ | 1 |
| Seguridad | ❌ | 2 |
| Diseño / SOLID | ❌ | 1 |
| Arquitectura | ✅ | 0 |
| Pruebas (> 80 %) | ✅ | 0 |

### Hallazgos — iteración 2 (2026-11-01), corregidos por T10
- [x] [MEDIA][Seguridad] El allowlist de `.gitleaks.toml` corrigió el patrón `(?i)test.*` (T9) pero deja 3 entradas con el mismo defecto de anclaje por subcadena en cualquier posición de la ruta: `(?i)__fixtures__`, `(?i)fixtures/`, `(?i)testresults/`. Una ruta como `legacy-fixtures/prod-secrets.yaml` o `nottestresults/credentials.json` quedaría exenta del escaneo de secretos igual que ocurría con el patrón original, con superficie más acotada pero el mismo mecanismo de bypass. — `.gitleaks.toml:15-17` — Corrección sugerida: anclar igual que las entradas ya corregidas: `'(?i)(^|/)__fixtures__($|/)'`, `'(?i)(^|/)fixtures/'`, `'(?i)(^|/)testresults/'`.
  - ✓ Corregido: las 3 entradas ancladas a `'(?i)(^|/)__fixtures__($|/)'`, `'(?i)(^|/)fixtures/'`, `'(?i)(^|/)testresults/'`. Verificado plantando un secreto `private-key` en `legacy-fixtures/secret.ts` y `nottestresults/secret.ts` (señuelo, no debe eximir): ambos detectados como leak tras el ancla; antes del ancla habrían quedado exentos por subcadena.
- [x] [MEDIA][Seguridad] T9 cambió la estructura TOML del allowlist de `[[allowlist]]` (con `regexTarget = "path"`) a `[allowlist]` (tabla única, sin `regexTarget`) — cambio no mencionado en ninguna tarea ni hallazgo de code review. La única verificación ejecutada (`gitleaks detect --source . --config .gitleaks.toml --no-git` → "no leaks found") no prueba que el allowlist exima lo que debe eximir ni que ya no exima lo que no debe (no se plantó ningún secreto de prueba), dejando abierta la duda de si el allowlist — antes o después de T9 — estuvo realmente activo. — `.gitleaks.toml` (diff completo del archivo) — Corrección sugerida: documentar el cambio estructural explícitamente y validar con un secreto de prueba plantado en (a) una ruta que debe quedar exenta (p. ej. `__tests__/fixture.ts`) y (b) una ruta que no debe quedar exenta (p. ej. `src/attestation.ts`), confirmando ambos comportamientos.
  - ✓ Corregido: documentado en `.gitleaks.toml` (comentario sobre `[allowlist]`) que gitleaks v8 define el allowlist global de nivel superior como tabla única (no arreglo de tablas) y que `paths` siempre compara contra la ruta del archivo independientemente de `regexTarget` (ese campo solo afecta a `regexes`), por lo que `regexTarget = "path"` era innecesario. Validado empíricamente (ver Dev Agent Record T10): secreto `private-key` (bloque PEM) plantado en `__tests__/fixture.ts` → exento (0 leaks); el mismo secreto en `src/attestation.ts` → detectado (1 leak, rule `private-key`). Ambos comportamientos confirmados correctos.
- [x] [MEDIA][Diseño] `readiness()` usa `@Res()` manual sin `passthrough` para fijar `Content-Type: application/problem+json` en el 503 (fix válido y probado de T9), pero esto saca esa ruta del pipeline estándar de interceptores de Nest: un interceptor global futuro (p. ej. de correlación/trazas de HU-00.11, que esta historia bloquea explícitamente según la cabecera) no podrá modificar la respuesta después de que el handler la envíe directamente, y podría fallar con "Cannot set headers after they are sent". — `apps/bff/src/health/health.controller.ts:18-34` — Corrección sugerida: centralizar la construcción de la respuesta RFC 9457 en un `ExceptionFilter` (`@Catch()` + `@UseFilters()`) reutilizable en vez de manejar `res` manualmente en el controlador, o documentar explícitamente en Dev Notes que esta ruta queda fuera del pipeline de interceptores antes de implementar HU-00.11.
  - ✓ Corregido: `readiness()` ya no usa `@Res()`; lanza `CoreUnavailableException` (nueva, `core-unavailable.exception.ts`) cuando el core está down y retorna un valor normal (como `liveness()`) cuando está up. Un nuevo `ProblemJsonExceptionFilter` (`@Catch(CoreUnavailableException)`, aplicado con `@UseFilters()` solo en `readiness()`) construye la respuesta `application/problem+json` manualmente contra el `Response` de Express crudo — evitando el `ExpressAdapter.reply()` que revertía el Content-Type (mismo mecanismo, ahora en el punto de extensión sancionado por Nest; el propio código de `express-adapter.js` sugiere explícitamente "you might need a custom ExceptionFilter for non-JSON responses"). La ruta de éxito (200) vuelve a fluir por el pipeline estándar de interceptores; la excepción sigue siendo observable por interceptores futuros vía `catchError`/`tap` sobre el observable del handler antes de llegar al filtro. 16/16 tests sin cambios funcionales (mismos asserts de status/headers/body), 100% cobertura en `health.controller.ts`, `core-unavailable.exception.ts` y `problem-json.filter.ts`.

### Hallazgos — iteración 1 (2026-11-01), corregidos por T9
- [x] [CRITICA][Arquitectura] El step `name: Build bff (CA3: regenerates the core client from the contract)` es un escalar plano YAML sin comillas que contiene "`: `"; la gramática YAML corta el escalar ahí y rompe el parseo de **todo** `ci.yml`, no solo el job `bff` (confirmado con `js-yaml`: `bad indentation of a mapping entry (235:29)`). — `.github/workflows/ci.yml:235` — Corrección sugerida: entrecomillar el valor (`name: "Build bff (CA3: regenerates the core client from the contract)"`) o reformular sin "`: `" interno.
  - ✓ Corregido: `name:` entrecomillado en `ci.yml:235`. Verificado parseo completo con `js-yaml` (`YAML OK, jobs keys: [detect, run-job, secrets, ci-ok]`).
- [x] [ALTA][Seguridad] Patrón sin anclar `'(?i)test.*'` en el allowlist de `.gitleaks.toml` exime del escaneo cualquier ruta que contenga la subcadena "test" en cualquier posición (p. ej. `attestation.ts`, `latest-config.yml`, `contest-results.json`), permitiendo que un secreto real commiteado en esas rutas pase desapercibido. — `.gitleaks.toml:11` — Corrección sugerida: anclar a convenciones reales del repo (`'(?i)(^|/)__tests__/'`, `'\.spec\.ts$'`, `'\.test\.ts$'`).
  - ✓ Corregido: allowlist anclado a `(?i)(^|/)__tests__/`, `(?i)(^|/)tests/` (proyectos `.NET *.UnitTests/*.IntegrationTests` bajo `services/core/tests/`), `\.spec\.ts$`, `\.test\.ts$`. Verificado con `gitleaks detect --source . --config .gitleaks.toml --no-git`: "no leaks found".
- [x] [MEDIA][Seguridad] La etapa `deps` del `Dockerfile` instala sin `--prod`, por lo que `devDependencies` (jest, eslint, nock, ts-jest, etc.) terminan en la imagen de runtime pese a `NODE_ENV=production`. — `apps/bff/Dockerfile:8-11,20-21` — Corrección sugerida: etapa `prod-deps` con `pnpm install --prod` (o `pnpm deploy`) y copiar a `runtime` solo ese `node_modules` reducido.
  - ✓ Corregido: nueva etapa `prod-deps` (`pnpm install --filter @animetracker/bff --frozen-lockfile --prod`); `runtime` copia `node_modules` desde `prod-deps` en vez de `deps`. `deps` (con devDependencies) solo alimenta la etapa `build`.
- [x] [MEDIA][Seguridad] `.dockerignore` (raíz) no excluye `.env`/`.env.*` como sí hace `.gitignore`; un `.env` local en `apps/bff` quedaría embebido en capas intermedias de la imagen de build. — `.dockerignore:1-9` — Corrección sugerida: agregar `.env`, `.env.*`, `!.env.example`.
  - ✓ Corregido: agregado `.env`, `.env.*`, `!.env.example` a `.dockerignore`.
- [x] [MEDIA][Diseño] `CoreHealthIndicator.check()` usa un `catch` genérico que descarta la excepción real (timeout, DNS, bug del cliente) sin loguearla; no hay `Logger` de NestJS en el BFF. Dificulta diagnosticar en producción si "down" es un corte de red real o un defecto. — `apps/bff/src/health/core-health.indicator.ts:22` — Corrección sugerida: inyectar `Logger` de `@nestjs/common` y loguear `error`/`stack` antes de `indicator.down()`.
  - ✓ Corregido: `Logger` de `@nestjs/common` inyectado; loguea `error.message`/`stack` (o `JSON.stringify` del error del cliente) antes de `indicator.down()`. Rama no-`Error` cubierta con nuevo test unitario.
- [x] [MEDIA][Arquitectura] La respuesta 503 de `GET /health/ready` no fija `Content-Type: application/problem+json`; NestJS usa `application/json` por defecto. Contradice ADR-003 punto 6 y `contracts/public/bff.v1.yaml` (503 declarado como `application/problem+json`). El test E2E no lo detecta porque `jest-openapi` resuelve el esquema por código de estado sin comparar el header real. — `apps/bff/src/health/health.controller.ts:18-29,39-47` — Corrección sugerida: `@Header('Content-Type', 'application/problem+json')` en el método de readiness.
  - ✓ Corregido: `@Header()` estático no funciona porque `ExpressAdapter.reply()` **revierte** cualquier `Content-Type` ≠ `application/json` cuando el body es un objeto y el status ≥ 400 (verificado en `node_modules/@nestjs/platform-express/adapters/express-adapter.js:83-94`). Se cambió `readiness()` a `@Res()` sin `passthrough` (control manual total): `res.status(503).setHeader('Content-Type', 'application/problem+json').json(...)`, evitando que Nest reescriba la respuesta. Nuevo assert `res.headers['content-type']` en `health.e2e.spec.ts` confirma el header real (no solo el esquema).

### Sugerencias (bajo el umbral)
- [BAJA][Aceptación] CA4: el `process.exit(1)` real de `main.ts` solo se verificó manualmente (Dev Agent Record), sin test automatizado que mockee `process.exit`/`console.error`. — `apps/bff/src/main.ts:14-19`
- [BAJA][Aceptación] El comando del Gherkin de CA3 (`pnpm --filter bff gen:core-client`) no coincide literalmente con el script real (`pnpm --filter @animetracker/bff run gen:core-client`); detalle de redacción sin impacto funcional. — `0-3-bff-nestjs-inicial.md:45`
- [BAJA][Diseño] `HealthController` mezcla orquestación HTTP con construcción de los DTOs del contrato; decisión justificada en la historia, aceptable con un solo indicador. Revisar si se agregan más (tracking, calendar). — `apps/bff/src/health/health.controller.ts:31-47`
- [BAJA][Diseño] `toHealthStatus`/`toProblem` reciben `string` en vez del union literal `'up'|'down'` (`HealthIndicatorStatus` de `@nestjs/terminus`), perdiendo verificación del compilador. — `apps/bff/src/health/health.controller.ts:31,39`
- [BAJA][Seguridad] `Dockerfile` usa el tag mutable `node:24-alpine` en vez de pin por digest. — `apps/bff/Dockerfile:4`
- [BAJA][Seguridad] `ci.yml` no pasa `--frozen-lockfile` explícito en `pnpm install` (pnpm lo infiere de `CI=true`, riesgo bajo). — `.github/workflows/ci.yml:229`
- [BAJA][Seguridad] Nota prospectiva: sin `helmet()` ni `ValidationPipe` global; razonable hoy (sin endpoints de negocio ni CORS), pero planificar antes de exponer rutas reales bajo `/api/v1`. — `apps/bff/src/main.ts`
- [BAJA][Arquitectura] `GET /` (`AppController`, expuesto como `GET /api/v1`) no está documentado en `contracts/public/bff.v1.yaml`, desviándose de ADR-003 punto 1. Es andamiaje de CA5, no bloquea, pero debería documentarse o marcarse como no contractual antes de añadir rutas de negocio reales. — `apps/bff/src/app.controller.ts:8-11`
- [BAJA][Arquitectura] La dependencia de producción `node-fetch` (adaptador para que `nock` intercepte `openapi-fetch`) está bien justificada en Dev Notes pero no quedó registrada en ningún ADR. — `apps/bff/src/core/node-fetch-adapter.ts:1-27`
- [BAJA][Pruebas] Rama no cubierta en `node-fetch-adapter.ts` (método ≠ GET/HEAD con cuerpo serializado); documentada en la historia como limitación conocida y no bloqueante (único uso actual es `GET /health`). — `apps/bff/src/core/node-fetch-adapter.ts:12-17`
- [BAJA][Pruebas] Duplicación menor de setup de entorno (`CORE_BASE_URL`/`PORT`/`NODE_ENV` + `AppModule`/`configureApp`) entre `health.e2e.spec.ts` y `api-prefix.e2e.spec.ts`. — `apps/bff/src/__tests__/health.e2e.spec.ts:14-17`
- [BAJA][Diseño] Literal `'/health/ready'` duplicado: una vez como ruta (`@Get('health/ready')`) y otra vez como `instance` en `toProblem()`. Si la ruta cambia, es fácil olvidar el segundo. — `apps/bff/src/health/health.controller.ts:18,50` — Corrección sugerida: extraer una constante o derivar `instance` de `req.url`.
- [BAJA][Aceptación] La tarea T2 describe el esquema de validación como `NODE_ENV, PORT, CORE_BASE_URL, CORS_ORIGINS`, pero la implementación (`validateBffConfig` en `app.module.ts:9-11`) omite `CORS_ORIGINS`. La omisión está justificada y documentada en "Decisiones de implementación" (diferida a HU-00.04), por lo que se registra como desalineación de redacción, no como bloqueo. — `0-3-bff-nestjs-inicial.md:91` — Corrección sugerida: actualizar el texto de T2 para reflejar el alcance real implementado.
