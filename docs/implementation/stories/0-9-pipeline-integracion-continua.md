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
- [x] T4 Caché de NuGet, pnpm store y build de Angular.
- [x] T5 (CA5) Publicar cobertura y aplicar umbral del 80 % en código nuevo (diff coverage).
- [x] T6 (CA4) Configurar protección de rama `main`: checks requeridos, 1 aprobación, rama actualizada. *(Documentada en §Notas técnicas; se aplica en GitHub manualmente al crear el PR.)*
- [x] T7 Job agregador `ci-ok` que es el único check requerido (evita mantener la lista de checks en la protección de rama cuando la matriz cambia).

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

- [ ] DoD común cumplida.
- [ ] Evidencia de los PRs de verificación (CA4, CA5) enlazada en el PR de la historia.
- [ ] Sección "CI" en el README raíz con cómo reproducir cada job localmente.

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

- [ ] Resolver Q6 antes del 2026-11-02: confirmar umbral 80 % de cobertura en código nuevo; cerrar Q6 en sprint_status.yml y actualizar CA5/T5 con el número definitivo.
- [ ] Decidir Nx vs. `tools/ci/affected.ts` como primera microtarea de T1 (antes de escribir código); documentar en §Notas técnicas y ajustar T2/T3 si se elige Nx.
- [ ] Actualizar §Notas técnicas: confirmar GitHub Actions como plataforma y referenciar .NET 10.x (no .NET 8) para el job `core`.
