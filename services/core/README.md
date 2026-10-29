# services/core — Backend .NET 8 (Monolito Modular)

**Propósito:** Núcleo de negocio de la plataforma. Implementado como un monolito modular en .NET 8 siguiendo Clean Architecture y DDD. Contiene los módulos de Catálogo, Seguimiento (Tracking) y Calendario.

**Dueño:** Backend team

## Stack

- .NET 8 (C#)
- Entity Framework Core
- PostgreSQL
- Redis

## Módulos

| Módulo | Responsabilidad |
|--------|----------------|
| `Catalog` | Metadatos de animes, integración con API externa (Jikan/AniList) |
| `Tracking` | Biblioteca personal, estados, contadores de episodios |
| `Calendar` | Cronogramas de simulcasts y cálculo de horarios |

## Arranque local

```bash
# Requiere .NET 8 SDK y Docker (para PostgreSQL y Redis)
# Desde la raíz del monorepo:
docker compose up -d db redis
dotnet run --project services/core/src/AnimeTracker.Api
```

Ver el README raíz para instrucciones completas del entorno local.
