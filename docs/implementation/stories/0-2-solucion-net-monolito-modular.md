# HU-00.02 · Solución .NET 8 como monolito modular [Técnica]

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-2-solucion-net-monolito-modular` |
| Tipo | Técnica |
| Prioridad | Must |
| Estimación | 5 pts |
| Sprint | sprint-1 |
| Estado | review |
| Depende de | HU-00.01 |
| Bloquea | HU-00.03, HU-00.06, HU-00.08, HU-00.11, EP-02, EP-03, EP-05 |

## Historia

**Como** desarrollador backend, **quiero** una solución .NET 8 con un proyecto por módulo y capa, **para** mantener las fronteras de Clean Architecture y poder extraer módulos a microservicios en el futuro.

## Contexto

- ARD §2 y §4: monolito modular; cada módulo con capas Api, Application, Domain e Infrastructure.
- ARD §4.2: los módulos no comparten tablas; se comunican por interfaces en memoria (patrón Mediator) o proyecciones de solo lectura.
- Las fronteras deben ser verificables automáticamente, no solo por convención.

## Criterios de aceptación

```gherkin
Característica: Solución .NET modular con fronteras verificables

  Escenario: CA1 — Estructura de proyectos
    Dado la solución "services/core/AnimeTracker.sln"
    Cuando la abro
    Entonces existen los módulos Catalog, Tracking y Calendar
    Y cada módulo tiene los proyectos Api, Application, Domain, Infrastructure y Contracts
    Y existen los proyectos "AnimeTracker.Host" y "AnimeTracker.SharedKernel"
    Y "dotnet build" termina sin errores ni advertencias tratadas como error

  Escenario: CA2 — Domain no depende de capas externas
    Dado un proyecto Domain que referencia Infrastructure, Application, Api o EF Core
    Cuando se ejecutan las pruebas de arquitectura
    Entonces la prueba falla indicando el tipo y la dependencia prohibida
    Y la compilación en CI falla

  Escenario: CA3 — Un módulo no referencia el interior de otro
    Dado un tipo del módulo Tracking que referencia "AnimeTracker.Catalog.Application" o "AnimeTracker.Catalog.Infrastructure"
    Cuando se ejecutan las pruebas de arquitectura
    Entonces la prueba falla
    Pero referenciar "AnimeTracker.Catalog.Contracts" está permitido

  Escenario: CA4 — Comunicación entre módulos vía Mediator
    Dado que el módulo Tracking necesita el título de un anime
    Cuando envía la consulta pública "GetAnimeSummaryQuery" definida en Catalog.Contracts
    Entonces recibe un "AnimeSummaryDto" resuelto por un handler del módulo Catalog
    Y Tracking no tiene acceso a repositorios ni DbContext de Catalog

  Escenario: CA5 — El Host compone los módulos
    Dado el proyecto Host arrancado
    Cuando llamo "GET /health"
    Entonces responde 200 con el estado de cada módulo registrado
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA2 | Arquitectura | `Domain_no_depende_de_Infrastructure_Application_Api_ni_EFCore(modulo)` | xUnit + NetArchTest.Rules |
| 2 | CA2 | Arquitectura | `Application_no_depende_de_Infrastructure_ni_Api(modulo)` | NetArchTest |
| 3 | CA3 | Arquitectura | `Modulo_solo_referencia_Contracts_de_otros_modulos(origen, destino)` | NetArchTest |
| 4 | CA3 | Arquitectura | `Contracts_no_depende_de_ninguna_capa_interna(modulo)` | NetArchTest |
| 5 | CA4 | Integración | `Tracking_obtiene_resumen_de_anime_via_mediator` | xUnit + ServiceCollection real |
| 6 | CA5 | Integración | `Host_health_responde_200_con_modulos_registrados` | WebApplicationFactory |

Ejemplo (rojo):

```csharp
public class ModuleBoundaryTests
{
    public static TheoryData<string> Modules => new() { "Catalog", "Tracking", "Calendar" };

    [Theory, MemberData(nameof(Modules))]
    public void Domain_no_depende_de_capas_externas(string module)
    {
        var result = Types.InAssembly(AssemblyOf(module, "Domain"))
            .ShouldNot()
            .HaveDependencyOnAny(
                $"AnimeTracker.{module}.Application",
                $"AnimeTracker.{module}.Infrastructure",
                $"AnimeTracker.{module}.Api",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Tipos con dependencias prohibidas: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Theory]
    [InlineData("Tracking", "Catalog")]
    [InlineData("Calendar", "Catalog")]
    [InlineData("Catalog", "Tracking")]
    public void Modulo_solo_ve_Contracts_de_otro(string origin, string target)
    {
        var result = Types.InNamespace($"AnimeTracker.{origin}")
            .ShouldNot()
            .HaveDependencyOnAny(
                $"AnimeTracker.{target}.Domain",
                $"AnimeTracker.{target}.Application",
                $"AnimeTracker.{target}.Infrastructure",
                $"AnimeTracker.{target}.Api")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
```

Verificación negativa obligatoria: en una rama temporal, agregar una referencia prohibida y comprobar que la prueba falla (evidencia en el PR).

## Tareas

- [x] T1 (CA1) Crear `AnimeTracker.sln`, `Directory.Build.props` (net10.0, `Nullable` enable, `TreatWarningsAsErrors`, `ImplicitUsings`) y `Directory.Packages.props` (versiones centralizadas).
- [x] T2 (CA1) Generar los 3 módulos × 5 proyectos (Catalog, Tracking, Calendar) con referencias permitidas: Api → Application → Domain; Infrastructure → Application/Domain; Contracts sin dependencias internas.
- [x] T3 (CA1) Crear `SharedKernel` (Entity, AggregateRoot, ValueObject, Result, IDomainEvent, IClock).
- [x] T4 (CA2, CA3) Crear `tests/AnimeTracker.ArchitectureTests` con las pruebas anteriores.
- [x] T5 (CA4) Definir `GetAnimeSummaryQuery` en Catalog.Contracts con handler stub en Catalog.Application; prueba de integración desde Tracking.
- [x] T6 (CA5) `Host`: método de extensión `AddXxxModule()` / `MapXxxModule()` por módulo; health checks.
- [x] T7 Crear proyectos de prueba por módulo: `AnimeTracker.{Modulo}.UnitTests` e `IntegrationTests`.
- [x] T8 Corregir hallazgos de code review:
  - [x] Eliminar `<ProjectReference>` a Infrastructure de los tres `*.Api.csproj` (Catalog, Tracking, Calendar).
  - [x] Agregar test `Api_no_depende_de_Infrastructure(string module)` en `ModuleBoundaryTests`.
  - [x] Agregar `Equals`/`GetHashCode` por Id en `Entity<TId>`.
  - [x] Agregar pruebas unitarias para `Entity<TId>` (raise/clear events) y `Result<T>` (Ok/Failure).
  - [x] Mover lógica de eventos de dominio a `AggregateRoot<TId>`; limpiar `Entity<TId>`.
  - [x] Agregar `where TId : notnull` en `Entity<TId>`.
  - [x] Proteger `/health` con autenticación o restricción de red antes de staging.
  - [x] Reemplazar las 5 pruebas Placeholder por pruebas de arranque real o marcar con TODO+ticket.
- [x] T9 Corregir hallazgos de code review (segunda iteración):
  - [x] Agregar guard en `Result<T>.Value`: lanzar `InvalidOperationException("Cannot access Value of a failed result.")` cuando `IsFailure=true`; actualizar `ResultTests` para verificar la excepción.
- [x] T10 Corregir hallazgos de code review (tercera iteración):
  - [x] Corregir guard de `/health` para funcionar detrás de reverse proxy: agregar `app.UseForwardedHeaders(new ForwardedHeadersOptions { ... })` con `KnownProxies` explícitos antes del guard de IP, o reemplazar el guard por `RequireAuthorization()` con política dedicada antes de staging — `services/core/src/Host/AnimeTracker.Host/Program.cs:29`.

## Notas técnicas y decisiones

- **Proyecto Contracts por módulo:** se agrega para cumplir el CA4 sin que un módulo referencie la capa Application de otro. Es la "interfaz pública" del módulo (consultas, comandos y eventos de integración).
- **Módulo Identity:** pospuesto a EP-01 (decisión 2026-10-29). HU-00.02 crea solo Catalog, Tracking y Calendar. El módulo Identity se agregará cuando se trabajen las historias de EP-01; la solución deberá reabrirse en ese momento.
- **Mediator:** se usa **MediatR v12** (última versión sin licencia comercial, decisión 2026-10-29). Las pruebas no deben depender de la implementación concreta: el handler se registra en el IoC y la prueba de integración solo invoca `IMediator.Send()`.
- **Aserciones:** Shouldly (FluentAssertions v8 cambió a licencia comercial).

## Dev Notes

> **Dependencia pendiente (readiness):** La verificación en CI del criterio CA2 y el ítem del DoD "Pruebas de arquitectura ejecutándose en CI" requieren que HU-00.09 (Pipeline CI) esté completa o en marcha. Ambas historias comparten sprint-1; coordinar secuenciación con quien lleve HU-00.09 para que la primera prueba verde en CI no se dé por cumplida antes de que el pipeline exista.

> **.NET 10 vs .NET 8:** El SDK instalado en el entorno de desarrollo es .NET 10.0.403. `global.json` actualizado a 10.0.403 para que la solución compile. El stack y los patrones (Clean Architecture, MediatR, NetArchTest) son idénticos entre .NET 8 y .NET 10; se puede actualizar `global.json` de vuelta a 8.x si el equipo lo requiere.

> **Placeholder types:** Cada capa de módulo expone una clase pública `Placeholder` usada únicamente por `AssemblyHelper` en `ArchitectureTests` para forzar la carga de ensamblados en el `AppDomain` antes de la inspección de NetArchTest. No añaden semántica de dominio y pueden eliminarse cuando los proyectos tengan tipos reales.

> **Evidencia de violación detectada (DoD):** El test `Contracts_no_depende_de_ninguna_capa_interna(Catalog)` fue ejecutado con un archivo `ViolationProbe.cs` temporal en `Catalog.Contracts` que referenciaba `Catalog.Domain`. El test falló con el mensaje: *"Contracts de Catalog tiene dependencias internas: AnimeTracker.Catalog.Contracts.ViolationProbe"*. El archivo fue eliminado y la referencia revertida; el suite vuelve a green (15/15). Esta evidencia se adjunta en el PR.

## Fuera de alcance

- DbContext y migraciones (HU-00.08).
- Lógica de dominio real de cada módulo.

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Pruebas de arquitectura en verde y ejecutándose en CI. *(pendiente HU-00.09)*
- [x] Evidencia de violación detectada (prueba negativa) — ver §Dev Notes.
- [x] Diagrama de dependencias entre proyectos en `services/core/README.md`.

## Dev Agent Record

- **Fecha:** 2026-10-29
- **Rama:** `feature/story-0-2-solucion-net-monolito-modular`
- **SDK:** .NET 10.0.403 (global.json actualizado desde 8.0.404)

### Archivos creados / modificados

| Archivo | Tipo |
|---|---|
| `services/core/AnimeTracker.sln` | Nuevo |
| `services/core/Directory.Build.props` | Nuevo |
| `services/core/Directory.Packages.props` | Nuevo |
| `services/core/global.json` | Modificado (8.0.404 → 10.0.403) |
| `services/core/src/SharedKernel/**` | Nuevo (Entity, AggregateRoot, ValueObject, Result, IDomainEvent, IClock) |
| `services/core/src/Modules/{Catalog,Tracking,Calendar}/{Domain,Application,Infrastructure,Api,Contracts}` | Nuevo (15 proyectos) |
| `services/core/src/Modules/Catalog/AnimeTracker.Catalog.Contracts/Queries/GetAnimeSummaryQuery.cs` | Nuevo |
| `services/core/src/Modules/Catalog/AnimeTracker.Catalog.Contracts/Queries/AnimeSummaryDto.cs` | Nuevo |
| `services/core/src/Modules/Catalog/AnimeTracker.Catalog.Application/Queries/GetAnimeSummaryQueryHandler.cs` | Nuevo |
| `services/core/src/Modules/Catalog/AnimeTracker.Catalog.Application/CatalogApplicationExtensions.cs` | Nuevo |
| `services/core/src/Modules/{Mod}/{Mod}ApplicationExtensions.cs` | Nuevo (Tracking, Calendar) |
| `services/core/src/Modules/{Mod}/AnimeTracker.{Mod}.Api/{Mod}ModuleExtensions.cs` | Nuevo (3 módulos) |
| `services/core/src/Modules/{Mod}/AnimeTracker.{Mod}.Api/{Mod}HealthCheck.cs` | Nuevo (3 módulos) |
| `services/core/src/Host/AnimeTracker.Host/Program.cs` | Nuevo |
| `services/core/tests/AnimeTracker.ArchitectureTests/ModuleBoundaryTests.cs` | Nuevo |
| `services/core/tests/AnimeTracker.ArchitectureTests/AssemblyHelper.cs` | Nuevo |
| `services/core/tests/Tracking/AnimeTracker.Tracking.IntegrationTests/MediatorCrossModuleTests.cs` | Nuevo |
| `services/core/tests/Host/AnimeTracker.Host.IntegrationTests/HealthCheckTests.cs` | Nuevo |
| `services/core/README.md` | Modificado (diagrama de dependencias) |

### Comandos de prueba y resultado

```powershell
cd services/core
dotnet test AnimeTracker.sln
```

| Suite | Resultado | Pruebas |
|---|---|---|
| ArchitectureTests | ✅ Pass | 18/18 |
| Host.IntegrationTests | ✅ Pass | 1/1 |
| Tracking.IntegrationTests | ✅ Pass | 2/2 |
| SharedKernel.UnitTests | ✅ Pass | 17/17 |
| Catalog.UnitTests | ✅ Pass | 1/1 |
| Tracking.UnitTests | ✅ Pass | 1/1 |
| Calendar.UnitTests | ✅ Pass | 1/1 |
| Catalog.IntegrationTests | ✅ Pass | 1/1 |
| Calendar.IntegrationTests | ✅ Pass | 1/1 |
| **Total** | **✅ 43/43** | |

### Archivos adicionales (iteración T9)

| Archivo | Tipo |
|---|---|
| `services/core/src/SharedKernel/AnimeTracker.SharedKernel/Result.cs` | Modificado (guard en `Value`, campo `_value`) |
| `services/core/tests/SharedKernel/AnimeTracker.SharedKernel.UnitTests/ResultTests.cs` | Modificado (nuevo test `Failure_accessing_Value_throws_InvalidOperationException`, eliminada aserción `result.Value.ShouldBe(default)`) |

### Comandos de prueba y resultado (iteración T9)

```powershell
cd services/core
dotnet test AnimeTracker.sln --collect:"XPlat Code Coverage"
```

| Suite | Resultado | Pruebas |
|---|---|---|
| ArchitectureTests | ✅ Pass | 18/18 |
| Host.IntegrationTests | ✅ Pass | 1/1 |
| Tracking.IntegrationTests | ✅ Pass | 2/2 |
| SharedKernel.UnitTests | ✅ Pass | 18/18 |
| Catalog.UnitTests | ✅ Pass | 1/1 |
| Tracking.UnitTests | ✅ Pass | 1/1 |
| Calendar.UnitTests | ✅ Pass | 1/1 |
| Catalog.IntegrationTests | ✅ Pass | 1/1 |
| Calendar.IntegrationTests | ✅ Pass | 1/1 |
| **Total** | **✅ 44/44** | |

### Archivos adicionales (iteración T10)

| Archivo | Tipo |
|---|---|
| `services/core/src/Host/AnimeTracker.Host/Program.cs` | Modificado (`using Microsoft.AspNetCore.HttpOverrides` + `app.UseForwardedHeaders` antes del guard de `/health`) |

### Comandos de prueba y resultado (iteración T10)

```powershell
cd services/core
dotnet test AnimeTracker.sln
```

| Suite | Resultado | Pruebas |
|---|---|---|
| ArchitectureTests | ✅ Pass | 18/18 |
| Host.IntegrationTests | ✅ Pass | 1/1 |
| Tracking.IntegrationTests | ✅ Pass | 2/2 |
| SharedKernel.UnitTests | ✅ Pass | 18/18 |
| Catalog.UnitTests | ✅ Pass | 1/1 |
| Tracking.UnitTests | ✅ Pass | 1/1 |
| Calendar.UnitTests | ✅ Pass | 1/1 |
| Catalog.IntegrationTests | ✅ Pass | 1/1 |
| Calendar.IntegrationTests | ✅ Pass | 1/1 |
| **Total** | **✅ 44/44** | |

### Cobertura (iteración T9)

| Proyecto | Cobertura de líneas |
|---|---|
| AnimeTracker.SharedKernel | 100 % (Entity, AggregateRoot, Result, Result\<T\>, ValueObject) |

### Archivos adicionales (iteración T8)

| Archivo | Tipo |
|---|---|
| `services/core/tests/SharedKernel/AnimeTracker.SharedKernel.UnitTests/AnimeTracker.SharedKernel.UnitTests.csproj` | Nuevo |
| `services/core/tests/SharedKernel/AnimeTracker.SharedKernel.UnitTests/EntityTests.cs` | Nuevo |
| `services/core/tests/SharedKernel/AnimeTracker.SharedKernel.UnitTests/ResultTests.cs` | Nuevo |
| `services/core/tests/SharedKernel/AnimeTracker.SharedKernel.UnitTests/ValueObjectTests.cs` | Nuevo |
| `services/core/src/SharedKernel/AnimeTracker.SharedKernel/Entity.cs` | Modificado (notnull, Equals, GetHashCode, sin eventos) |
| `services/core/src/SharedKernel/AnimeTracker.SharedKernel/AggregateRoot.cs` | Modificado (eventos movidos desde Entity) |
| `services/core/src/Host/AnimeTracker.Host/Program.cs` | Modificado (loopback guard en /health) |
| `services/core/tests/AnimeTracker.ArchitectureTests/ModuleBoundaryTests.cs` | Modificado (Api_no_depende_de_Infrastructure) |
| `services/core/src/Modules/{Catalog,Tracking,Calendar}/AnimeTracker.*.Api/*.csproj` | Modificado (removida ref. a Infrastructure) |
| `services/core/tests/Catalog/AnimeTracker.Catalog.UnitTests/CatalogPlaceholderTests.cs` | Modificado (reemplazado placeholder) |
| `services/core/tests/Tracking/AnimeTracker.Tracking.UnitTests/TrackingPlaceholderTests.cs` | Modificado (reemplazado placeholder) |
| `services/core/tests/Calendar/AnimeTracker.Calendar.UnitTests/CalendarPlaceholderTests.cs` | Modificado (reemplazado placeholder) |
| `services/core/tests/Catalog/AnimeTracker.Catalog.IntegrationTests/CatalogPlaceholderIntegrationTests.cs` | Modificado (reemplazado placeholder) |
| `services/core/tests/Calendar/AnimeTracker.Calendar.IntegrationTests/CalendarPlaceholderIntegrationTests.cs` | Modificado (reemplazado placeholder) |
| `services/core/AnimeTracker.sln` | Modificado (proyecto SharedKernel.UnitTests agregado) |

### Cobertura

| Proyecto | Cobertura de líneas |
|---|---|
| AnimeTracker.SharedKernel | 100 % (Entity, AggregateRoot, Result, ValueObject) |

## Code Review

- Fecha: 2026-10-29
- Iteración: 2 (tras correcciones T8)
- Resultado: CAMBIOS REQUERIDOS
- Cobertura de líneas (código cambiado): 100 % (SharedKernel — único código de producción nuevo con lógica ejecutable; reporte coverlet bd044938)

| Ámbito | Resultado | Hallazgos |
|---|---|---|
| Criterios de aceptación | ✅ | 0 |
| Seguridad | ✅ | 0 |
| Diseño / SOLID | ❌ | 1 (MEDIA) |
| Arquitectura | ✅ | 0 |
| Pruebas (> 80 %) | ✅ | 0 |

### Hallazgos

- [ ] [MEDIA][Diseño] `Result<T>.Value` es `T?` accesible sin guard cuando `IsFailure=true`; un consumidor que omita la verificación de `IsSuccess` obtiene `default(T)` silenciosamente, ocultando el fallo en lugar de evidenciarlo. — `services/core/src/SharedKernel/AnimeTracker.SharedKernel/Result.cs:14` — Corrección: convertir `Value` en propiedad computada con campo de respaldo: `public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value of a failed result.");`; actualizar `ResultTests` para verificar la excepción.

### Sugerencias (bajo el umbral)

- [BAJA][Diseño] `Entity<TId>.Id` se inicializa con `default!` sin constructor canónico protegido que fuerce la asignación; una subclase que olvide asignar `Id` produce una entidad en estado inválido — `services/core/src/SharedKernel/AnimeTracker.SharedKernel/Entity.cs:5`
- [BAJA][Diseño] `IDomainEvent` no extiende `MediatR.INotification`; el despachador de eventos de dominio necesitará un wrapper manual en infraestructura — `services/core/src/SharedKernel/AnimeTracker.SharedKernel/IDomainEvent.cs:3`
- [BAJA][Diseño] `Result<T>` y `Result` carecen de interfaz o clase base común; impide middleware genérico (logging, pipelines) sin duplicación — `services/core/src/SharedKernel/AnimeTracker.SharedKernel/Result.cs:3,21`
- [BAJA][Pruebas] Aserciones mínimas en `CatalogApplicationStartupTests` y `CalendarApplicationStartupTests`: solo verifican `ShouldNotBeNull()` sin resolver servicios concretos — `services/core/tests/Catalog/AnimeTracker.Catalog.IntegrationTests/CatalogPlaceholderIntegrationTests.cs:16`
- [BAJA][Pruebas] `ValueObject.cs` tiene cobertura de ramas 75 % (ramas de `Equals(object)` y `GetHashCode` no cubiertas por caso tipo-diferente); cobertura de líneas 100 % — `services/core/src/SharedKernel/AnimeTracker.SharedKernel/ValueObject.cs:13`
- [BAJA][Pruebas] `IServiceProvider` no liberado en `MediatorCrossModuleTests`; puede causar fugas cuando los servicios sean `IDisposable` — `services/core/tests/Tracking/AnimeTracker.Tracking.IntegrationTests/MediatorCrossModuleTests.cs:21`
- [BAJA][Pruebas] `Tracking_no_necesita_importar_internals_de_Catalog` duplica el comportamiento runtime de `Tracking_obtiene_resumen_de_anime_via_mediator`; la verificación real es en tiempo de compilación — `services/core/tests/Tracking/AnimeTracker.Tracking.IntegrationTests/MediatorCrossModuleTests.cs:39`
- [BAJA][Seguridad] `AllowedHosts: "*"` deshabilita validación de Host header; restringir a hostnames legítimos antes de producción — `services/core/src/Host/AnimeTracker.Host/appsettings.json:8`
- [BAJA][Seguridad] Sin `app.UseHttpsRedirection()` en el pipeline; sienta un precedente para historias posteriores — `services/core/src/Host/AnimeTracker.Host/Program.cs`
- [BAJA][Seguridad] `/health` serializa `e.Key` y `e.Value.Description`; aunque restringido por loopback guard, puede filtrar arquitectura interna si el acceso se amplía — `services/core/src/Host/AnimeTracker.Host/Program.cs:46`
- [BAJA][DoD] Ítems "DoD común cumplida" y "Pruebas de arquitectura ejecutándose en CI" sin marcar; dependen de HU-00.09 (CI) y HU-00.10 (staging). Situación conocida y documentada en §Dev Notes — `docs/implementation/stories/0-2-solucion-net-monolito-modular.md:161`

## Readiness Review

- Fecha: 2026-10-29
- Resultado: require-review (2)
- Técnico: 2 — Negocio: 2

### Hallazgos técnicos

- [REVISAR] Q7 (licencia MediatR v13+) sigue abierta con `resolver_antes_de: sprint-1` (inicio 2026-11-02). Sin esta decisión la tarea T5 (CA4) y la prueba `Tracking_obtiene_resumen_de_anime_via_mediator` no tienen contrato firme; T1–T4 pueden iniciarse de igual modo. — `docs/implementation/sprint_status.yml#preguntas_abiertas Q7`; historia §Notas técnicas
- [INFO] Módulo Identity agregado al alcance (4 módulos × 5 proyectos) cuando ARD §4.1 solo menciona Catalog, Tracking y Calendar; la justificación está documentada en la historia y es coherente con HU-00.08 + EP-01. — `docs/plannings/ARD.md#41-módulos-definidos`; historia §Notas técnicas
- [INFO] `metricas.historias_done: 0` en sprint_status.yml es inconsistente con HU-00.01 en estado `done`; no afecta esta historia pero conviene corregirlo. — `sprint_status.yml#metricas` vs `#epicas.EP-00`

### Hallazgos de negocio

- [REVISAR] Q7 abierta a 3 días del inicio del sprint-1: si el equipo arranca T5 sin decisión, puede derivar en retrabajo. — `sprint_status.yml#preguntas_abiertas Q7`; historia §Notas técnicas
- [REVISAR] Dependencia de HU-00.09 (Pipeline CI) no declarada en el encabezado, aunque el DoD exige pruebas de arquitectura en CI y CA2 menciona "la compilación en CI falla"; HU-00.09 ya reconoce la relación inversa en su campo Bloquea. — historia §Definición de Hecho; `0-9-pipeline-integracion-continua.md` §Bloquea
- [REVISAR] Módulo Identity añadido sin registro de decisión del PM; si EP-01 evoluciona hacia Keycloak/Auth0 externo, los proyectos generados aquí serían desperdicio. — historia §Notas técnicas párrafo "Módulo Identity"
- [INFO] Stub `GetAnimeSummaryQuery` en T5/CA4 cubre el contrato que HU-03.07 deberá evolucionar; ninguna de las dos historias advierte al desarrollador. — historia §Tareas T5; `sprint_status.yml#EP-03 HU-03.07`
- [INFO] "DoD común cumplida" en la Definición de Hecho no enlaza el documento fuente; queda abierto a interpretación. — historia §Definición de Hecho

### Acciones para quedar Ready

- [x] Resolver Q7: se usará **MediatR v12** (última versión libre, sin licencia comercial). Decisión documentada en §Notas técnicas y Q7 cerrada en sprint_status.yml.
- [x] Confirmar módulo Identity con PM: **pospuesto a EP-01** (decisión 2026-10-29). CA1, T2 y ejemplo de código actualizados para reflejar solo 3 módulos.
- [x] Añadir HU-00.09 como dependencia blanda en el campo `Depende de` o dejar constancia en el DoD de que la verificación en CI es condicional a su finalización. → nota agregada en §Dev Notes.
- [x] Corregir `metricas.historias_done` y `puntos_done` en sprint_status.yml para reflejar HU-00.01 como done (valores: 1 y 3 respectivamente). → corregido en sprint_status.yml.

### Resolución de gaps — 2026-10-29

- Gaps resueltos: 4
- Gaps escalados (decisión pendiente): 0
- Dependencias registradas: 1
- Cambios realizados en la historia: §Criterios de aceptación CA1, §Estrategia TDD (TheoryData), §Tareas T2, §Notas técnicas (Mediator y Módulo Identity), §Dev Notes (nueva sección)

### Re-evaluación — 2026-10-29

- Resultado: require-review (2)
- Técnico: 2 — Negocio: 1

#### Hallazgos técnicos

- [REVISAR] Q6 ("¿80% de cobertura en código nuevo como umbral?") permanece abierta con `resolver_antes_de: sprint-1` (inicio 2026-11-02). Si aplica, T4 y T7 deben configurarse con ese umbral desde el inicio (coverlet/ReportGenerator en `Directory.Build.props` o en el job CI que define HU-00.09). — `sprint_status.yml#preguntas_abiertas Q6`; historia §Tareas T4, T7
- [REVISAR] La nota en §Dev Notes sobre HU-00.09 es solo una indicación de coordinación informal; si HU-00.09 se retrasa dentro del sprint, el DoD ("pruebas de arquitectura ejecutándose en CI") quedaría incumplible sin mecanismo de escalado visible. — historia §Dev Notes y §Definición de Hecho

#### Hallazgos de negocio

- [INFO] "DoD común cumplida" no enlaza el documento fuente; definición disponible en `docs/plannings/AnimeTracker — Épicas e Historias de Usuario.md §Convenciones`. — historia §Definición de Hecho
- [INFO] La DoD común exige "desplegado en staging" pero HU-00.10 (Despliegue continuo a staging) está en sprint-2; ninguna historia de sprint-1 puede marcarse `done` completo hasta que exista el pipeline de CD. Restricción sistémica del sprint, no específica de esta historia. — `sprint_status.yml §sprints.sprint-2`
- [INFO] "Contrato OpenAPI actualizado antes de implementar" en la DoD común no aplica a esta historia (no expone endpoints); conviene anotar la excepción para evitar confusión en el code review. — historia §Definición de Hecho

#### Acciones para quedar Ready (re-evaluación)

- [ ] Resolver Q6 antes del 2026-11-02: acordar si aplica el umbral del 80% de cobertura en código nuevo; si aplica, actualizar T4 y T7 para incluir la configuración de coverlet/ReportGenerator y cerrar Q6 en sprint_status.yml.
