# infra — Infraestructura

**Propósito:** Configuración de infraestructura y despliegue. Contiene los archivos Docker Compose para el entorno local, manifiestos de Kubernetes (o equivalente) para staging/producción y scripts de CI/CD.

**Dueño:** DevOps / Backend team

## Contenido

```
infra/
  docker/         # Dockerfiles por servicio
  compose/        # Docker Compose para entorno local y staging
  k8s/            # Manifiestos de Kubernetes (staging/producción)
  scripts/        # Scripts de despliegue y mantenimiento
```

## Arranque del entorno local

```bash
# Desde la raíz del monorepo:
docker compose -f infra/compose/docker-compose.local.yml up -d
```

Ver el README raíz y HU-00.06 (Entorno local con Docker Compose) para instrucciones detalladas.
