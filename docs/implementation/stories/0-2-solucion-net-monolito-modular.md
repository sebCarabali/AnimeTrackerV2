# HU-00.02 · Solución .NET 8 como monolito modular [Técnica]

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-2-solucion-net-monolito-modular` |
| Tipo | Técnica |
| Prioridad | Must |
| Estimación | 5 pts |
| Sprint | sprint-1 |
| Estado | ready-for-dev |
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
    Entonces existen los módulos Catalog, Tracking, Calendar e Identity
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
    public static TheoryData<string> Modules => new() { "Catalog", "Tracking", "Calendar", "Identity" };

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

- [ ] T1 (CA1) Crear `AnimeTracker.sln`, `Directory.Build.props` (net8.0, `Nullable` enable, `TreatWarningsAsErrors`, `ImplicitUsings`) y `Directory.Packages.props` (versiones centralizadas).
- [ ] T2 (CA1) Generar los 4 módulos × 5 proyectos con referencias permitidas: Api → Application → Domain; Infrastructure → Application/Domain; Contracts sin dependencias internas.
- [ ] T3 (CA1) Crear `SharedKernel` (Entity, AggregateRoot, ValueObject, Result, IDomainEvent, IClock).
- [ ] T4 (CA2, CA3) Crear `tests/AnimeTracker.ArchitectureTests` con las pruebas anteriores.
- [ ] T5 (CA4) Definir `GetAnimeSummaryQuery` en Catalog.Contracts con handler stub en Catalog.Application; prueba de integración desde Tracking.
- [ ] T6 (CA5) `Host`: método de extensión `AddXxxModule()` / `MapXxxModule()` por módulo; health checks.
- [ ] T7 Crear proyectos de prueba por módulo: `AnimeTracker.{Modulo}.UnitTests` e `IntegrationTests`.

## Notas técnicas y decisiones

- **Proyecto Contracts por módulo:** se agrega para cumplir el CA4 sin que un módulo referencie la capa Application de otro. Es la "interfaz pública" del módulo (consultas, comandos y eventos de integración).
- **Módulo Identity:** el backlog lista solo Catalog, Tracking y Calendar, pero HU-00.08 pide el esquema `identity` y EP-01 un módulo de identidad; se incluye aquí para no reabrir la solución después.
- **Mediator:** MediatR v13+ requiere licencia comercial según el tamaño de la organización. Validar licencia o usar la alternativa con generador de código `Mediator` (martinothamar) o un despachador propio en SharedKernel. Las pruebas no deben depender de la librería elegida.
- **Aserciones:** Shouldly (FluentAssertions v8 cambió a licencia comercial).

## Fuera de alcance

- DbContext y migraciones (HU-00.08).
- Lógica de dominio real de cada módulo.

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Pruebas de arquitectura en verde y ejecutándose en CI.
- [ ] Evidencia en el PR de una violación detectada (prueba negativa).
- [ ] Diagrama de dependencias entre proyectos en `services/core/README.md`.
