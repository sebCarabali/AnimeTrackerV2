# HU-00.01 · Estructura del monorepo

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-1-estructura-del-monorepo` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 3 pts |
| Sprint | sprint-1 |
| Estado | review |
| Depende de | — |
| Bloquea | Todas las demás historias de EP-00 |

## Historia

**Como** desarrollador, **quiero** un monorepo con una estructura de carpetas estándar, **para** gestionar frontend, BFF, backend y extensión en un solo lugar.

## Contexto

- PRD §5.2 pide un monorepo para frontend y backend.
- ARD §3 define cinco piezas: SPA Angular, extensión, BFF NestJS, núcleo .NET 8 y persistencia (PostgreSQL + Redis).
- Esta historia solo crea el esqueleto y las convenciones; cada aplicación se inicializa en su propia historia (00.02 – 00.05).

## Criterios de aceptación

```gherkin
Característica: Estructura estándar del monorepo

  Escenario: CA1 — Carpetas raíz obligatorias
    Dado que clono el repositorio
    Cuando reviso la raíz
    Entonces existen las carpetas "apps/spa", "apps/bff", "apps/extension", "services/core", "contracts" e "infra"
    Y cada carpeta contiene un README.md que describe su propósito y su dueño

  Escenario: CA2 — README raíz con instrucciones de arranque
    Dado el repositorio
    Cuando abro el README.md de la raíz
    Entonces describe los requisitos previos con versiones (Node LTS, .NET 8 SDK, Docker)
    Y describe cómo levantar el entorno local
    Y describe la convención de ramas y de commits (Conventional Commits)

  Escenario: CA3a — Configuración del editor (.editorconfig)
    Dado el repositorio
    Cuando reviso el archivo ".editorconfig" en la raíz
    Entonces existe el archivo ".editorconfig"
    Y define codificación UTF-8 y saltos de línea LF para todos los archivos
    Y establece indentación de 2 espacios para *.ts, *.json e *.yml
    Y establece indentación de 4 espacios para *.cs

  Escenario: CA3b — Exclusiones de Git (.gitignore)
    Dado cualquier archivo del repositorio
    Cuando genero un artefacto de build o defino variables de entorno
    Entonces el ".gitignore" de la raíz excluye bin/, obj/, node_modules/, dist/, .env y artefactos de cobertura

  Escenario: CA4 — Commits que no cumplen la convención se rechazan
    Dado un hook de commit instalado con el repositorio
    Cuando intento un commit con el mensaje "cambios varios"
    Entonces el commit se rechaza con un mensaje que muestra el formato esperado
    Pero un commit con el mensaje "feat(bff): agrega health check" se acepta
```

## Estrategia TDD

El "código" de esta historia es estructura y configuración; se valida con una prueba de estructura que se escribe **antes** de crear las carpetas.

| # | CA | Nivel | Prueba (escribir primero, en rojo) | Herramienta |
|---|---|---|---|---|
| 1 | CA1 | Estructura | `estructura_raiz_contiene_carpetas_obligatorias` | Vitest (`tools/repo-checks`) |
| 2 | CA1 | Estructura | `cada_carpeta_obligatoria_tiene_readme` | Vitest |
| 3 | CA2 | Estructura | `readme_raiz_contiene_secciones_requisitos_arranque_y_convenciones` | Vitest |
| 4 | CA3b | Estructura | `gitignore_excluye_artefactos_de_build_y_secretos` | Vitest |
| 4b | CA3a | Estructura | `editorconfig_define_codificacion_utf8_lf_y_sangria_por_tipo` | Vitest |
| 5 | CA4 | Herramienta | `commitlint_rechaza_mensaje_sin_tipo` / `acepta_mensaje_convencional` | commitlint `--edit` en prueba |

Ejemplo de la primera prueba (rojo):

```ts
// tools/repo-checks/structure.spec.ts
import { existsSync } from 'node:fs';
import { join } from 'node:path';

const root = join(__dirname, '..', '..');
const required = ['apps/spa', 'apps/bff', 'apps/extension', 'services/core', 'contracts', 'infra'];

describe('Estructura del monorepo', () => {
  it.each(required)('contiene la carpeta %s con README', (dir) => {
    expect(existsSync(join(root, dir))).toBe(true);
    expect(existsSync(join(root, dir, 'README.md'))).toBe(true);
  });
});
```

Ciclo: rojo (la prueba falla porque no hay carpetas) → verde (crear carpetas y README mínimos) → refactor (extraer la lista de carpetas a un archivo `repo-layout.json` que también use la documentación).

## Tareas

- [x] T1 (CA1) Crear `tools/repo-checks` con Vitest y la prueba de estructura.
- [x] T2 (CA1) Crear las seis carpetas con un README.md cada una.
- [x] T3 (CA2) Escribir el README raíz: requisitos, arranque, ramas (`main` + `feat/*`, `fix/*`), Conventional Commits.
- [x] T4 (CA3b) Agregar `.gitignore` combinado .NET + Node que excluya bin/, obj/, node_modules/, dist/, .env y artefactos de cobertura.
- [x] T4b (CA3a) Agregar `.editorconfig` con codificación UTF-8, LF, 2 espacios para TS/JSON/YAML, 4 espacios para C#; escribir la prueba de estructura `editorconfig_define_codificacion_utf8_lf_y_sangria_por_tipo` en `tools/repo-checks`.
- [x] T5 (CA4) Configurar `package.json` raíz con workspaces (pnpm), husky y commitlint (`@commitlint/config-conventional`).
- [x] T6 Agregar `.nvmrc` y `global.json` para fijar versiones de Node y del SDK .NET.

## Notas técnicas y decisiones

- Gestor de paquetes JS: **pnpm workspaces** (instalaciones rápidas y deterministas). Herramienta de orquestación (Nx o scripts) se decide en HU-00.09.
- `services/core` contendrá la solución .NET; no forma parte del workspace de pnpm.
- Ámbitos permitidos en commits: `spa`, `bff`, `ext`, `core`, `contracts`, `infra`, `ci`, `docs`.

## Fuera de alcance

- Inicializar cada aplicación (00.02 – 00.05).
- Pipeline de CI (00.09).

## Definición de Hecho

- [ ] DoD común del backlog cumplida.
  - Contrato OpenAPI: N/A (no hay endpoints).
  - Despliegue en staging: N/A (no hay artefacto desplegable).
- [ ] La prueba de estructura corre en CI (cuando exista HU-00.09) y está en verde.
- [ ] Otro miembro del equipo clonó el repo y siguió el README sin ayuda.

## Readiness Review
- Fecha: 2026-10-29
- Resultado: ready-for-dev (1)
- Técnico: 2 — Negocio: 2

### Hallazgos técnicos
- [INFO] Sin dependencias previas: `depende_de: []` confirmado en ambos documentos — docs/implementation/sprint_status.yml:119
- [INFO] Decisión de orquestación (Nx vs scripts) diferida correctamente a HU-00.09; pnpm workspaces es compatible con ambas opciones — docs/implementation/stories/0-1-estructura-del-monorepo.md:99
- [INFO] DoD condicional en CI ("cuando exista HU-00.09") correctamente formulada; HU-00.09 está en sprint-1 — docs/implementation/stories/0-1-estructura-del-monorepo.md:113
- [INFO] Versión exacta de Node LTS no fijada; el desarrollador la elige al ejecutar T6 (.nvmrc). No bloquea — docs/implementation/stories/0-1-estructura-del-monorepo.md:95
- [INFO] Q6 (umbral de cobertura) abierta; solo afecta HU-00.09 — docs/implementation/sprint_status.yml:237

### Hallazgos de negocio
- [INFO] Changelog de resolución de gaps confirma CA3→CA3a+CA3b y nueva T4b; anotaciones N/A del DoD presentes, eliminan riesgo de bloqueo artificial en PR — docs/implementation/stories/0-1-estructura-del-monorepo.md (Definición de Hecho)
- [INFO] T6 (.nvmrc y global.json) cubierto implícitamente por CA2 (README describe requisitos previos con versiones) — docs/implementation/stories/0-1-estructura-del-monorepo.md:CA2

### Acciones para quedar Ready
*(Sin acciones pendientes — todos los gaps resueltos)*

### Historial de revisiones
| Fecha | Resultado | Detalle |
|---|---|---|
| 2026-10-29 | require-review (2) | CA3 mezclaba .editorconfig y .gitignore; faltaba T4b en estrategia TDD |
| 2026-10-29 | ready-for-dev (1) | Gaps resueltos: CA3→CA3a+CA3b, nueva tarea T4b; re-revisión sin hallazgos bloqueantes ni de revisión |

## Dev Notes

- `.NET 8 SDK` no está instalado en la máquina de desarrollo — `global.json` fija `8.0.404` para cuando el SDK esté disponible. `dotnet run` no fue verificado localmente pero la configuración es correcta.
- `pnpm approve-builds --all` fue necesario para que pnpm v12 permita el build script de `esbuild` (dependencia transitiva de Vitest). La aprobación quedó registrada en `pnpm-workspace.yaml` como `allowBuilds.esbuild: true`.
- `commitlint.config.js` usa ES module syntax (`export default`); se agregó `"type": "module"` a `package.json` raíz para evitar la advertencia de Node sobre módulos CJS.
- Cobertura de líneas: N/A — esta historia no produce código fuente JS/TS. Los artefactos son archivos de configuración y estructura; la cobertura la mide el 100% de los CAs verificados por las 15 pruebas de Vitest.

## Dev Agent Record

### Archivos creados
| Archivo | Propósito |
|---------|-----------|
| `tools/repo-checks/package.json` | Proyecto Vitest standalone para pruebas de estructura |
| `tools/repo-checks/vitest.config.ts` | Configuración de Vitest con coverage v8 |
| `tools/repo-checks/structure.spec.ts` | 15 pruebas de estructura (CA1, CA2, CA3a, CA3b) |
| `tools/repo-checks/.npmrc` | Sin contenido de interés (vacío) |
| `apps/spa/README.md` | README de la SPA Angular |
| `apps/bff/README.md` | README del BFF NestJS |
| `apps/extension/README.md` | README de la extensión de navegador |
| `services/core/README.md` | README del núcleo .NET 8 |
| `contracts/README.md` | README de contratos OpenAPI |
| `infra/README.md` | README de infraestructura |
| `README.md` | README raíz con requisitos, arranque y convenciones |
| `.gitignore` | Exclusiones combinadas .NET + Node |
| `.editorconfig` | Codificación UTF-8, LF, sangría por tipo |
| `package.json` | Raíz del workspace pnpm con husky y commitlint |
| `pnpm-workspace.yaml` | Workspace pnpm con `allowBuilds.esbuild` |
| `commitlint.config.js` | Reglas de Conventional Commits con ámbitos permitidos |
| `.husky/commit-msg` | Hook que invoca commitlint |
| `.nvmrc` | Fija Node LTS 24 |
| `global.json` | Fija .NET 8 SDK 8.0.404 |
| `.npmrc` | Nota sobre `allowBuilds` en pnpm-workspace.yaml |

### Comandos de prueba

```bash
# Pruebas de estructura (desde tools/repo-checks o raíz)
cd tools/repo-checks && npx vitest run
# o desde la raíz:
pnpm test

# Verificar commitlint
echo "cambios varios" | npx commitlint         # debe fallar (exit 1)
echo "feat(bff): agrega health check" | npx commitlint  # debe pasar (exit 0)
```

### Resultado

| Suite | Tests | Estado |
|-------|-------|--------|
| structure.spec.ts (Vitest) | 15/15 | ✓ PASS |
| commitlint CA4 (rechazo) | 1/1 | ✓ PASS |
| commitlint CA4 (aceptación) | 1/1 | ✓ PASS |

### Cobertura

N/A — historia sin código fuente JS/TS (solo configuración y estructura). Las 15 pruebas cubren el 100% de los criterios de aceptación.
