---
name: story-dev
description: Desarrolla una historia de usuario de docs/implementation/stories - la pasa a in-progress en sprint_status.yml, crea las tareas de desarrollo si no existen, implementa cada tarea marcándola como hecha, genera pruebas unitarias y e2e, y al terminar pasa la historia a review. Usar cuando se pida desarrollar, implementar o continuar una historia.
argument-hint: <story_number>
---

# Story Dev

Entrada: `$ARGUMENTS` = número de historia.

## Rutas

| Recurso | Ruta |
|---|---|
| Planeación | `docs/planning/` |
| Historias | `docs/implementation/stories/` |
| Estado del sprint | `docs/implementation/sprint_status.yml` |

## Estados

| Uso | Valor |
|---|---|
| Estados de entrada permitidos | `ready-for-dev`, `in-progress` (continuar o corregir hallazgos de review) |
| Al iniciar | `in-progress` |
| Al finalizar | `review` |

## Pasos

### 1. Preparación
1. Localiza el archivo de la historia (Glob con variantes `1.3`, `1-3`, `1_3`). Si no hay una coincidencia única, pregunta.
2. Lee `sprint_status.yml`. Si el estado no es de entrada permitido, informa y detente (por ejemplo: `require-review`/`blocked` → sugiere ejecutar `/story-readiness`; `review` o posteriores → ya fue desarrollada).
3. Lee la historia completa, incluidas las secciones `Readiness Review` y `Code Review` si existen. Si hay una sección `Code Review` con hallazgos abiertos, esos hallazgos son la prioridad de esta ejecución.
4. Lee los documentos de planeación relevantes (arquitectura, estándares de código, stack) y explora el código existente para seguir sus convenciones: estructura de paquetes/proyectos, naming, frameworks de prueba, build (Maven/Gradle, `dotnet`), etc.
5. Si el repo es git, crea o cambia a la rama `feature/story-<numero>-<slug>` desde la rama principal actualizada (sin descartar cambios locales del usuario; si hay cambios sin commit ajenos a la historia, pregunta).

### 2. Marcar en progreso
Con Edit, cambia solo el valor de la historia en `sprint_status.yml` a `in-progress` (conserva formato y comentarios).

### 3. Tareas
Si la historia no tiene una sección `## Tasks` (o `## Tareas`), créala derivando las tareas de los criterios de aceptación y del diseño técnico. Cada tarea:

```markdown
## Tasks
- [ ] T1: <tarea> (AC: 1, 2)
  - [ ] T1.1: <subtarea>
- [ ] T2: Pruebas unitarias de <componente> (AC: …)
- [ ] T3: Pruebas e2e de <flujo> (AC: …)
```

Reglas: cada criterio de aceptación queda cubierto por al menos una tarea; siempre hay tareas explícitas de pruebas unitarias y e2e. Si las tareas ya existen, no las reescribas; añade solo las que falten. Refleja también las tareas en la lista de tareas de la sesión (TaskCreate) para que el usuario vea el avance.

### 4. Desarrollo (por cada tarea, en orden)
1. Implementa siguiendo la arquitectura y los principios SOLID; reutiliza lo existente antes de crear abstracciones nuevas.
2. Escribe las pruebas unitarias de lo implementado (preferible antes o junto al código):
   - Java: JUnit 5 + Mockito (AssertJ si ya se usa).
   - C#/.NET: el framework ya presente (xUnit/NUnit/MSTest) + Moq/NSubstitute.
   - Otros stacks: el framework que ya use el repo.
3. Compila y ejecuta las pruebas afectadas. No marques la tarea si algo falla.
4. Marca la tarea `[x]` en el archivo de la historia en cuanto pase, y actualiza la tarea de la sesión.
5. Anota en `## Dev Notes` (créala si no existe) decisiones relevantes, supuestos y desviaciones.

### 5. Pruebas e2e
Implementa pruebas e2e que recorran los criterios de aceptación desde el punto de entrada real (API o UI), usando la herramienta ya presente en el repo (por ejemplo REST Assured / Spring Boot Test / Testcontainers, `WebApplicationFactory` en ASP.NET Core, Playwright/Selenium/Cypress para UI). Si el repo no tiene ninguna, elige la más coherente con el stack y regístralo en Dev Notes.

### 6. Verificación final
1. Ejecuta la suite completa (unitarias + e2e) y el build; todo debe pasar sin pruebas deshabilitadas para "hacer pasar" el build.
2. Mide la cobertura de líneas del código nuevo/modificado (JaCoCo, coverlet u equivalente). Objetivo ≥ 80 %; si no se alcanza, añade pruebas antes de continuar.
3. Verifica que todas las tareas estén en `[x]` y que cada criterio de aceptación tenga prueba.
4. Si es git, haz commits atómicos con mensajes descriptivos que referencien la historia (`story <numero>: …`). No hagas push.

### 7. Cerrar
1. Agrega a la historia una sección `## Dev Agent Record` con: archivos creados/modificados, comandos de prueba, resultado y porcentaje de cobertura.
2. Cambia el estado en `sprint_status.yml` a `review`.
3. Responde con un resumen breve: tareas completadas, pruebas añadidas, cobertura y siguiente paso (`/story-review <numero>`).

## Reglas
- Nunca pases a `review` con tareas sin marcar, pruebas fallando o build roto: deja la historia en `in-progress` y explica qué falta.
- Si una tarea requiere una decisión que la historia y la planeación no resuelven, pregunta en lugar de inventar.
- No modifiques el alcance de la historia ni los criterios de aceptación.
