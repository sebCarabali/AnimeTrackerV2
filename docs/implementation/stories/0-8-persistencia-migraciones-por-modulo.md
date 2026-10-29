# HU-00.08 · Persistencia y migraciones por módulo [Técnica]

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-8-persistencia-migraciones-por-modulo` |
| Tipo | Técnica |
| Prioridad | Must |
| Estimación | 3 pts |
| Sprint | sprint-1 |
| Estado | ready-for-dev |
| Depende de | HU-00.02 |
| Bloquea | HU-00.06, HU-07.02, EP-01 a EP-05 (persistencia) |

## Historia

**Como** desarrollador backend, **quiero** que cada módulo tenga su propio esquema de PostgreSQL y sus migraciones de EF Core, **para** que los módulos no compartan tablas.

## Contexto

- ARD §4.2: los módulos no comparten tablas; la separación debe permitir extraer un módulo a microservicio.
- Un DbContext y un historial de migraciones por módulo hacen esa extracción mecánica.

## Criterios de aceptación

```gherkin
Característica: Un esquema y un DbContext por módulo

  Escenario: CA1 — Esquemas creados por migraciones
    Dado una base PostgreSQL vacía
    Cuando se aplican las migraciones de todos los módulos
    Entonces existen los esquemas "catalog", "tracking", "calendar" e "identity"
    Y cada esquema tiene su propia tabla "__EFMigrationsHistory"

  Escenario: CA2 — Un DbContext por módulo
    Dado la solución
    Cuando reviso la capa Infrastructure de cada módulo
    Entonces existe exactamente un DbContext por módulo con su esquema por defecto
    Y el DbContext es "internal" a su módulo

  Escenario: CA3 — Ningún módulo mapea tablas de otro esquema
    Dado el modelo de EF Core de un módulo
    Cuando una entidad se mapea a una tabla de un esquema distinto al del módulo
    Entonces las pruebas de arquitectura fallan indicando la entidad y el esquema

  Escenario: CA4 — Migraciones independientes
    Dado una migración nueva en el módulo Tracking
    Cuando se aplica
    Entonces solo cambia el esquema "tracking"
    Y las migraciones de otros módulos no se regeneran

  Escenario: CA5 — Convenciones comunes
    Dado cualquier DbContext
    Cuando se construye el modelo
    Entonces las tablas y columnas usan snake_case
    Y las propiedades DateTime/DateTimeOffset se mapean a "timestamptz"
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA3 | Modelo | `Todas_las_entidades_usan_el_esquema_del_modulo(dbContextType, schema)` | xUnit (metadatos de EF, sin base) |
| 2 | CA5 | Modelo | `Columnas_de_fecha_son_timestamptz(dbContextType)` | xUnit |
| 3 | CA2 | Arquitectura | `Cada_modulo_tiene_un_solo_DbContext_interno` | NetArchTest |
| 4 | CA1 | Integración | `Migraciones_crean_los_cuatro_esquemas` | xUnit + Testcontainers.PostgreSql |
| 5 | CA4 | Integración | `Cada_esquema_tiene_su_historial_de_migraciones` | Testcontainers |

Ejemplo (rojo):

```csharp
public static TheoryData<Type, string> Contexts => new()
{
    { typeof(CatalogDbContext),  "catalog"  },
    { typeof(TrackingDbContext), "tracking" },
    { typeof(CalendarDbContext), "calendar" },
    { typeof(IdentityDbContext), "identity" },
};

[Theory, MemberData(nameof(Contexts))]
public void Todas_las_entidades_usan_el_esquema_del_modulo(Type contextType, string schema)
{
    using var ctx = DbContextFactory.CreateDesignTime(contextType);

    var foreign = ctx.Model.GetEntityTypes()
        .Where(e => e.GetSchema() != schema)
        .Select(e => e.Name)
        .ToList();

    foreign.ShouldBeEmpty($"Entidades fuera del esquema '{schema}'");
}
```

## Tareas

- [ ] T1 (CA2) Crear `CatalogDbContext`, `TrackingDbContext`, `CalendarDbContext`, `IdentityDbContext` con `HasDefaultSchema`.
- [ ] T2 (CA1, CA4) Registrar cada uno con `MigrationsHistoryTable("__EFMigrationsHistory", schema)` y `MigrationsAssembly` del módulo.
- [ ] T3 (CA5) Convención compartida en SharedKernel/Infrastructure: `UseSnakeCaseNamingConvention()` (EFCore.NamingConventions) y mapeo de fechas a `timestamptz`.
- [ ] T4 (CA1) Migración inicial vacía por módulo (`InitialCreate`).
- [ ] T5 (CA1, CA4) Fixture compartido de Testcontainers en `tests/AnimeTracker.Testing`.
- [ ] T6 Comando del Host `--migrate` o bundle para el servicio `migrator` de HU-00.06.

## Notas técnicas y decisiones

- Las consultas entre módulos se resuelven vía Contracts/Mediator o proyecciones (ARD §4.2), nunca con joins entre esquemas.
- Un usuario de base por módulo con permisos solo sobre su esquema es deseable para producción; se registra como mejora en EP-07.

## Fuera de alcance

- Entidades de negocio (se crean en cada épica).
- Redis (se configura con el primer uso real en EP-02).

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Pruebas de modelo y de integración en verde en CI.
- [ ] Guía "cómo agregar una migración a un módulo" en `services/core/README.md`.
