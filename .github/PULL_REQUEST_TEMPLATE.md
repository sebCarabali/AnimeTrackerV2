## Descripción

<!-- Describe brevemente los cambios de este PR y el motivo. -->

## Tipo de cambio

- [ ] `feat` — nueva funcionalidad
- [ ] `fix` — corrección de error
- [ ] `refactor` — refactorización sin cambio funcional
- [ ] `test` — adición o corrección de pruebas
- [ ] `docs` — cambios de documentación
- [ ] `ci` — cambios en el pipeline de CI
- [ ] `chore` — tareas de mantenimiento

## Checklist — Contratos OpenAPI primero

- [ ] ¿Se actualizó el contrato antes del código? (Si el PR cambia la API, el contrato `bff.v1.yaml` o `core.v1.yaml` se modificó primero.)
- [ ] ¿Pasó el lint de Spectral (`pnpm contracts:lint`) sin errores?
- [ ] ¿Se verificó que no existen breaking changes no intencionados (oasdiff)?
- [ ] Si se añadió un nuevo endpoint, ¿se agregó una prueba de contrato en `.IntegrationTests`?

## Checklist general

- [ ] Build pasa (`dotnet build` / `pnpm build`) sin advertencias.
- [ ] Todas las pruebas pasan.
- [ ] Cobertura de líneas ≥ 80 % en código nuevo.
- [ ] No se bypasearon hooks (`--no-verify`).
