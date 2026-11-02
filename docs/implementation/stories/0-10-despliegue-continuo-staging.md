# HU-00.10 · Despliegue continuo a staging

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-10-despliegue-continuo-staging` |
| Tipo | Habilitadora |
| Prioridad | Should |
| Estimación | 5 pts |
| Sprint | sprint-2 |
| Estado | ready-for-dev |
| Depende de | HU-00.06, HU-00.09, HU-00.12 |
| Bloquea | HU-07.01, HU-07.04 |

## Historia

**Como** equipo, **quiero** que cada merge a `main` publique imágenes y despliegue en staging, **para** validar los cambios en un entorno real.

## Contexto

- La DoD común incluye "desplegado en el entorno de staging".
- La prueba de carga (HU-07.01) y las e2e (HU-07.04) se ejecutan contra staging.
- **Plataforma de staging (Q5) cerrada el 2026-11-01:** servidor único self-hosted (VM con Docker Engine + Docker Compose), sin suscripciones adicionales. Decisión del PM junto al Architect tras comparar con Azure Container Apps y AKS; detalle completo en `sprint_status.yml` (preguntas_abiertas → Q5).

## Criterios de aceptación

```gherkin
Característica: Despliegue continuo a staging

  Escenario: CA1 — Imágenes versionadas
    Dado un merge a "main"
    Cuando termina el pipeline de entrega
    Entonces las imágenes de "bff", "core" y "migrator" se publican en el registro
    Y cada imagen tiene las etiquetas "sha-<commit>" y "main"
    Y la imagen incluye las etiquetas OCI de origen y revisión

  Escenario: CA2 — Despliegue automático
    Dado imágenes nuevas publicadas
    Cuando termina el despliegue
    Entonces staging ejecuta las versiones "sha-<commit>"
    Y las migraciones se aplican antes de que el núcleo reciba tráfico

  Escenario: CA3 — Verificación post-despliegue
    Dado un despliegue terminado
    Cuando se ejecutan las pruebas de humo contra staging
    Entonces "GET /health/ready" del BFF responde 200
    Y el endpoint de versión devuelve el commit desplegado

  Escenario: CA4 — Reversión ante health checks fallidos
    Dado un despliegue cuya nueva versión no pasa los health checks
    Cuando vence el tiempo de espera
    Entonces staging mantiene o restaura la versión anterior
    Y el pipeline queda en rojo con un aviso al equipo

  Escenario: CA5 — Despliegues serializados
    Dado dos merges seguidos a "main"
    Cuando corren sus pipelines
    Entonces los despliegues no se solapan y se aplica el más reciente
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA3 | Integración | `Version_endpoint_devuelve_commit_de_la_build` (core) y `GET /version` (BFF) | xUnit / Jest |
| 2 | CA3 | Humo | `smoke-staging.sh` falla si `/health/ready` ≠ 200 o la versión no coincide | Bash |
| 3 | CA4 | Aceptación | Despliegue de una imagen de prueba que nunca pasa health → se mantiene la versión anterior | Ejecución manual documentada |
| 4 | CA1 | Pipeline | Verificar etiquetas de la imagen publicada | `docker buildx imagetools inspect` |

## Tareas

- [ ] T1 (CA3) Endpoint `GET /version` en BFF y núcleo (commit, fecha de build en UTC) alimentado por build args.
- [ ] T2 (CA1) Workflow `cd-staging.yml` disparado por push a `main`; build multiplataforma con `docker/build-push-action` y publicación en **GitHub Container Registry (ghcr.io)** (gratis, sin suscripción nueva) con tags `sha-<commit>` y `main`.
- [ ] T3 (CA2) Paso de despliegue vía `appleboy/ssh-action` contra el servidor único: `docker compose -f docker-compose.yml -f docker-compose.staging.yml pull`, luego `migrator` como `run --rm` (bloqueante, antes de levantar `core`/`bff`), y finalmente `up -d`.
- [ ] T4 (CA4) Rollback scripted: antes del `pull`, guardar el tag vigente (`LAST_GOOD_SHA` en el servidor); si el smoke test post-deploy (T6) falla, reintentar `up -d` con ese tag anterior; si vuelve a fallar, el job queda en rojo y avisa al equipo (sin plataforma gestionada que lo haga nativo).
- [ ] T5 (CA5) `concurrency: group: deploy-staging, cancel-in-progress: false` en el workflow, más `flock /var/run/deploy.lock` envolviendo el script en el servidor como defensa adicional ante despliegues concurrentes.
- [ ] T6 (CA3) `smoke-staging.sh` post-despliegue (`/health/ready` y `GET /version`) y notificación al canal del equipo.
- [ ] T7 Entorno `staging` en GitHub con secretos de despliegue: `SSH_HOST`, `SSH_USER`, `SSH_PRIVATE_KEY` y credenciales de `ghcr.io` si el `pull` en el servidor requiere autenticación (ver HU-00.12).
- [ ] T8 (CA3) Configurar **Caddy o Traefik** en el servidor como reverse proxy: TLS automático, servir el build estático de la SPA y rutear `/api/*` al BFF.

## Notas técnicas y decisiones

- **Q5 cerrada (2026-11-01):** servidor único self-hosted, sin suscripciones adicionales (descartadas Azure Container Apps y AKS por costo/alcance frente al requisito explícito del PM). Trade-off aceptado por el equipo: sin alta disponibilidad, sin autoescalado, sin SLA; parcheo de SO/Docker y mantenimiento de los scripts de T4/T5 quedan a cargo del equipo.
- La SPA se despliega en el mismo servidor (T8, Caddy/Traefik sirviendo el build estático) sin elevar la estimación — ya no se evalúa plataforma separada ni corte de alcance.
- Base reutilizada de HU-00.06 (T8 de esa historia: `docker-compose.staging.yml`).

## Fuera de alcance

- Producción y aprobaciones manuales.
- Alta disponibilidad, autoescalado o SLA de staging (aceptado como riesgo, ver `sprint_status.yml` → riesgo R-01).

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Tres despliegues consecutivos exitosos a staging.
- [ ] Prueba de reversión (CA4) documentada con evidencia.
