---
name: story-readiness
description: Valida si una historia de usuario de docs/implementation/stories está lista para desarrollarse, con una revisión técnica y una de negocio ejecutadas en paralelo, y actualiza su estado en sprint_status.yml a ready-for-dev, require-review o blocked. Usar cuando se pida validar, evaluar o revisar la preparación (readiness) de una historia.
argument-hint: <story_number>
---

# Story Readiness

Entrada: `$ARGUMENTS` = número de historia (ej. `3`, `1.3`, `1-3`).

## Rutas

| Recurso | Ruta |
|---|---|
| Planeación (PRD, arquitectura, épicas, UX) | `docs/planning/` |
| Historias | `docs/implementation/stories/` |
| Estado del sprint | `docs/implementation/sprint_status.yml` |

## Estados

| Valor | Estado en sprint_status.yml | Significado |
|---|---|---|
| 1 | `ready-for-dev` | Se puede empezar sin preguntas abiertas |
| 2 | `require-review` | Hay ambigüedades o vacíos que conviene resolver, pero no impiden del todo |
| 3 | `blocked` | Algo impide empezar (dependencia, decisión pendiente, contradicción) |

**Resultado final = máximo(resultado técnico, resultado negocio).**

## Pasos

1. **Localizar la historia.** Busca con Glob en `docs/implementation/stories/` archivos cuyo nombre contenga el número (prueba variantes `1.3`, `1-3`, `1_3`). Si no hay coincidencia o hay varias, detente y pregunta.

2. **Leer el estado actual** en `sprint_status.yml` y ubicar la clave de la historia. Si ya está en `in-progress`, `review`, `pull-request` o `done`, informa y detente: la readiness no retrocede una historia en curso, salvo que el usuario lo pida explícitamente.

3. **Inventariar contexto.** Lista (sin leer completos) los archivos de `docs/planning/` y las demás historias. Identifica los relevantes (PRD, arquitectura, épica de la historia, UX, estándares). Estas rutas se pasan a los revisores.

4. **Lanzar las dos revisiones en paralelo.** En UN SOLO mensaje, haz dos llamadas a la herramienta Agent (subagent_type `general-purpose`), una por ámbito:
   - Técnico: prompt = contenido de `reviewers/technical.md` (en el directorio de esta skill)
   - Negocio: prompt = contenido de `reviewers/business.md`

   Sustituye `{story_path}`, `{planning_paths}`, `{stories_dir}` y `{sprint_status_path}`. Los revisores son de solo lectura.

5. **Consolidar.**
   - Extrae `RESULTADO` de cada respuesta.
   - Si un revisor falla o no respeta el formato, toma su resultado como `2`.
   - Final = máximo de ambos.

6. **Registrar en la historia.** Agrega o reemplaza (no dupliques) la sección al final del archivo:

   ```markdown
   ## Readiness Review
   - Fecha: YYYY-MM-DD
   - Resultado: <estado> (<valor>)
   - Técnico: <valor> — Negocio: <valor>

   ### Hallazgos técnicos
   - [BLOQUEANTE|REVISAR|INFO] <hallazgo> — <evidencia>

   ### Hallazgos de negocio
   - [BLOQUEANTE|REVISAR|INFO] <hallazgo> — <evidencia>

   ### Acciones para quedar Ready
   - [ ] ...
   ```

7. **Actualizar `sprint_status.yml`.** Cambia solo el valor de la clave de esta historia con Edit (no reescribas el archivo; conserva comentarios, orden e indentación). Si existe un campo `last_updated`, actualízalo.

8. **Responder** con el resultado final, el de cada ámbito y las acciones necesarias (máximo 5 viñetas).

## Reglas

- No inventes requisitos: si algo no está en la historia ni en planeación, es un hallazgo, no una suposición.
- Cada hallazgo cita evidencia (archivo y sección).
- No modifiques código ni otros documentos.
