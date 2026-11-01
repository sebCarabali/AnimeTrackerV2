# ADR-003 — Metodología Contratos Primero y Gobernanza OpenAPI

| Campo | Valor |
|---|---|
| Fecha | 2026-10-31 |
| Estado | Aceptado |
| Autor | AnimeTracker Team |
| Historia | HU-00.07 |

## Contexto

El PRD §5.1 y el ARD §6 establecen que toda comunicación entre la SPA / extensión y el BFF, y entre el BFF y el núcleo .NET, debe describirse en un contrato OpenAPI antes de escribir cualquier lógica de negocio.

Sin una convención explícita y herramientas automatizadas de verificación, los contratos pueden desactualizarse, introducirse cambios incompatibles sin advertencia, o la documentación puede no reflejar el comportamiento real.

## Decisión

1. **Contratos como fuente de verdad**: los contratos OpenAPI en `contracts/` son la única fuente de verdad para las interfaces públicas e internas. Los clientes se generan a partir de ellos; los servidores se implementan a mano y sus pruebas de contrato verifican la conformidad.

2. **Dos especificaciones versionadas**:
   - `contracts/public/bff.v1.yaml` — API pública expuesta por el BFF a SPA y extensión.
   - `contracts/internal/core.v1.yaml` — API interna expuesta por el núcleo .NET al BFF.
   - Ambas usan OpenAPI 3.1.x y versionado semántico (`info.version`).

3. **Lint de estilo obligatorio (Spectral)**: todo PR que modifique un contrato debe pasar `spectral lint` con el ruleset corporativo (`.spectral.yaml`). El ruleset verifica:
   - `operationId` presente y en camelCase.
   - Paths en kebab-case.
   - Respuestas de error (4xx/5xx) con `application/problem+json` y esquema `Problem` (RFC 9457).

4. **Detección de breaking changes (oasdiff)**: el CI compara la rama actual con `main` y falla si se detectan cambios incompatibles no acompañados de un incremento de versión mayor.

5. **Pruebas de contrato automatizadas**:
   - `.NET`: helper `ContractAssert` en `tests/AnimeTracker.Testing` valida cada respuesta del Host contra `core.v1.yaml`.
   - BFF: se configura `jest-openapi` al crearse el proyecto NestJS (HU-00.03).

6. **Errores en formato `application/problem+json`** (RFC 9457) en ambos contratos.

7. **Documentación navegable** generada con Redocly CLI (`pnpm contracts:docs`) en `dist/docs/`.

## Alternativas consideradas

- **Generación de contrato desde el código** (code-first con Swashbuckle / NSwag): descartado porque se desea que el contrato preceda a la implementación y sea agnóstico del framework del servidor.
- **Sin versionado semántico**: descartado por falta de trazabilidad ante breaking changes.
- **`Microsoft.OpenApi.Readers` para parsear el contrato en .NET**: descartado por conflicto de versión en runtime entre `Microsoft.OpenApi.Readers` 1.6.22 y `Microsoft.OpenApi` 2.12.2 que requiere `Microsoft.AspNetCore.OpenApi` 10.0.x. Se optó por `YamlDotNet` 16.3.0 para parsear el YAML directamente en `ContractAssert`.
- **`pnpm-workspace.yaml` como ancla de raíz del repositorio en `ContractAssert`**: descartado porque acopla el código de pruebas .NET a un artefacto del ecosistema JavaScript. Se adoptó un archivo `.reporoot` dedicado en la raíz del monorepo, con `REPO_ROOT` como variable de entorno primaria (útil en CI y tests de aislamiento).

## Consecuencias

- **Positivas**: ciclo de feedback rápido ante cambios incompatibles; documentación siempre actualizada; clientes y servidores coherentes por construcción.
- **Negativas**: disciplina adicional requerida al añadir endpoints (el contrato va primero, siempre); el helper `ContractAssert` usa `YamlDotNet` 16.3.0 como dependencia de prueba, con un validador de esquema propio en lugar de la librería oficial de OpenAPI.

## Referencias

- PRD §5.1 — Arquitectura y contratos
- ARD §6 — Decisiones de interfaz
- HU-00.07 — Contratos OpenAPI primero
- HU-07.02 — Fechas en UTC en todos los contratos
