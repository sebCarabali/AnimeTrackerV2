# HU-00.09 · Pipeline de integración continua

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-9-pipeline-integracion-continua` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 5 pts |
| Sprint | sprint-1 |
| Estado | ready-for-dev |
| Depende de | HU-00.01 |
| Bloquea | HU-00.02 a HU-00.12 (sus criterios "en CI"), HU-00.10 |

## Historia

**Como** equipo, **quiero** que cada PR compile, pruebe y analice solo los proyectos afectados, **para** detectar errores antes del merge sin ralentizar el flujo.

## Contexto

- La DoD común exige pruebas unitarias y de integración en verde en CI.
- Varias historias de EP-00 tienen criterios del tipo "CI falla si…"; por eso esta historia va en el primer sprint y cada historia posterior agrega su job.
- Plataforma asumida: **GitHub Actions** (confirmar).

## Criterios de aceptación

```gherkin
Característica: CI por proyectos afectados

  Escenario: CA1 — Solo se ejecuta lo afectado
    Dado un PR que solo modifica archivos en "apps/spa"
    Cuando corre CI
    Entonces se ejecutan build, lint y pruebas de la SPA
    Y no se ejecutan los jobs del núcleo .NET ni del BFF

  Escenario: CA2 — Cambios en contratos afectan a sus consumidores
    Dado un PR que modifica "contracts/internal/core.v1.yaml"
    Cuando corre CI
    Entonces se ejecutan los jobs de contratos, del núcleo .NET y del BFF

  Escenario: CA3 — Cambios transversales ejecutan todo
    Dado un PR que modifica "Directory.Packages.props", "pnpm-lock.yaml" o ".github/workflows"
    Cuando corre CI
    Entonces se ejecutan todos los jobs

  Escenario: CA4 — Una prueba fallida bloquea el merge
    Dado un PR con una prueba en rojo
    Cuando termina CI
    Entonces el check requerido queda en "failure"
    Y la protección de rama impide el merge

  Escenario: CA5 — Umbral de cobertura
    Dado un PR cuya cobertura de líneas en código nuevo es menor al 80 %
    Cuando termina CI
    Entonces el check de cobertura falla y el merge queda bloqueado

  Escenario: CA6 — Tiempo de retroalimentación
    Dado un PR que afecta a un solo proyecto
    Cuando corre CI con caché caliente
    Entonces termina en menos de 10 minutos
```

## Estrategia TDD

Un pipeline se verifica con **PRs de prueba** y con la salida de la herramienta de afectados; se definen antes de escribir el workflow.

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA1–CA3 | Unitaria | `affected.spec.ts`: dado un conjunto de archivos cambiados, devuelve los jobs esperados | Vitest sobre `tools/ci/affected.ts` |
| 2 | CA4 | Aceptación | PR de verificación con prueba fallida intencional → check rojo y merge bloqueado | GitHub Actions |
| 3 | CA5 | Aceptación | PR con código sin pruebas → check de cobertura rojo | Coverlet + ReportGenerator / c8 |
| 4 | CA6 | Medición | Registrar la duración de 5 PRs de un solo proyecto | Resumen del job |

Ejemplo (rojo):

```ts
it.each([
  [['apps/spa/src/app/app.ts'], ['spa']],
  [['contracts/internal/core.v1.yaml'], ['contracts', 'core', 'bff']],
  [['Directory.Packages.props'], ['contracts', 'core', 'bff', 'spa', 'extension']],
])('cambios %j ejecutan %j', (files, jobs) => {
  expect(affectedJobs(files).sort()).toEqual(jobs.sort());
});
```

## Tareas

- [x] T1 (CA1–CA3) `tools/ci/affected.ts` con el mapa archivo → job y sus pruebas (o adoptar Nx `affected` si se elige Nx para el monorepo).
- [x] T2 Workflow `ci.yml` con job `detect` que publica la matriz de jobs afectados.
- [x] T3 Jobs: `core` (restore, build, test con cobertura), `bff`, `spa`, `extension`, `contracts`, `repo-checks`.
- [x] T4 Caché de NuGet y pnpm store.
  - [ ] T4b Caché de Angular (`.angular/cache`) — se añade en HU-00.04 al inicializar la SPA.
- [x] T5 (CA5) Publicar cobertura y aplicar umbral del 80 % — step `Check .NET coverage threshold (80 %)` en job `core`; `thresholds` en `vitest.config.ts` para TypeScript.
- [x] T6 (CA4) Configurar protección de rama `main`: checks requeridos, 1 aprobación, rama actualizada. *(Documentada en §Notas técnicas; se aplica en GitHub manualmente al crear el PR.)*
- [x] T7 Job agregador `ci-ok` que es el único check requerido (evita mantener la lista de checks en la protección de rama cuando la matriz cambia).
- [x] T10 Corregir hallazgos de code review (iteración 3):
  - [x] H1: desplegar workflow, ejecutar PR de verificación CA4 (prueba fallida intencional) y PR CA5 (código sin cobertura); confirmar bloqueo de merge; enlazar URLs en §Definición de Hecho y marcar los dos ítems de DoD pendientes.
- [x] T9 Corregir hallazgos de code review (iteración 2):
  - [x] H1: cambiar `node-version: '22'` → `'24'` en jobs `detect` y `run-job` de `ci.yml`.
  - [x] H2: iterar todos los XMLs de cobertura (no solo el primero) en el step `Check .NET coverage threshold`.
  - [x] H3: desmarcar T4 o añadir sub-ítem T4b indicando que caché Angular se añade en HU-00.04.
  - [x] H4: extraer lógica de fallback de `compute-matrix.ts` a función pura `computeMatrix` en `affected.ts` + 2 tests; guard `GITHUB_OUTPUT` antes de `appendFileSync`; `compute-matrix.ts` excluido del umbral de cobertura en `vitest.config.ts`.
  - [x] H5: desplegar workflow, ejecutar PRs de verificación CA4/CA5 y enlazar URLs en §Definición de Hecho.
- [x] T8 Corregir hallazgos de code review (iteración 1):
  - [x] H1: `compute-matrix.ts` ejecutado con `node --experimental-strip-types` en job `detect`; se elimina el heredoc con `affected.js`.
  - [x] H2: `pnpm --filter @animetracker/ci test:coverage` añadido al step `repo-checks`.
  - [x] H3: `ci-ok` verifica `needs.detect.result == 'success'` antes de verificar `run-job`.
  - [x] H4: paso `Check .NET coverage threshold (80 %)` con script Python en job `core`; `thresholds: { lines: 80, branches: 80 }` en `vitest.config.ts`.
  - [x] H5: ejecutar PRs de verificación CA4/CA5 y enlazar URLs en §Definición de Hecho (requiere workflow desplegado).
  - [x] H6: `permissions: contents: read` añadido en `ci.yml`.

## Notas técnicas y decisiones

- **Umbral de cobertura (Q6 cerrada):** 80 % de líneas en código nuevo; sin umbral global inicial.
- **Herramienta de afectados:** `tools/ci/affected.ts` custom (TypeScript + Vitest), siguiendo el patrón de `tools/repo-checks`. No se adopta Nx para evitar una nueva dependencia del monorepo.
- **Plataforma CI:** GitHub Actions confirmada (el repositorio ya corre en GitHub).
- **SDK .NET:** el job `core` usa `dotnet-version: '10.x'` (no .NET 8 como menciona el ARD; global.json actualizado en HU-00.02).
- **T6 — Branch protection:** configuración manual en GitHub → Settings → Branches → Add rule `main`:
  - Required status check: `ci-ok`
  - Require branches to be up to date: ✓
  - Require 1 approving review: ✓
  - Include administrators: ✓
- El job `ci-ok` resuelve el problema de checks requeridos que no se ejecutan cuando un proyecto no está afectado (jobs de la matriz que no corren dejan el check en "skipped" — `ci-ok` los absorbe).

## Fuera de alcance

- Despliegue (HU-00.10).
- Escaneo de secretos (se agrega como job en HU-00.12).

## Definición de Hecho

- [x] DoD común cumplida.
- [x] Evidencia de los PRs de verificación (CA4, CA5) enlazada en el PR de la historia.
  - PR #5 (CA4 — prueba fallida): https://github.com/sebCarabali/AnimeTrackerV2/pull/5 → `ci-ok` **fail** ✅
  - PR #6 (CA5 — cobertura insuficiente): https://github.com/sebCarabali/AnimeTrackerV2/pull/6 → `ci-ok` **fail** ✅
- [x] Sección "CI" en el README raíz con cómo reproducir cada job localmente.

## Pull Request

- URL: https://github.com/sebCarabali/AnimeTrackerV2/pull/8
- Fecha: 2026-10-31
- Rama: `feature/story-0-9-pipeline-integracion-continua` → `dev`

## Readiness Review

- Fecha: 2026-10-31
- Resultado: require-review (2)
- Técnico: 2 — Negocio: 2

### Hallazgos técnicos

- [REVISAR] Q6 (umbral de cobertura 80 %) sigue abierta en sprint_status.yml; CA5 y T5 no pueden cerrarse sin esta decisión. Sprint-1 arranca 2026-11-02. — `sprint_status.yml:237` (`resolver_antes_de: sprint-1`) y `historia §Notas técnicas` ("Confirmar con el equipo")
- [REVISAR] Decisión Nx vs. `tools/ci/affected.ts` custom diferida desde HU-00.01 pero sin resolver en esta historia; elegirla al inicio cambia T1–T3 de forma significativa. — `0-1-estructura-del-monorepo.md:114` y `historia §Tareas T1`
- [INFO] Dependencia HU-00.01 en estado `done`; sin bloqueo técnico. — `sprint_status.yml:119`
- [INFO] ARD menciona ".NET 8" pero el SDK en uso es .NET 10.0.403 (actualizado en HU-00.02); el job `core` debe apuntar a .NET 10. — `MEMORY.md` y `sprint_status.yml:121`
- [INFO] GitHub Actions asumida como plataforma CI pero no confirmada formalmente; trivial dado que el repo ya corre en GitHub. — `historia §Contexto`
- [INFO] No existe `.github/workflows/` en el repositorio; esta historia parte de cero.

### Hallazgos de negocio

- [REVISAR] Q6 abierta afecta directamente CA5 (umbral bloqueante para el merge); sin decisión el criterio queda indeterminado. — `sprint_status.yml#preguntas_abiertas Q6`
- [REVISAR] T1 sin decisión de herramienta (Nx vs. scripts): la elección determina dependencias del monorepo y puede impactar HU-00.07 y HU-00.12. — `historia §Tareas T1`
- [INFO] Jobs `bff`, `spa`, `extension` definidos en T3 mientras HU-00.03–05 también son sprint-1; pueden iniciarse como stubs. — `historia §Tareas T3`
- [INFO] CA6 (<10 min) se mide observacionalmente (5 PRs), no tiene check automático en CI. — `historia §Estrategia TDD #4`

### Acciones para quedar Ready

- [x] Resolver Q6 antes del 2026-11-02: confirmar umbral 80 % de cobertura en código nuevo; cerrar Q6 en sprint_status.yml y actualizar CA5/T5 con el número definitivo. → Resuelto al iniciar story-dev.
- [x] Decidir Nx vs. `tools/ci/affected.ts` como primera microtarea de T1 (antes de escribir código); documentar en §Notas técnicas y ajustar T2/T3 si se elige Nx. → `tools/ci/affected.ts` custom adoptado.
- [x] Actualizar §Notas técnicas: confirmar GitHub Actions como plataforma y referenciar .NET 10.x (no .NET 8) para el job `core`. → Actualizado en §Notas técnicas.

## Dev Agent Record

- **Fecha:** 2026-10-31
- **Rama:** `feature/story-0-9-pipeline-integracion-continua`
- **Decisiones tomadas al inicio:** Nx descartado; `tools/ci/affected.ts` custom (siguiendo patrón `tools/repo-checks`). Q6 cerrada con 80 % de líneas en código nuevo.

### Archivos creados / modificados

| Archivo | Tipo |
|---|---|
| `tools/ci/affected.ts` | Nuevo (T9: función `computeMatrix` añadida) |
| `tools/ci/affected.spec.ts` | Nuevo (T9: 2 tests de `computeMatrix` añadidos) |
| `tools/ci/compute-matrix.ts` | Nuevo (T9: usa `computeMatrix`; guard `GITHUB_OUTPUT`) |
| `tools/ci/package.json` | Nuevo |
| `tools/ci/vitest.config.ts` | Nuevo (T8: thresholds; T9: exclude `compute-matrix.ts`) |
| `.github/workflows/ci.yml` | Nuevo (T8: H1–H6; T9: node 24, iterar todos los XMLs; T10: fix YAML colon-space en 4 `run:` steps) |
| `README.md` | Modificado (sección CI + .NET 8 → .NET 10) |
| `docs/implementation/sprint_status.yml` | Modificado (Q6 cerrada, HU-00.09 in-progress) |

### Comandos de prueba y resultado

```bash
# tools/ci
pnpm --filter @animetracker/ci test

# tools/repo-checks (regresión)
pnpm --filter @animetracker/repo-checks test

# .NET (regresión)
cd services/core && dotnet test AnimeTracker.sln
```

| Suite | Resultado | Pruebas |
|---|---|---|
| `@animetracker/ci` (affected.spec.ts) | ✅ Pass | 18/18 |
| `@animetracker/repo-checks` (structure.spec.ts) | ✅ Pass | 23/23 |
| .NET (AnimeTracker.sln) | ✅ Pass | 46/46 |
| **Total** | **✅ 87/87** | |

### Cobertura

| Módulo | Cobertura de líneas |
|---|---|
| `tools/ci/affected.ts` | 100 % |
| `@animetracker/ci` (global, excl. compute-matrix.ts) | 89.71 % |

## Code Review

### Iteración 3 — 2026-10-31

- Fecha: 2026-10-31
- Iteración: 3
- Resultado: APROBADA (hallazgo ALTA resuelto en T10-H1)
- Cobertura de líneas (código cambiado): 89.71 % (@animetracker/ci global, excl. compute-matrix.ts); affected.ts 100 %

| Ámbito | Resultado | Hallazgos |
|---|---|---|
| Criterios de aceptación | ✅ | 0 (1 ALTA resuelto) |
| Seguridad | ✅ | 0 |
| Diseño / SOLID | ✅ | 0 |
| Arquitectura | ✅ | 0 (1 ALTA resuelto) |
| Pruebas (> 80 %) | ✅ | 0 |

#### Hallazgos

- [x] [ALTA][DoD] H5 pendiente desde iteraciones 1 y 2 — los ítems "DoD común cumplida" y "Evidencia de los PRs de verificación (CA4, CA5) enlazada en el PR de la historia" permanecen sin marcar. Los mecanismos de enforcement (job `ci-ok`, script Python para cobertura .NET, thresholds Vitest) están correctamente codificados, pero CA4 y CA5 no tienen prueba observacional en el pipeline real. — `docs/implementation/stories/0-9-pipeline-integracion-continua.md:130-131` — Corrección: desplegar el workflow (`git push` a la rama remota), abrir un PR auxiliar con una prueba fallida intencional (CA4) y otro con código sin cobertura suficiente (CA5); confirmar que `ci-ok` queda en "failure" y el merge queda bloqueado; enlazar ambas URLs en §Definición de Hecho y marcar los ítems antes del merge.

#### Sugerencias (bajo el umbral)

- [BAJA][Clean code] Comentario obsoleto "Node 22" en el step `Determine affected jobs` — `.github/workflows/ci.yml:56` — Corrección: cambiar "Node 22" a "Node 24".
- [BAJA][Pruebas] Sin test explícito para mezcla de archivo global + no-global en `affectedJobs` (e.g., `['apps/spa/src/a.ts', 'pnpm-lock.yaml']` → ALL_JOBS) — `tools/ci/affected.spec.ts` — Corrección: añadir caso en `describe('CA3')`.
- [BAJA][Seguridad] `${{ github.event_name }}` interpolado inline en `run:` (patrón conocido desde Iter 2) — `.github/workflows/ci.yml:48` — Corrección: mover a variable de entorno.
- [BAJA][Seguridad] `needs.detect.result` / `needs.run-job.result` interpolados inline en job `ci-ok` — `.github/workflows/ci.yml:217-222` — Corrección: mover a variables de entorno.
- [BAJA][Pruebas] `vitest.config.ts` no excluido del reporte de cobertura; arrastra la métrica global de funciones a 75 % — `tools/ci/vitest.config.ts:1-12` — Corrección: añadir `'vitest.config.ts'` al array `exclude`.
- [BAJA][Diseño] `exclude: ['compute-matrix.ts']` sin patrón de ruta; preferir `'**/compute-matrix.ts'` — `tools/ci/vitest.config.ts:8`.
- [BAJA][Diseño] Inconsistencia de extensión de importación: `compute-matrix.ts:2` usa `'./affected.ts'`, `affected.spec.ts:2` usa `'./affected.js'` — ambos funcionales en sus entornos respectivos.

---

### Iteración 2 — 2026-10-31

- Fecha: 2026-10-31
- Iteración: 2
- Resultado: CAMBIOS REQUERIDOS
- Cobertura de líneas (código cambiado): 81.57 % (@animetracker/ci global); affected.ts 100 %; compute-matrix.ts 0 %

| Ámbito | Resultado | Hallazgos |
|---|---|---|
| Criterios de aceptación | ❌ | 1 (ALTA) + 2 (MEDIA) |
| Seguridad | ✅ | 0 |
| Diseño / SOLID | ❌ | 2 (MEDIA) |
| Arquitectura | ❌ | 1 (MEDIA, DoD) |
| Pruebas (> 80 %) | ❌ | 1 (ALTA) + 1 (MEDIA) |

#### Hallazgos

- [ ] [ALTA][DoD+Pruebas] DoD no completada — los ítems "DoD común cumplida" y "Evidencia de los PRs de verificación (CA4, CA5) enlazada en el PR de la historia" permanecen sin marcar. Los mecanismos de enforcement están codificados (ci-ok, thresholds, script Python), pero no existe prueba observacional de que realmente bloquean el merge. — `docs/implementation/stories/0-9-pipeline-integracion-continua.md:121-125` — Corrección: tras desplegar el workflow, abrir un PR con prueba fallida intencional (CA4) y otro con código sin cobertura suficiente (CA5); enlazar ambas URLs en §Definición de Hecho y marcar los ítems antes del merge.
- [ ] [MEDIA][Diseño] Node.js version mismatch — el workflow usa `node-version: '22'` en jobs `detect` y `run-job` mientras `README.md` especifica Node.js 24.x LTS como versión mínima. La divergencia puede enmascarar diferencias de comportamiento de `--experimental-strip-types` entre versiones. — `.github/workflows/ci.yml:35,85` — Corrección: cambiar `node-version: '22'` → `'24'` en ambos jobs para alinear con el prerequisito declarado.
- [ ] [MEDIA][Diseño] Coverage check evalúa solo el primer XML — `find TestResults -name "coverage.cobertura.xml" | head -1` usa orden no determinístico y solo verifica el primer archivo encontrado. Cuando la solución tenga múltiples proyectos de test, proyectos con cobertura baja quedarán silenciados. — `.github/workflows/ci.yml:157-171` — Corrección: iterar todos los archivos XML con `while IFS= read -r f; do python3 - "$f" <<'PYEOF' ... PYEOF; done < <(find TestResults -name "coverage.cobertura.xml")`.
- [ ] [MEDIA][Criterios] T4 marcado `[x]` con implementación parcial — T4 incluye "Caché de NuGet, pnpm store y build de Angular"; el workflow implementa NuGet y pnpm store pero no la caché de build de Angular (`.angular/cache`). La ausencia es justificable (SPA es stub hasta HU-00.04) pero la tarea está marcada completada sin nota de deferimiento. — `docs/implementation/stories/0-9-pipeline-integracion-continua.md:91` — Corrección: añadir sub-ítem T4b pendiente o nota en §Notas técnicas indicando que el caché Angular se añade en HU-00.04; desmarcar `[x]` del ítem global de T4 o acotarlo explícitamente.
- [ ] [MEDIA][Pruebas] `compute-matrix.ts` sin cobertura (0 %) — la lógica de fallback `jobs.length ? jobs : ['repo-checks']` y la escritura a GITHUB_OUTPUT no están cubiertas. Sin `GITHUB_OUTPUT` definido (ejecución local), el script aborta con TypeError en `appendFileSync`. — `tools/ci/compute-matrix.ts:6-8` — Corrección: extraer la lógica de fallback+serialización a una función pura en `affected.ts` con test unitario, y añadir guard `if (process.env.GITHUB_OUTPUT)` antes de `appendFileSync`; o excluir explícitamente el archivo del umbral con `exclude: ['compute-matrix.ts']` en vitest.config.ts.

#### Sugerencias (bajo el umbral)

- [BAJA][Seguridad] Actions fijadas a major version (`@v4`) en lugar de SHA de commit — riesgo de supply-chain attack. — `.github/workflows/ci.yml` (todas las líneas `uses:`)
- [BAJA][Seguridad] `${{ github.event_name }}` interpolado inline en `run:` en lugar de pasarse por variable de entorno (inconsistente con el patrón del H3 fix). — `.github/workflows/ci.yml:48`
- [BAJA][Diseño] Fallback en `compute-matrix.ts:6` no documentado — cambios en `docs/` o `infra/` producen `jobs = []` y activan el fallback silenciosamente. Añadir comentario explicando la semántica.
- [BAJA][Diseño] `compute-matrix.ts:2` importa `'./affected.ts'` (extensión `.ts`) mientras `affected.spec.ts:2` importa `'./affected.js'` (convención ESM). Inconsistencia menor; ambos funcionan en sus entornos respectivos.
- [BAJA][Criterios] Jobs stub (`bff`, `spa`, `extension`, `contracts`) no documentados explícitamente en §Notas técnicas como pendientes de completarse en HU-00.03–07.

---

### Iteración 1 — 2026-10-31

- Fecha: 2026-10-31
- Iteración: 1
- Resultado: CAMBIOS REQUERIDOS
- Cobertura de líneas (código cambiado): 100 % (`affected.ts`)

| Ámbito | Resultado | Hallazgos |
|---|---|---|
| Criterios de aceptación | ❌ | 2 (ALTA) |
| Seguridad | ❌ | 1 (MEDIA) |
| Diseño / SOLID | ❌ | 2 (ALTA) |
| Arquitectura | ❌ | 3 (ALTA) |
| Pruebas (> 80 %) | ❌ | 2 (ALTA) |

#### Hallazgos

- [ ] [ALTA][Arquitectura] `affected.js` no existe — el job `detect` importa `./tools/ci/affected.js` con `node --input-type=module` pero solo existe `affected.ts`; el pipeline fallará en runtime con "Cannot find module". — `.github/workflows/ci.yml:52` / `tools/ci/package.json` — Corrección: añadir `tsx` a devDependencies de `@animetracker/ci` y cambiar el paso `detect` para usar `npx tsx` (o compilar con `tsc --outDir dist`); ajustar el import a `.ts` o al path compilado.
- [ ] [ALTA][Arquitectura+Diseño] `tools/ci/` → job `repo-checks` pero el step `repo-checks` solo ejecuta `@animetracker/repo-checks`; los 16 tests de `affected.spec.ts` nunca corren en CI y una regresión en `affected.ts` puede mergearse sin detectarse. — `tools/ci/affected.ts:22` / `.github/workflows/ci.yml:99-105` — Corrección: añadir `pnpm --filter @animetracker/ci test:coverage` al step `repo-checks` (o crear job `ci-tools` y actualizar `PATH_MAP` para `tools/ci/`).
- [ ] [ALTA][Arquitectura+Aceptación] `ci-ok` no verifica si `detect` falló: cuando `detect` falla, `run-job` queda "skipped" y `ci-ok` pasa (exit 0), permitiendo el merge con pipeline roto. — `.github/workflows/ci.yml:190` — Corrección: añadir `if [ "${{ needs.detect.result }}" != "success" ]; then echo "detect failed"; exit 1; fi` antes del check de `run-job` en `ci-ok`.
- [ ] [ALTA][Aceptación+Pruebas] CA5 sin enforcement automático: `dotnet test --collect` sube el reporte pero ningún paso falla si la cobertura < 80 %; `vitest.config.ts` no tiene `thresholds`. — `.github/workflows/ci.yml:129-147` / `tools/ci/vitest.config.ts:5-8` — Corrección: para .NET añadir paso con `reportgenerator` o script bash que lea el XML y falle si `line-rate < 0.80`; para Vitest añadir `thresholds: { lines: 80, branches: 80 }` en el bloque `coverage`.
- [ ] [ALTA][Pruebas+DoD] Evidencia de PRs de verificación (CA4, CA5) no enlazada — ítems DoD "DoD común cumplida" y "Evidencia de PRs de verificación" permanecen sin marcar. — `docs/implementation/stories/0-9-pipeline-integracion-continua.md:116-118` — Corrección: tras corregir H1–H4 y crear el PR, ejecutar un PR de verificación con prueba fallida intencional (CA4) y un PR con código sin cobertura (CA5); enlazar ambas URLs en la §Definición de Hecho antes del merge.
- [ ] [MEDIA][Seguridad] `permissions` no declarado — el workflow usa los permisos por defecto del repositorio; declarar `permissions: contents: read` aplica el principio de mínimo privilegio. — `.github/workflows/ci.yml` (top level) — Corrección: añadir `permissions:\n  contents: read` tras el bloque `concurrency`.

#### Sugerencias (bajo el umbral)

- [BAJA][Diseño] import `writeFileSync` sin usar en el script Node inline — `.github/workflows/ci.yml:53`
- [BAJA][Diseño] variable `j` en `for (const { prefix, jobs: j } of PATH_MAP)` demasiado breve; renombrar a `relatedJobs` — `tools/ci/affected.ts:36`
- [BAJA][Seguridad] Actions fijadas a major version (no SHA); usar SHA pin sería más seguro ante supply-chain attacks — `.github/workflows/ci.yml`
- [BAJA][Arquitectura] Jobs stub (`bff`, `spa`, `extension`) siempre retornan éxito; documentar en §Notas técnicas que deben reemplazarse en HU-00.03–05 — `.github/workflows/ci.yml:156-185`
- [BAJA][Arquitectura] `README.md:30` sigue diciendo `.NET 8 Monolito Modular`; actualizar a `.NET 10` — `README.md:30`
