---
name: story-review
description: Hace el code review de una historia de usuario en estado review lanzando agentes en paralelo que validan criterios de aceptación y requisitos, seguridad, patrones de diseño y principios SOLID, alineación a la arquitectura y cobertura de pruebas unitarias mayor al 80 %. Sin hallazgos pasa la historia a pull-request; con hallazgos la devuelve a in-progress. Usar cuando se pida revisar el código de una historia.
argument-hint: <story_number>
---

# Story Review

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
| Estado de entrada requerido | `review` |
| Sin hallazgos | `pull-request` |
| Con hallazgos | `in-progress` |

## Severidades y umbral de bloqueo

Orden: `CRITICA` > `ALTA` > `MEDIA` > `BAJA`.

**`UMBRAL_BLOQUEO = MEDIA`** ← ajusta este valor (`BAJA` = estricto, `ALTA` = permisivo).

- Severidad ≥ umbral → **hallazgo**: impide pasar a `pull-request` y genera tarea de corrección.
- Severidad < umbral → **sugerencia**: se registra en la historia y en el PR, no bloquea.
- Siempre bloquean, sin importar el umbral: cobertura ≤ 80 %, criterio de aceptación sin implementar o sin prueba, vulnerabilidad explotable.

## Pasos

1. **Validar entrada.** Localiza la historia y confirma que su estado en `sprint_status.yml` es `review`. Si no, informa y detente.

2. **Determinar el alcance del cambio.**
   - Si es git: rama de la historia vs. rama base (`git merge-base`), `git diff --name-only <base>...HEAD`. Guarda el diff completo en un archivo temporal del scratchpad para que los revisores lo lean.
   - Si no es git: usa la lista de archivos de la sección `Dev Agent Record` de la historia.

3. **Ejecutar pruebas y cobertura una vez** (para que los revisores no compitan por el build): ejecuta la suite y genera el reporte de cobertura (JaCoCo para Maven/Gradle, `dotnet test --collect:"XPlat Code Coverage"` + coverlet para .NET, o lo que use el repo). Guarda la salida y la ruta del reporte.

4. **Lanzar cinco revisores en paralelo.** En UN SOLO mensaje, cinco llamadas a la herramienta Agent, cada una con el agente especializado correspondiente:

   | Revisor | subagent_type |
   |---|---|
   | Criterios de aceptación y requisitos | `story-reviewer-acceptance` |
   | Seguridad | `story-reviewer-security` |
   | Patrones de diseño y SOLID | `story-reviewer-design` |
   | Alineación a la arquitectura | `story-reviewer-architecture` |
   | Pruebas y cobertura > 80 % | `story-reviewer-tests` |

   Prompt para cada agente (omite los campos que no apliquen al revisor):
   ```
   Historia: {story_path}
   Planeación: {planning_paths}
   Archivos cambiados: {changed_files}
   Diff: {diff_path}
   Salida de pruebas: {test_output_path}
   Reporte de cobertura: {coverage_report_path}
   ```

   Los revisores son de solo lectura.

5. **Consolidar.**
   - Une los hallazgos y elimina duplicados (mismo archivo/línea y misma causa → conserva la severidad mayor).
   - Si un revisor falla o no responde en el formato, repítelo una vez; si vuelve a fallar, regístralo como hallazgo `ALTA` ("revisión de <ámbito> no completada").
   - Verifica tú mismo cualquier hallazgo `CRITICA`/`ALTA` dudoso leyendo el código antes de aceptarlo; descarta falsos positivos dejando constancia.

6. **Registrar en la historia.** Agrega o reemplaza la sección `## Code Review`:

   ```markdown
   ## Code Review
   - Fecha: YYYY-MM-DD
   - Resultado: APROBADA | CAMBIOS REQUERIDOS
   - Cobertura de líneas (código cambiado): NN %

   | Ámbito | Resultado | Hallazgos |
   |---|---|---|
   | Criterios de aceptación | ✅/❌ | n |
   | Seguridad | ✅/❌ | n |
   | Diseño / SOLID | ✅/❌ | n |
   | Arquitectura | ✅/❌ | n |
   | Pruebas (> 80 %) | ✅/❌ | n |

   ### Hallazgos
   - [ ] [ALTA][Seguridad] <descripción> — `ruta/Archivo.java:42` — Corrección sugerida: …

   ### Sugerencias (bajo el umbral)
   - [BAJA][Diseño] <descripción> — `ruta/Archivo.cs:17`
   ```

7. **Transición de estado** (Edit sobre `sprint_status.yml`, solo la clave de esta historia):
   - Sin hallazgos (ninguna observación ≥ `UMBRAL_BLOQUEO` ni de las que siempre bloquean) → `pull-request`.
   - Con hallazgos → `in-progress`, y agrega a `## Tasks` una tarea `- [ ] Corregir hallazgos de code review` con un ítem por hallazgo.

8. **Responder** con el resultado, la tabla por ámbito y el siguiente paso (`/story-pr <numero>` o `/story-dev <numero>`).

## Reglas
- La review no modifica código de producción ni pruebas; solo la historia y `sprint_status.yml`.
- La cobertura ≤ 80 % es siempre hallazgo `ALTA`.
- Un criterio de aceptación sin implementar o sin prueba es siempre hallazgo `ALTA`.
- Una vulnerabilidad explotable es siempre hallazgo, como mínimo `ALTA`.
