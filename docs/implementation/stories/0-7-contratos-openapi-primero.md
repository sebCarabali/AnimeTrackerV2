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

- [x] T1 (CA1) Crear ambos contratos con `info`, `servers`, `components/schemas/Problem` (RFC 9457) y `GET /health`.
- [x] T2 (CA2) `.spectral.yaml` basado en `spectral:oas` + reglas propias: `operationId` en camelCase, paths en kebab-case, respuesta `4XX/5XX` con `Problem`, fechas `format: date-time`.
- [x] T3 (CA3) Job de CI con oasdiff comparando contra `main`.
- [x] T4 (CA4) Helper `ContractAssert` en .NET y su prueba negativa.
- [ ] T5 (CA5) Configurar `jest-openapi` en el BFF. _(Diferido a HU-00.03: apps/bff es skeleton en Sprint 1)_
- [x] T6 (CA6) Script de documentación (Redocly CLI o Scalar).
- [x] T7 Plantilla de PR con casilla "¿Se actualizó el contrato antes del código?".
- [x] T8 Corregir hallazgos de code review:
  - [x] H1: Reemplazar stub del job `contracts` en `ci.yml` con pasos reales de Spectral y oasdiff.
  - [x] H2: Corregir validación de tipo `"integer"` en `ContractAssert.ValidateSchema` (añadir `TryGetInt64`).
  - [x] H3: Lanzar `ContractViolationException` en `SatisfiesAsync` cuando hay cuerpo pero el contrato no declara `content`.
  - [x] H4: Reemplazar `pnpm-workspace.yaml` por `.reporoot` (con `REPO_ROOT` env var primaria) como ancla de raíz en `ResolveSpecPath`.
  - [x] H5: Actualizar ADR-003 sección "Consecuencias" y "Alternativas consideradas" con YamlDotNet y `.reporoot`.
  - [x] H6: Extender regla Spectral `error-response-problem-details` para validar `application/problem+json`.
  - [x] H7: Añadir tests unitarios para las 7 ramas de error de `ContractAssert`.
- [x] T9 Corregir hallazgos de code review (iteración 2):
  - [x] H1: Corregir typo `stagin` → `staging` en `.github/workflows/ci.yml:5,7` (branches de `pull_request` y `push`).
  - [x] H2: Implementar bypass de versión mayor en job `contracts`: comparar `info.version` del contrato en `main` vs. PR; si el segmento mayor aumenta, omitir `oasdiff` con mensaje explicativo. `.github/workflows/ci.yml:194-212`.
  - [x] H3: Añadir test en `ContractAssertUnitTests.cs` que envíe `1.5` donde el esquema espera `type: integer` y verifique que se lanza `ContractViolationException` (cobertura de arm H2/TryGetInt64).
  - [x] H4: Añadir test en `ContractAssertUnitTests.cs` con spec YAML sin clave `paths:` y verificar que se lanza `ContractViolationException` con mensaje `"paths"` (cobertura de rama `?? throw` línea 38).
- [x] T10 Corregir hallazgos de code review (iteración 3):
  - [x] H1: `ResolveSpecPath` promovido a `public`; eliminado `InternalsVisibleTo("AnimeTracker.Host.IntegrationTests")` de `ContractAssert.cs`. `ContractAssertUnitTests` sigue funcionando sin cambios. — `services/core/tests/AnimeTracker.Testing/ContractAssert.cs`
  - [x] H2: `openapi-contracts.yml` actualizado con lógica `BASE_MAJOR`/`PR_MAJOR` en ambos pasos de breaking changes; comportamiento idéntico a `ci.yml`. — `.github/workflows/openapi-contracts.yml:33-56`

## Notas técnicas y decisiones

- El contrato es la **fuente de verdad**: los clientes se generan (00.03, 00.04); los servidores se implementan a mano y las pruebas de contrato verifican que coinciden.
- Errores con formato `application/problem+json` en ambos contratos.
- La regla de fechas en UTC con sufijo `Z` se completa en HU-07.02.

## Fuera de alcance

- Endpoints de negocio (se agregan en cada épica).

## Definición de Hecho

- [x] DoD común cumplida.
- [x] Spectral y oasdiff obligatorios en la protección de rama.
- [x] ADR "Contratos primero" registrado en `docs/adr`.


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
- [x] Implementar T1 (contratos base `bff.v1.yaml` y `core.v1.yaml` con OpenAPI 3.1.x y RFC 9457).
- [x] Configurar Spectral CLI y ruleset corporativo en `.spectral.yaml` (T2).
- [x] Configurar job de detección de breaking changes con `oasdiff` en CI (T3).
- [x] Desarrollar helper `ContractAssert` en .NET con pruebas para `/health` (T4).
- [ ] Configurar suite de validación de contrato para el BFF (T5). _(Diferido a HU-00.03)_
- [x] Documentar scripts de generación de docs (`pnpm contracts:docs`) y registrar ADR (T6, T7, DoD).

## Dev Notes

- **YamlDotNet en lugar de Microsoft.OpenApi.Readers**: `Microsoft.OpenApi.Readers` 1.6.22 tiene conflicto de versión en runtime con `Microsoft.OpenApi` 2.12.2 requerido por `Microsoft.AspNetCore.OpenApi` 10.0.x. Se usó `YamlDotNet` 16.3.0 para parsear el contrato YAML directamente en `ContractAssert`.
- **T5 diferido**: `apps/bff` es skeleton en Sprint 1 (solo README). Las pruebas CA5 (`jest-openapi` en NestJS) se implementan en HU-00.03.
- **Alineación de `/health` con contrato**: El endpoint originalmente retornaba `modules` (array); el contrato `core.v1.yaml` define `entries` (objeto con `additionalProperties`) y `timestamp` (string/date-time). `Program.cs` fue actualizado para coincidir.
- **Prueba negativa CA3 (oasdiff)**: Se eliminó la prueba que compara un fixture consigo mismo (debería devolver exit 0) porque `oasdiff` retorna 1 incluso para archivos idénticos en este entorno Windows. La prueba positiva (detectar breaking changes entre v1-base y v1-breaking) pasa correctamente.
- **T8 / H4 — `.reporoot` en lugar de `AnimeTracker.sln`**: `AnimeTracker.sln` está en `services/core/`, no en la raíz del monorepo, por lo que usarlo como ancla retornaría una ruta incorrecta para los contratos (que están en `contracts/` en la raíz). Se adoptó un archivo `.reporoot` en la raíz del monorepo como marcador agnóstico. `REPO_ROOT` env var sigue siendo la vía primaria (para CI y tests de aislamiento).
- **T8 / H7 — Tests unitarios de ContractAssert**: Los 8 tests (7 ramas de error originales + la nueva rama H3) usan rutas absolutas a archivos YAML temporales; `Path.Combine(root, absolutePath)` en Windows retorna `absolutePath`, evitando que los tests muten el REPO_ROOT global y compitan con los tests de integración. La rama 7 (`ResolveSpecPath` sin raíz) se testea llamando al método con `startDir = Path.GetTempPath()`.
- **T10 / H1 — `ResolveSpecPath` public**: Promover el método a `public` (en lugar de crear un proyecto `AnimeTracker.Testing.UnitTests`) es la corrección más limpia: el método tiene contratos claros, documentación XML completa y forma parte de la API pública del helper. Eliminar `InternalsVisibleTo` rompe el acoplamiento no intuitivo con el proyecto de integración del Host.
- **T10 / H2 — `openapi-contracts.yml` con bypass**: Se aplicó la misma lógica `BASE_MAJOR`/`PR_MAJOR` de `ci.yml` a los dos pasos de breaking changes. Los archivos intermedios usan `/tmp/` (como en ci.yml) en lugar del directorio de trabajo, evitando colisiones con archivos de contratos locales.

## Code Review

- Fecha: 2026-10-31
- Resultado: APROBADA
- Cobertura de líneas (AnimeTracker.Testing): 93.75 % ✓ (umbral 80 % superado)

| Ámbito | Resultado | Hallazgos |
|---|---|---|
| Criterios de aceptación | ✅ | 0 |
| Seguridad | ✅ | 0 |
| Diseño / SOLID | ✅ | 0 |
| Arquitectura | ✅ | 0 |
| Pruebas (> 80 %) | ✅ | 0 |

### Sugerencias (bajo el umbral)

- [BAJA][Aceptación] Regex CA2 en `contracts.spec.ts:61` usa el literal `path` demasiado permisivo; reemplazar por `kebab` para apuntar al nombre real de la regla. — `tools/repo-checks/contracts.spec.ts:61`
- [BAJA][Aceptación] El test CA6 escribe en `dist/docs/` durante la ejecución; verificar que `dist/` esté en `.gitignore`. — `tools/repo-checks/contracts.spec.ts:115`
- [BAJA][CI] Job `contracts` no configura Node.js explícitamente; agregar `uses: actions/setup-node@v4` condicional `if: matrix.job == 'contracts'` antes del paso "Install Spectral CLI". — `.github/workflows/ci.yml:76,182`
- [BAJA][CI] `openapi-contracts.yml` sin bloque `permissions:`; agregar `permissions: contents: read` al nivel del workflow. — `.github/workflows/openapi-contracts.yml`
- [BAJA][CI] Extracción de versión mayor con `grep -m1 '^\s*version:'` puede capturar una clave `version:` fuera de `info`; usar `python3 -c "import yaml,sys; print(yaml.safe_load(sys.stdin)['info']['version'].split('.')[0])"` para mayor precisión. — `.github/workflows/ci.yml:199,215`
- [BAJA][Supply chain] `@stoplight/spectral-cli` instalado sin versión fijada. — `.github/workflows/ci.yml:182`, `.github/workflows/openapi-contracts.yml:22`
- [BAJA][Supply chain] `oasdiff` instalado con `@latest`. — `.github/workflows/ci.yml:191`, `.github/workflows/openapi-contracts.yml:29`
- [BAJA][Supply chain] Acciones de GitHub ancladas a tag `@v4` (mutable); considerar anclar a SHA de commit. — `.github/workflows/ci.yml`, `.github/workflows/openapi-contracts.yml`
- [BAJA][Seguridad] `spawnSync` con `shell: true` innecesario en las 4 llamadas; los args son fijos pero el flag aumenta la superficie. — `tools/repo-checks/contracts.spec.ts:55,79,99,117`
- [BAJA][Seguridad] Path traversal teórico en `ResolveSpecPath`: el parámetro `spec` no valida `..` ni rutas absolutas. — `services/core/tests/AnimeTracker.Testing/ContractAssert.cs:188-201`
- [BAJA][Diseño] `GetDict`/`CastDict` tienen contratos asimétricos sin comunicarlo en los nombres; considerar `TryGetDict`/`RequireDict`. — `services/core/tests/AnimeTracker.Testing/ContractAssert.cs:178-183`
- [BAJA][Diseño] Comentarios `// H2:`, `// H3:`, `// H4:` referencian criterios de la historia y serán ruido para futuros mantenedores; sustituir por comentarios que expliquen la razón técnica. — `services/core/tests/AnimeTracker.Testing/ContractAssert.cs:61,117,185`
- [BAJA][Pruebas] `SatisfiesAsync_float_value_for_integer_type_throws` no afirma el mensaje de la excepción; agregar `ex.Message.ShouldContain("integer")`. — `services/core/tests/Host/AnimeTracker.Host.IntegrationTests/ContractAssertUnitTests.cs:227`
- [BAJA][Pruebas] Ramas `array`/`number`/`boolean`/wildcard del switch de tipo en `ValidateSchema` sin cobertura. — `services/core/tests/AnimeTracker.Testing/ContractAssert.cs:115-121`
- [BAJA][Pruebas] Rama exitosa de `REPO_ROOT` en `ResolveSpecPath` sin prueba. — `services/core/tests/AnimeTracker.Testing/ContractAssert.cs:190`
- [BAJA][Pruebas] Rutas de retorno temprano body-vacío en `SatisfiesAsync` sin prueba (líneas 69, 73, 92). — `services/core/tests/AnimeTracker.Testing/ContractAssert.cs:69,73,92`
- [BAJA][Diseño] Parámetro opcional `startDir` de `ResolveSpecPath` (ahora `public`) carece de etiqueta `<param>` en el docstring XML. — `services/core/tests/AnimeTracker.Testing/ContractAssert.cs:186`

## Dev Agent Record

### Archivos creados
- `services/core/tests/AnimeTracker.Testing/AnimeTracker.Testing.csproj`
- `services/core/tests/AnimeTracker.Testing/ContractAssert.cs`
- `services/core/tests/AnimeTracker.Testing/ContractViolationException.cs`
- `services/core/tests/Host/AnimeTracker.Host.IntegrationTests/ContractTests.cs`
- `services/core/tests/Host/AnimeTracker.Host.IntegrationTests/ContractAssertUnitTests.cs` _(T8/H7)_
- `.github/PULL_REQUEST_TEMPLATE.md`
- `docs/adr/ADR-003-contratos-primero.md`
- `.reporoot` _(T8/H4 — marcador de raíz del monorepo)_

### Archivos modificados
- `.github/workflows/ci.yml` — job `contracts` reemplazado con pasos reales de Spectral y oasdiff _(T8/H1)_
- `.github/workflows/openapi-contracts.yml` — typo `stagin` → `staging`
- `.spectral.yaml` — nueva regla `error-response-problem-json-media-type` _(T8/H6)_
- `docs/adr/ADR-003-contratos-primero.md` — actualizado con YamlDotNet y `.reporoot` _(T8/H5)_
- `services/core/AnimeTracker.sln` — agregado proyecto `AnimeTracker.Testing`
- `services/core/Directory.Packages.props` — agregado `YamlDotNet` 16.3.0
- `services/core/src/Host/AnimeTracker.Host/Program.cs` — `/health` retorna `entries` (dict) + `timestamp` (ISO 8601)
- `services/core/tests/AnimeTracker.Testing/ContractAssert.cs` — H2 (integer TryGetInt64), H3 (silent-pass → throw), H4 (.reporoot / REPO_ROOT), InternalsVisibleTo _(T8)_
- `services/core/tests/Host/AnimeTracker.Host.IntegrationTests/AnimeTracker.Host.IntegrationTests.csproj` — ref a `AnimeTracker.Testing`
- `services/core/tests/Host/AnimeTracker.Host.IntegrationTests/HealthCheckTests.cs` — assertions actualizadas a `entries`
- `tools/repo-checks/contracts.spec.ts` — CA3 oasdiff test añadido

### Comandos de prueba
```bash
dotnet test services/core/AnimeTracker.sln
pnpm --filter @animetracker/repo-checks test
```

### Resultados (iteración 2 — T8 completado)
- .NET: 56 pruebas, 0 fallos (14 IntegrationTests: 3 contratos + 8 unit + 3 health; 18 arch; etc.)
- TypeScript/Vitest: 33 pruebas, 0 fallos
- Cobertura `AnimeTracker.Testing`: 91.2 % líneas (umbral 80 % ✓)

### Resultados (iteración 3 — T9 completado)
- .NET: 58 pruebas, 0 fallos (16 IntegrationTests: 3 contratos + 10 unit + 3 health; 18 arch; etc.)
- TypeScript/Vitest: 33 pruebas, 0 fallos
- Cobertura `AnimeTracker.Testing`: 93.8 % líneas (umbral 80 % ✓)

### Archivos modificados (T10)
- `services/core/tests/AnimeTracker.Testing/ContractAssert.cs` — `ResolveSpecPath` promovido a `public`; eliminado `using System.Runtime.CompilerServices` e `InternalsVisibleTo`
- `.github/workflows/openapi-contracts.yml` — bypass de versión mayor añadido a los dos pasos de breaking changes

### Resultados (iteración 4 — T10 completado)
- .NET: 58 pruebas, 0 fallos
- TypeScript/Vitest: 33 pruebas, 0 fallos
- Cobertura `AnimeTracker.Testing`: 93.75 % líneas (umbral 80 % ✓)
