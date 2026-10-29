---
name: story-pr
description: Crea el pull request de una historia de usuario que ya pasó code review (estado pull-request en sprint_status.yml), con descripción generada desde la historia, y al crearlo pasa la historia a done. Soporta GitHub, Azure DevOps y GitLab. Usar cuando se pida abrir o generar el PR de una historia.
argument-hint: <story_number>
---

# Story PR

Entrada: `$ARGUMENTS` = número de historia.

## Estados

| Uso | Valor |
|---|---|
| Estado de entrada requerido | `pull-request` |
| Al crear el PR | `done` |

## Pasos

1. **Validar entrada.**
   - Localiza la historia y confirma que su estado en `sprint_status.yml` es `pull-request`. Si no, informa y detente (sugiere `/story-review`).
   - Confirma que la sección `## Code Review` de la historia dice `APROBADA`.
   - Verifica que es un repositorio git y que estás en la rama de la historia (`feature/story-<numero>-…`). Si estás en la rama principal, detente y pregunta.

2. **Preparar la rama.**
   - `git status`: si hay cambios sin commit relacionados con la historia (por ejemplo, el propio archivo de la historia o `sprint_status.yml`), haz commit; si hay cambios ajenos, pregunta.
   - Detecta la rama base (`main`, `master` o `develop`, según el remote/HEAD por defecto o lo que indique la planeación).
   - Actualiza con la base (`git fetch` + rebase o merge según la convención del repo). Si hay conflictos, detente y reporta.
   - Ejecuta una última vez el build y las pruebas; si fallan, detente sin crear el PR.

3. **Actualizar el estado antes de publicar.** Cambia la historia a `done` en `sprint_status.yml` (Edit, solo esa clave) y haz commit `story <numero>: mark as done`, para que el cambio de estado viaje dentro del PR.

4. **Publicar la rama**: `git push -u origin <rama>`.

5. **Construir la descripción del PR** a partir de la historia:

   ```markdown
   ## Historia <numero>: <título>
   <Como … quiero … para …>

   ## Cambios
   - <resumen por componente/archivo principal>

   ## Criterios de aceptación
   - [x] AC1 … — prueba: `<Clase#metodo>`

   ## Pruebas
   - Unitarias: N | E2E: M | Cobertura líneas cambiadas: NN %
   - Comando: `<comando de pruebas>`

   ## Code review
   - Resultado: APROBADA (fecha)
   - Sugerencias pendientes (bajo el umbral de bloqueo):
     - <copiar de la sección "Sugerencias" de la historia, o "ninguna">


   ## Notas
   - <decisiones de Dev Notes, migraciones, configuración necesaria>
   ```

   Termina la descripción con las líneas de atribución que indique la configuración del entorno, si las hay.

6. **Crear el PR** según el remote (`git remote get-url origin`):

   | Plataforma | Comando |
   |---|---|
   | GitHub | `gh pr create --base <base> --head <rama> --title "<numero>: <título>" --body-file <archivo>` |
   | Azure DevOps | `az repos pr create --source-branch <rama> --target-branch <base> --title "<numero>: <título>" --description "@<archivo>"` |
   | GitLab | `glab mr create --source-branch <rama> --target-branch <base> --title "<numero>: <título>" --description "$(cat <archivo>)"` |

   Si el CLI no está instalado o autenticado, no inventes alternativas: revierte el commit de estado del paso 3 si aún no se publicó o, si ya se publicó, déjalo y entrega al usuario el título, la descripción y la URL para crear el PR manualmente; informa que la historia queda en `done` solo cuando confirme que el PR existe.

7. **Registrar** en la historia una sección `## Pull Request` con la URL y la fecha (commit + push de ese cambio).

8. **Responder** con la URL del PR y el estado final de la historia.

## Reglas
- Nunca hagas merge del PR, ni force-push sobre ramas compartidas, ni push a la rama principal.
- No crees el PR si el build o las pruebas fallan.
