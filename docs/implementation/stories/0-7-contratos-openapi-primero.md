# HU-00.07 · Contratos OpenAPI primero [Técnica]

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-7-contratos-openapi-primero` |
| Tipo | Técnica |
| Prioridad | Must |
| Estimación | 5 pts |
| Sprint | sprint-1 |
| Estado | in-progress |
| Depende de | HU-00.01, HU-00.09 |
| Bloquea | HU-00.03, HU-00.04, HU-07.02 y cualquier endpoint nuevo |

## Historia

**Como** equipo, **quiero** que los contratos OpenAPI se definan y versionen antes de implementar, **para** cumplir la metodología Contratos Primero del PRD.

## Contexto

- PRD §5.1 y ARD §6: toda comunicación SPA ↔ BFF y BFF ↔ .NET se define en OpenAPI antes de escribir lógica.
- Dos contratos con audiencias distintas: el público (SPA y extensión) y el interno (BFF ↔ núcleo).

## Criterios de aceptación

```gherkin
Característica: Contratos versionados y verificados

  Escenario: CA1 — Dos especificaciones versionadas
    Dado la carpeta "contracts"
    Cuando la reviso
    Entonces existe "contracts/public/bff.v1.yaml" (SPA y extensión ↔ BFF)
    Y existe "contracts/internal/core.v1.yaml" (BFF ↔ núcleo .NET)
    Y ambas declaran "openapi: 3.1.x" e "info.version" con versionado semántico

  Escenario: CA2 — Lint de estilo
    Dado un PR que modifica un contrato
    Cuando corre CI
    Entonces se ejecuta Spectral con el ruleset del repositorio
    Y una operación sin "operationId", sin respuesta de error documentada o con nombres que no siguen la convención hace fallar el job

  Escenario: CA3 — Detección de cambios incompatibles
    Dado la versión publicada del contrato en "main"
    Cuando un PR elimina un campo obligatorio de una respuesta o cambia el tipo de un parámetro
    Entonces CI marca el cambio como incompatible y falla
    A menos que el PR incremente la versión mayor del contrato

  Escenario: CA4 — Pruebas de contrato del núcleo
    Dado un endpoint del núcleo .NET implementado
    Cuando corren sus pruebas de integración
    Entonces cada respuesta se valida contra el esquema de "core.v1.yaml"
    Y la prueba falla si sobra o falta un campo obligatorio o un tipo no coincide

  Escenario: CA5 — Pruebas de contrato del BFF
    Dado un endpoint del BFF implementado
    Cuando corren sus pruebas e2e
    Entonces cada respuesta se valida contra "bff.v1.yaml"

  Escenario: CA6 — Documentación navegable
    Dado los contratos
    Cuando ejecuto "pnpm contracts:docs"
    Entonces se genera documentación HTML de ambos contratos
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA2 | Lint | Fixture `contracts/__fixtures__/invalid.yaml` debe producir errores | Spectral CLI en prueba Vitest |
| 2 | CA3 | Compatibilidad | Fixtures `v1-base.yaml` vs `v1-breaking.yaml` → oasdiff reporta breaking | oasdiff |
| 3 | CA4 | Contrato | `Health_responde_segun_contrato` | xUnit + WebApplicationFactory + validador de esquema |
| 4 | CA4 | Contrato | Prueba negativa: respuesta con campo extra/faltante falla el validador | xUnit |
| 5 | CA5 | Contrato | `GET /health cumple bff.v1.yaml` | Jest + `jest-openapi` |

Ejemplo .NET (rojo):

```csharp
[Fact]
public async Task Health_responde_segun_contrato()
{
    var response = await _client.GetAsync("/health");

    await ContractAssert.SatisfiesAsync(
        spec: "contracts/internal/core.v1.yaml",
        method: "GET", path: "/health",
        response);
}
```

`ContractAssert` es un helper compartido en `tests/AnimeTracker.Testing` que carga el YAML con `Microsoft.OpenApi.Readers` y valida el cuerpo con un validador JSON Schema (p. ej. `JsonSchema.Net`).

## Tareas

- [ ] T1 (CA1) Crear ambos contratos con `info`, `servers`, `components/schemas/Problem` (RFC 9457) y `GET /health`.
- [ ] T2 (CA2) `.spectral.yaml` basado en `spectral:oas` + reglas propias: `operationId` en camelCase, paths en kebab-case, respuesta `4XX/5XX` con `Problem`, fechas `format: date-time`.
- [ ] T3 (CA3) Job de CI con oasdiff comparando contra `main`.
- [ ] T4 (CA4) Helper `ContractAssert` en .NET y su prueba negativa.
- [ ] T5 (CA5) Configurar `jest-openapi` en el BFF.
- [ ] T6 (CA6) Script de documentación (Redocly CLI o Scalar).
- [ ] T7 Plantilla de PR con casilla "¿Se actualizó el contrato antes del código?".

## Notas técnicas y decisiones

- El contrato es la **fuente de verdad**: los clientes se generan (00.03, 00.04); los servidores se implementan a mano y las pruebas de contrato verifican que coinciden.
- Errores con formato `application/problem+json` en ambos contratos.
- La regla de fechas en UTC con sufijo `Z` se completa en HU-07.02.

## Fuera de alcance

- Endpoints de negocio (se agregan en cada épica).

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Spectral y oasdiff obligatorios en la protección de rama.
- [ ] ADR "Contratos primero" registrado en `docs/adr`.


## Readiness Review
- Fecha: 2026-10-31
- Resultado: ready-for-dev (1)
- Técnico (Dev & Architect): 1 — Negocio (PM): 1 — Ágil (Scrum Master): 1

### Hallazgos técnicos (Dev & Architect)
- [INFO] Dependencias previas completadas: HU-00.01 (Estructura del monorepo) y HU-00.09 (Pipeline de integración continua) se encuentran en estado `done`. No existen bloqueos técnicos. — `sprint_status.yml:119,135`
- [REVISAR] Estrategia de prueba para CA5/T5 (BFF): Dado que `apps/bff` se implementa en HU-00.03 (Sprint 2), la prueba de CA5 debe abordarse en HU-00.07 con un fixture HTTP/mock mínimo en el runner de pruebas o acotar que la prueba E2E contra el servicio NestJS se completa en HU-00.03 al crearse el endpoint `/health`. — `0-7-contratos-openapi-primero.md:54-58,98`
- [INFO] Paquetes NuGet para .NET 10: `Microsoft.OpenApi.Readers` y `JsonSchema.Net` deben agregarse centralizados en `services/core/Directory.Packages.props` asegurando compatibilidad con .NET 10. — `CLAUDE.md:78`
- [INFO] Gobernanza arquitectónica: Se debe registrar el ADR "Metodología Contratos Primero y Gobernanza OpenAPI" en `docs/adr/ADR-003-contratos-primero.md` como parte del DoD. — `0-7-contratos-openapi-primero.md:116`

### Hallazgos de negocio (Product Manager)
- [INFO] Valor y alineación estratégica: La historia cumple con el PRD §5.1 ("Contratos primero"). Asegura que las APIs entre SPA ↔ BFF y BFF ↔ Core estén blindadas contractualmente antes del inicio de las épicas funcionales (EP-01 a EP-06). — `PRD.md#51-arquitectura-y-contratos`
- [INFO] Criterios de aceptación exhaustivos: Los 6 escenarios Gherkin (CA1–CA6) cubren versionado semántico, linting, breaking changes, pruebas en .NET, pruebas en BFF y documentación HTML navegable.
- [INFO] Alcance acotado: Los endpoints de negocio quedan explícitamente fuera de alcance, limitándose a los tipos base (`Problem` RFC 9457) y `/health`.

### Hallazgos ágiles (Scrum Master)
- [INFO] Cumplimiento pleno del Definition of Ready (DoR): Criterios en formato Given-When-Then, estimación acordada (5 pts), dependencias resueltas y criterios de UI no aplican por ser historia técnica de infraestructura. — `AGENTS.md#definition-of-ready-dor`
- [INFO] Desbloqueo de cadena crítica: Esta historia es el habilitador directo que bloquea HU-00.03 (BFF NestJS), HU-00.04 (SPA Angular) y HU-07.02 (Fechas UTC). Su ejecución en Sprint 1 es prioritaria para el flujo del equipo. — `sprint_status.yml:13`

### Acciones para iniciar desarrollo
- [x] Confirmar dependencias técnicas (HU-00.01 y HU-00.09) cerradas en `dev`.
- [ ] Implementar T1 (contratos base `bff.v1.yaml` y `core.v1.yaml` con OpenAPI 3.1.x y RFC 9457).
- [ ] Configurar Spectral CLI y ruleset corporativo en `.spectral.yaml` (T2).
- [ ] Configurar job de detección de breaking changes con `oasdiff` en CI (T3).
- [ ] Desarrollar helper `ContractAssert` en .NET con pruebas para `/health` (T4).
- [ ] Configurar suite de validación de contrato para el BFF (T5).
- [ ] Documentar scripts de generación de docs (`pnpm contracts:docs`) y registrar ADR (T6, T7, DoD).
