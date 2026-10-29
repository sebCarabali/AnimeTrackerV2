# HU-00.06 · Entorno local con Docker Compose

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-6-entorno-local-docker-compose` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 3 pts |
| Sprint | sprint-2 |
| Estado | ready-for-dev |
| Depende de | HU-00.02, HU-00.03, HU-00.08, HU-00.12 |
| Bloquea | HU-00.10, HU-07.04 |

## Historia

**Como** desarrollador, **quiero** levantar PostgreSQL, Redis y los servicios con un solo comando, **para** reproducir el sistema completo en mi máquina.

## Contexto

- ARD §2: PostgreSQL (transaccional) y Redis (caché) son parte del sistema desde el día uno.
- El objetivo de EP-00 es "un comando y todo corre".

## Criterios de aceptación

```gherkin
Característica: Entorno local reproducible

  Escenario: CA1 — Un comando levanta todo
    Dado Docker instalado y un archivo ".env" creado desde ".env.example"
    Cuando ejecuto "docker compose -f infra/docker-compose.yml up --wait"
    Entonces arrancan "postgres", "redis", "core" y "bff"
    Y todos quedan en estado "healthy" en menos de 3 minutos en una máquina de referencia

  Escenario: CA2 — Orden de arranque por salud
    Dado PostgreSQL aún no está listo
    Cuando arranca el entorno
    Entonces "core" no inicia hasta que "postgres" y "redis" estén "healthy"
    Y "bff" no inicia hasta que "core" esté "healthy"

  Escenario: CA3 — Migraciones y seed en base vacía
    Dado una base de datos vacía
    Cuando arranca el entorno
    Entonces se aplican las migraciones de todos los módulos
    Y se cargan usuarios y animes de prueba

  Escenario: CA4 — Seed idempotente
    Dado un entorno ya inicializado con datos de prueba
    Cuando reinicio el entorno
    Entonces no se duplican usuarios ni animes

  Escenario: CA5 — Persistencia y limpieza
    Dado datos creados durante el desarrollo
    Cuando ejecuto "docker compose down" y vuelvo a levantar
    Entonces los datos se conservan
    Pero con "docker compose down -v" se eliminan y el siguiente arranque reaplica el seed
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA3 | Integración | `Seeder_inserta_usuarios_y_animes_en_base_vacia` | xUnit + Testcontainers.PostgreSql |
| 2 | CA4 | Integración | `Seeder_ejecutado_dos_veces_no_duplica_registros` | xUnit + Testcontainers |
| 3 | CA3 | Integración | `Seeder_no_corre_fuera_de_Development` | xUnit |
| 4 | CA1, CA2 | Humo | `smoke.sh`: `up --wait` y luego `GET bff/health` y `core/health` = 200 | Bash en job de CI |

Ejemplo (rojo):

```csharp
[Fact]
public async Task Seeder_ejecutado_dos_veces_no_duplica_registros()
{
    await using var db = await _fixture.CreateMigratedDatabaseAsync();
    var seeder = new DevelopmentDataSeeder(db.Catalog, db.Identity, _clock);

    await seeder.SeedAsync();
    await seeder.SeedAsync();

    (await db.Catalog.Animes.CountAsync()).ShouldBe(DevelopmentDataSeeder.AnimeCount);
}
```

## Tareas

- [ ] T1 (CA1) `infra/docker-compose.yml` con servicios `postgres` (16+), `redis` (7+), `core`, `bff`; volúmenes con nombre.
- [ ] T2 (CA2) `healthcheck` en cada servicio (`pg_isready`, `redis-cli ping`, `curl /health`) y `depends_on: condition: service_healthy`.
- [ ] T3 (CA3) Servicio `migrator` (EF Core migration bundle) que corre una vez antes de `core`.
- [ ] T4 (CA3, CA4) `DevelopmentDataSeeder` idempotente (upsert por clave natural) que solo corre en `Development`.
- [ ] T5 (CA1) `.env.example` con todas las variables y valores locales no sensibles.
- [ ] T6 (CA1) Script `infra/scripts/smoke.sh` y job de CI que lo ejecuta.
- [ ] T7 Documentar en el README raíz: arranque, reinicio limpio y puertos expuestos.

## Notas técnicas y decisiones

- La SPA y la extensión se ejecutan fuera de Compose (`ng serve`, build de la extensión) para tener recarga rápida; Compose cubre backend y datos.
- El seed usa datos ficticios con `ExternalId` reales de prueba solo si no viola términos de la API externa.

## Fuera de alcance

- Entorno de staging (HU-00.10).
- Colector de telemetría (se agrega en HU-00.11).

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Job de humo de Compose en verde en CI.
- [ ] Probado en Linux y en Windows o macOS por dos personas distintas.
