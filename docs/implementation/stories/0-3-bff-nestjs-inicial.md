# HU-00.03 · Proyecto BFF NestJS inicial

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-3-bff-nestjs-inicial` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 3 pts |
| Sprint | sprint-2 |
| Estado | ready-for-dev |
| Depende de | HU-00.01, HU-00.07, HU-00.12 |
| Bloquea | HU-00.06, HU-00.11, EP-01, EP-04, HU-06.06 |

## Historia

**Como** desarrollador full-stack, **quiero** un BFF NestJS configurado, **para** empezar a exponer endpoints a la SPA y a la extensión.

## Contexto

- ARD §5: el BFF agrega datos, maneja sesión y expone la ruta rápida para la extensión.
- La comunicación BFF ↔ .NET se define en el contrato interno (`contracts/internal`), por eso el cliente se genera, no se escribe a mano.

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

- [ ] T1 Generar proyecto NestJS en `apps/bff` (TypeScript estricto, ESLint, Prettier).
- [ ] T2 (CA4) `ConfigModule.forRoot` con esquema de validación (zod o Joi): `NODE_ENV`, `PORT`, `CORE_BASE_URL`, `CORS_ORIGINS`.
- [ ] T3 (CA3) Script `gen:core-client` (openapi-typescript + openapi-fetch, u orval) apuntando a `contracts/internal`.
- [ ] T4 (CA1, CA2) `HealthModule` con `@nestjs/terminus`: `/health` (liveness) y `/health/ready` (readiness).
- [ ] T5 (CA5) `app.setGlobalPrefix('api/v1', { exclude: ['health', 'health/ready'] })`.
- [ ] T6 Dockerfile multietapa del BFF (usado en HU-00.06).

## Notas técnicas y decisiones

- **Liveness vs. readiness:** el CA original pide 200 con el estado del núcleo; para no reiniciar el BFF cuando cae .NET, `/health` siempre devuelve 200 y `/health/ready` devuelve 503 si el núcleo no responde. Los orquestadores usan cada uno para su propósito.
- El cliente generado se versiona en git o se genera en `prebuild`; se recomienda **generar en `prebuild`** para que CI detecte contratos desactualizados.

## Fuera de alcance

- Autenticación y sesión (EP-01).
- Correlación y trazas (HU-00.11).

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Cobertura de pruebas del BFF en el umbral de HU-00.09.
- [ ] `GET /health` documentado en `contracts/public`.
