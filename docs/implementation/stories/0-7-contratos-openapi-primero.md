# HU-00.07 · Contratos OpenAPI primero [Técnica]

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-7-contratos-openapi-primero` |
| Tipo | Técnica |
| Prioridad | Must |
| Estimación | 5 pts |
| Sprint | sprint-1 |
| Estado | ready-for-dev |
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
