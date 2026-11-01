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
- [x] Cobertura de pruebas del BFF: **80% líneas en código nuevo (T1–T6)** medido con Jest coverage (umbral HU-00.09). Resultado: 100% statements/functions/lines, 80% branches (umbral `coverageThreshold` de `jest.config.js` cumplido).
- [x] `GET /health` documentado en `contracts/public`. `GET /health/ready` agregado al mismo contrato (T4a).
- [x] Desacoplamiento validado: Opción A confirmada en readiness (2026-11-03) — sin CA6 propio; HU-06.06 (tracking <200ms) y HU-04.03 (agregador) validarán el SLA cuando se implementen. Sin acción adicional en esta historia.

## Dev Agent Record

### Archivos creados

- `apps/bff/src/main.ts`, `src/bootstrap.ts`, `src/app.module.ts`, `src/app.controller.ts`, `src/app.service.ts`
- `apps/bff/src/core/core-client.ts`, `core-client.module.ts`, `node-fetch-adapter.ts`
- `apps/bff/src/health/health.module.ts`, `health.controller.ts`, `core-health.indicator.ts`, `health-status.types.ts`
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
