---
name: story-party
description: Sesión colaborativa donde cinco agentes especializados (Product Manager, Business Analyst, UX Designer, Developer, Tester) analizan simultáneamente una historia de usuario, identificando gaps en temas propuestos por el usuario o detectados automáticamente. Genera un Party Report con perspectivas múltiples. Usar cuando se quiera un análisis profundo y multidimensional de una historia en cualquier punto de su ciclo de vida.
argument-hint: <story_number> [--topics "tema1, tema2, ..."]
---

# Story Party

Entrada: `$ARGUMENTS` = número de historia; opcionalmente `--topics "tema1, tema2, ..."`.

## Rutas

| Recurso | Ruta |
|---|---|
| Planeación | `docs/planning/` |
| Historias | `docs/implementation/stories/` |
| Estado del sprint | `docs/implementation/sprint_status.yml` |

## Concepto

Party Mode es una sesión de análisis colaborativo donde cinco agentes especializados examinan simultáneamente una historia desde sus perspectivas únicas. El objetivo es descubrir gaps, ambigüedades, riesgos y oportunidades de mejora antes de que lleguen a desarrollo o a producción. Se puede ejecutar en cualquier estado de la historia.

## Pasos

### 1. Inicializar la sesión

1. Parsea `$ARGUMENTS`:
   - Extrae el número de historia.
   - Si hay `--topics "..."`, extrae la lista de temas. De lo contrario, `TEMAS = auto`.
2. Localiza el archivo de la historia (Glob con variantes `1.3`, `1-3`, `1_3`). Si no hay coincidencia única, pregunta.
3. Lee la historia completa, incluidas las secciones `Readiness Review`, `Code Review` y `Party Session` previas si existen.
4. Lista los archivos de `docs/planning/` e identifica los relevantes (PRD, arquitectura, UX, épica de la historia). Lee los relevantes.

### 2. Determinar los temas de análisis

**Si el usuario proporcionó `--topics`:** usa esa lista directamente.

**Si `TEMAS = auto`:** analiza la historia buscando las áreas con mayor ambigüedad o riesgo. Señales a detectar:

| Señal | Tipo de gap |
|---|---|
| ACs sin métricas, vagos o no verificables | Requisitos |
| Reglas de negocio con condiciones incompletas | Reglas de negocio |
| Flujos de error o casos límite no descritos | Casos de borde |
| Requisitos no funcionales ausentes (rendimiento, seguridad, i18n, accesibilidad) | NFR |
| UX/interfaz no definida cuando la historia implica UI | UX |
| Dependencias técnicas o de negocio sin resolver | Dependencias |
| Preguntas abiertas o suposiciones sin responder | Ambigüedades |
| Alcance inusualmente grande o confuso | Tamaño/scope |

Selecciona los **3 a 5 temas** con mayor impacto potencial. Anuncia la lista en una línea antes de lanzar los agentes:

```
Temas de análisis: [tema1], [tema2], [tema3], ...
```

### 3. Lanzar los agentes de la party en paralelo

En **UN SOLO mensaje**, lanza cinco llamadas a la herramienta Agent, una por rol:

| Agente | subagent_type | Perspectiva |
|---|---|---|
| Product Manager | `product-manager` | Valor de negocio, alcance, priorización, métricas de éxito, riesgos de producto |
| Business Analyst | `business-analyst` | Completitud de requisitos, ACs, reglas de negocio, trazabilidad con PRD |
| UX Designer | `ux-designer` | Flujos de usuario, estados de pantalla, microcopy, accesibilidad WCAG 2.2 AA |
| Developer | `developer` | Factibilidad técnica, complejidad de implementación, dependencias, deuda técnica |
| Tester | `tester` | Testabilidad, casos de borde, estrategia de pruebas, riesgos de calidad |

**Prompt para cada agente** (sustituye `{ROL}`, `{story_path}`, `{planning_paths}` y `{topics}`):

```
Estás participando en una sesión Party Mode de análisis de historia de usuario.
Tu rol en esta sesión es: {ROL}

Historia a analizar: {story_path}
Documentos de planeación: {planning_paths}
Temas prioritarios de esta party: {topics}

Instrucciones:
1. Lee la historia completa y los documentos de planeación relevantes para tu rol.
2. Analiza la historia EXCLUSIVAMENTE desde tu perspectiva de {ROL}.
3. Presta especial atención a los temas prioritarios listados.
4. Los agentes son de SOLO LECTURA: no modifiques ningún archivo.

Responde con exactamente este formato:

ROL: {ROL}
PERSPECTIVA_GENERAL: [1–2 oraciones que resumen tu visión de la historia]

GAPS_IDENTIFICADOS:
- [CRITICO|ALTO|MEDIO|BAJO] <descripción del gap> — Impacto: <consecuencia si no se resuelve>

PREGUNTAS_ABIERTAS:
- <pregunta concreta que el equipo debe responder antes o durante el desarrollo>

RECOMENDACIONES:
- <mejora concreta y accionable>

TEMAS_CUBIERTOS:
- [nombre del tema]: <hallazgo específico sobre ese tema, o "sin gap detectado">

PUNTUACION_READINESS: <número del 1 al 5, donde 1=bloqueante crítico, 3=con gaps importantes, 5=lista para dev>
JUSTIFICACION_PUNTUACION: <1 oración explicando el puntaje>
```

Si no hay gaps: `GAPS_IDENTIFICADOS: ninguno`.
Si no hay preguntas: `PREGUNTAS_ABIERTAS: ninguna`.

### 4. Consolidar el party report

Una vez que todos los agentes respondan:

1. **Agrupar gaps por severidad**: reúne todos los `CRITICO` primero, luego `ALTO`, `MEDIO`, `BAJO`. Elimina duplicados manteniendo el de mayor severidad.
2. **Detectar convergencia**: gap mencionado por ≥ 3 agentes → marca como `[CONVERGENTE]` y eleva un nivel de severidad si no es ya `CRITICO`.
3. **Detectar conflictos**: perspectivas contradictorias (p. ej., Dev dice "implementación directa" vs. BA dice "reglas de negocio no definidas") → anótalas en la sección de conflictos.
4. **Calcular Puntuación Party**: promedio de los cinco `PUNTUACION_READINESS`. Interpreta:
   - 4.5–5.0 → Historia lista, gaps menores.
   - 3.0–4.4 → Historia con gaps que conviene resolver antes de dev.
   - 1.0–2.9 → Historia con gaps críticos; se recomienda no iniciar dev.

### 5. Registrar en la historia

Agrega o reemplaza la sección `## Party Session` al final del archivo de la historia usando Edit:

```markdown
## Party Session
- Fecha: YYYY-MM-DD
- Temas analizados: [lista]
- Puntuación Party: X.X / 5 — [interpretación en 1 línea]

### Product Manager
[respuesta completa del agente PM]

### Business Analyst
[respuesta completa del agente BA]

### UX Designer
[respuesta completa del agente UX]

### Developer
[respuesta completa del agente Dev]

### Tester
[respuesta completa del agente Tester]

---

### Gaps convergentes (≥ 3 perspectivas)
- [CRITICO|ALTO|MEDIO|BAJO] [CONVERGENTE] <descripción> — Roles: [lista]

### Conflictos entre perspectivas
- <perspectiva A> vs. <perspectiva B>: <descripción del conflicto>

### Resumen ejecutivo
[Top 5 gaps o hallazgos más críticos con el rol que los identificó]

### Acciones recomendadas
- [ ] <acción> — Responsable: [ROL] — Urgencia: [CRITICO|ALTO|MEDIO|BAJO]
```

### 6. Responder

Responde con un resumen estructurado:

1. **Puntuación Party** y su interpretación.
2. **Top 5 gaps** más críticos (convergentes primero).
3. **3 acciones más urgentes** con responsable sugerido.
4. **Siguiente paso recomendado**:
   - Puntuación < 3 → Resolver gaps antes de continuar; sugiere agendar refinamiento.
   - Puntuación 3–4.4 → `/story-readiness <numero>` para validación formal.
   - Puntuación ≥ 4.5 → `/story-dev <numero>` si el estado lo permite.

## Reglas

- Los agentes son de SOLO LECTURA durante la party; solo el orquestador escribe la sección `## Party Session` en la historia.
- Si un agente responde en formato inválido, incluye su respuesta en bruto en la sección correspondiente marcada con `[FORMATO INVÁLIDO]` y toma su puntuación como 2.5.
- La party se puede ejecutar en cualquier estado de la historia; no modifica `sprint_status.yml`.
- Si hay una `## Party Session` previa, reemplázala por completo; no la acumules.
- Si la historia no tiene criterios de aceptación, anúncialo como gap `CRITICO` antes de lanzar los agentes, pero continúa la party.
