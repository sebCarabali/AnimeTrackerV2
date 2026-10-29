---
name: story-gaps
description: Resuelve los gaps pendientes de un Readiness Review en una historia de usuario, actualiza el documento con las correcciones necesarias y re-ejecuta /story-readiness para validar que los gaps quedaron cerrados. Usar cuando una historia quedó en require-review o blocked tras el story-readiness y se quieren resolver los hallazgos antes de pasar a dev.
argument-hint: <story_number>
---

# Story Gaps

Entrada: `$ARGUMENTS` = número de historia (ej. `3`, `1.3`, `1-3`).

## Rutas

| Recurso | Ruta |
|---|---|
| Planeación (PRD, arquitectura, épicas, UX) | `docs/planning/` |
| Historias | `docs/implementation/stories/` |
| Estado del sprint | `docs/implementation/sprint_status.yml` |

## Condición de entrada

La historia debe tener una sección `## Readiness Review` con al menos una acción `- [ ]` sin marcar en `### Acciones para quedar Ready`. Si no hay gaps abiertos, informa que la historia ya está lista y detente.

## Pasos

### 1. Localizar y leer la historia

1. Busca con Glob en `docs/implementation/stories/` archivos cuyo nombre contenga el número (prueba variantes `1.3`, `1-3`, `1_3`). Si no hay coincidencia única, pregunta.
2. Lee el archivo completo de la historia.
3. Lee `sprint_status.yml` y verifica el estado actual.
4. Si el estado es `in-progress`, `review`, `pull-request` o `done`, informa que la historia está en curso y detente: no es el momento de resolver gaps de readiness.

### 2. Extraer gaps pendientes

Extrae todas las acciones sin marcar (`- [ ]`) de la sección `### Acciones para quedar Ready`. Para cada gap:

- Identifica el hallazgo asociado (BLOQUEANTE o REVISAR) en las secciones `### Hallazgos técnicos` y `### Hallazgos de negocio`.
- Clasifica el tipo de resolución:

| Tipo | Descripción | Acción |
|---|---|---|
| `DOCUMENTACIÓN` | Falta definir o clarificar algo en la historia (AC, regla de negocio, flujo de error, NFR, out-of-scope, etc.) | El skill lo resuelve leyendo planeación y completando el campo en la historia |
| `AMBIGÜEDAD` | Hay una decisión de negocio o técnica abierta que requiere input del equipo | El skill la presenta al usuario y espera respuesta antes de continuar |
| `DEPENDENCIA` | Otra historia o recurso externo debe estar disponible primero | Registra la dependencia como nota y marca el gap como parcialmente resuelto hasta que la dependencia esté lista |

### 3. Leer contexto de planeación relevante

Lista los archivos de `docs/planning/` sin leerlos todos. Identifica cuáles son relevantes para los gaps encontrados (PRD, arquitectura, épica de la historia, UX, estándares). Léelos antes de resolver.

### 4. Resolver gaps de tipo DOCUMENTACIÓN

Para cada gap de tipo `DOCUMENTACIÓN`, en orden de severidad (BLOQUEANTE primero):

1. Determina qué sección de la historia debe actualizarse (criterios de aceptación, reglas de negocio, alcance, requisitos no funcionales, diseño técnico, etc.).
2. Redacta el contenido que resuelve el gap, basándote exclusivamente en la planeación existente. No inventes requisitos: si la planeación no cubre el punto, trátalo como `AMBIGÜEDAD`.
3. Usa Edit para agregar o completar la sección correspondiente en la historia. No reescribas contenido que ya existe; solo añade lo que falta.
4. Marca la acción como completada en `### Acciones para quedar Ready` cambiando `- [ ]` por `- [x]`.

### 5. Escalar gaps de tipo AMBIGÜEDAD

Para cada gap de tipo `AMBIGÜEDAD`, presenta al usuario la pregunta concreta que debe responder:

```
GAP ABIERTO — requiere decisión del equipo:
  Hallazgo: <descripción del hallazgo>
  Pregunta: <qué debe decidirse>
  Opciones sugeridas (si las hay): <opción A / opción B>
```

Espera la respuesta del usuario. Una vez recibida, documenta la decisión en la sección correspondiente de la historia y marca la acción como `- [x]`.

### 6. Registrar gaps de tipo DEPENDENCIA

Para cada gap de tipo `DEPENDENCIA`, agrega una nota en `## Dev Notes` (créala si no existe):

```markdown
> **Dependencia pendiente (readiness):** <descripción de la dependencia que debe resolverse antes de iniciar dev>
```

Marca la acción como `- [x]` indicando que fue registrada (no bloqueará el readiness si las demás dimensiones están listas).

### 7. Actualizar la sección Readiness Review

Al final de la sección `## Readiness Review` existente, agrega un bloque de seguimiento:

```markdown
### Resolución de gaps — YYYY-MM-DD
- Gaps resueltos: <n>
- Gaps escalados (decisión pendiente): <n>
- Dependencias registradas: <n>
- Cambios realizados en la historia: <lista de secciones modificadas>
```

### 8. Re-ejecutar la validación

Una vez resueltos o escalados todos los gaps, invoca el skill `story-readiness` pasando el mismo número de historia:

```
/story-readiness <numero>
```

El skill actualizará el resultado en `sprint_status.yml` y en la sección `## Readiness Review` de la historia con el nuevo estado.

### 9. Responder

Responde con un resumen de tres partes:

1. **Gaps resueltos**: lista de acciones completadas con la sección que se actualizó.
2. **Gaps escalados**: preguntas que requieren decisión del equipo (si las hay).
3. **Resultado tras re-validación**: el nuevo estado de la historia (`ready-for-dev`, `require-review` o `blocked`) y el siguiente paso sugerido.

## Reglas

- Solo modifica el archivo de la historia y `sprint_status.yml` (vía la re-ejecución de `story-readiness`). No toques código de producción ni otros documentos.
- No inventes requisitos: cada adición a la historia debe poder citarse en un documento de planeación.
- Si todos los gaps son de tipo `AMBIGÜEDAD` o `DEPENDENCIA` y no hay nada que resolver sin input externo, infórmalo antes del paso 4 y solicita las respuestas al usuario en un solo bloque.
- Si el re-readiness devuelve el mismo estado `blocked` o `require-review`, lista los hallazgos que permanecen y por qué no pudieron resolverse automáticamente.
