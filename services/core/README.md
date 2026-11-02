# services/core — Backend .NET 10 (Monolito Modular)

**Propósito:** Núcleo de negocio de la plataforma. Implementado como un monolito modular en .NET 10 siguiendo Clean Architecture y DDD. Contiene los módulos de Catálogo, Seguimiento (Tracking) y Calendario.

**Dueño:** Backend team

## Stack

- .NET 10 (C#)
- MediatR v12 (comunicación entre módulos)
- Entity Framework Core (HU-00.08)
- PostgreSQL / Redis (HU-00.08 / HU-00.06)

## Módulos

| Módulo | Responsabilidad |
|--------|----------------|
| `Catalog` | Metadatos de animes, integración con API externa (Jikan/AniList) |
| `Tracking` | Biblioteca personal, estados, contadores de episodios |
| `Calendar` | Cronogramas de simulcasts y cálculo de horarios |

## Dependencias entre proyectos

```
AnimeTracker.Host
 ├── AnimeTracker.{Mod}.Api
 │     ├── AnimeTracker.{Mod}.Application
 │     │     ├── AnimeTracker.{Mod}.Domain
 │     │     │     └── AnimeTracker.SharedKernel
 │     │     └── AnimeTracker.{Mod}.Contracts  (solo MediatR + SharedKernel)
 │     └── AnimeTracker.{Mod}.Infrastructure
 │           ├── AnimeTracker.{Mod}.Application
 │           └── AnimeTracker.{Mod}.Domain

Cross-module (solo a través de Contracts):
  AnimeTracker.Tracking.Application  --> AnimeTracker.Catalog.Contracts
  AnimeTracker.Calendar.Application  --> AnimeTracker.Catalog.Contracts
```

**Regla:** ningún módulo puede referenciar el Domain, Application, Infrastructure o Api de otro módulo. Solo puede referenciar su capa `Contracts`. Esta regla está verificada por `tests/AnimeTracker.ArchitectureTests`.

## Pruebas

```powershell
# Desde services/core/
dotnet test AnimeTracker.sln
```

| Suite | Propósito |
|-------|-----------|
| `AnimeTracker.ArchitectureTests` | Fronteras de Clean Architecture (NetArchTest) |
| `AnimeTracker.Host.IntegrationTests` | GET /health con WebApplicationFactory |
| `AnimeTracker.Tracking.IntegrationTests` | Query cross-módulo vía IMediator |
| `AnimeTracker.{Mod}.UnitTests` | Pruebas unitarias por módulo |
| `AnimeTracker.{Mod}.IntegrationTests` | Pruebas de integración por módulo |

## Arranque local

```bash
# Requiere .NET 10 SDK y Docker (para PostgreSQL y Redis — HU-00.06)
# Desde la raíz del monorepo:
docker compose up -d db redis
dotnet run --project services/core/src/Host/AnimeTracker.Host
```

## Deuda Técnica — Sprint-2

### T5 (Testcontainers)

**Descripción:** Las pruebas de integración (`AnimeTracker.Host.IntegrationTests`, módulos `*.IntegrationTests`) requieren una instancia PostgreSQL activa en `localhost:5432`. En ambientes de CI/CD, esto obliga a usar Docker Compose dentro del pipeline.

**Objetivo sprint-2:** Implementar una fixture basada en **Testcontainers** que lance un contenedor PostgreSQL ephemeral por prueba, permitiendo ejecución autónoma de la suite sin dependencias externas.

**Impacto:** 
- ✅ CI/CD independiente de infraestructura externa
- ✅ Tests más rápidos (contenedor por test en paralelo)
- ✅ Validación de migraciones garantizada en cada ejecución

**Referencia:** [Testcontainers.DotNet](https://dotnet.testcontainers.com/)
