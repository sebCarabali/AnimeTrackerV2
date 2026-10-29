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
- **Plataforma de staging sin definir** en PRD/ARD; los criterios son independientes de la plataforma.

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
- [ ] T2 (CA1) Workflow `cd-staging.yml` disparado por push a `main`; build multiplataforma con `docker/build-push-action` y caché.
- [ ] T3 (CA2) Paso de despliegue según la plataforma elegida; `migrator` como tarea previa.
- [ ] T4 (CA4) Estrategia de despliegue con health checks (rolling o blue/green) y reversión.
- [ ] T5 (CA5) `concurrency: group: deploy-staging, cancel-in-progress: false`.
- [ ] T6 (CA3) Pruebas de humo post-despliegue y notificación al canal del equipo.
- [ ] T7 Entorno `staging` en GitHub con secretos de despliegue (ver HU-00.12).

## Notas técnicas y decisiones

- **Pregunta abierta:** plataforma de staging (p. ej. Azure Container Apps, Kubernetes, VM con Compose). Resolver en el refinamiento del sprint 2; la estimación asume una plataforma gestionada de contenedores.
- La SPA se publica como sitio estático; incluir su despliegue si la plataforma lo permite sin elevar la estimación, si no, crear historia aparte.

## Fuera de alcance

- Producción y aprobaciones manuales.

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Tres despliegues consecutivos exitosos a staging.
- [ ] Prueba de reversión (CA4) documentada con evidencia.
