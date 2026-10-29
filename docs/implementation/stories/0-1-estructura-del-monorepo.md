# HU-00.01 · Estructura del monorepo

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-1-estructura-del-monorepo` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 3 pts |
| Sprint | sprint-1 |
| Estado | ready-for-dev |
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

  Escenario: CA3 — Configuración común del editor y de Git
    Dado cualquier archivo del repositorio
    Cuando lo edito
    Entonces se aplican el ".editorconfig" y el ".gitignore" de la raíz
    Y el ".gitignore" excluye bin/, obj/, node_modules/, dist/, .env y artefactos de cobertura

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
| 4 | CA3 | Estructura | `gitignore_excluye_artefactos_de_build_y_secretos` | Vitest |
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

- [ ] T1 (CA1) Crear `tools/repo-checks` con Vitest y la prueba de estructura.
- [ ] T2 (CA1) Crear las seis carpetas con un README.md cada una.
- [ ] T3 (CA2) Escribir el README raíz: requisitos, arranque, ramas (`main` + `feat/*`, `fix/*`), Conventional Commits.
- [ ] T4 (CA3) Agregar `.editorconfig` (UTF-8, LF, 2 espacios TS/JSON/YAML, 4 espacios C#) y `.gitignore` combinado .NET + Node.
- [ ] T5 (CA4) Configurar `package.json` raíz con workspaces (pnpm), husky y commitlint (`@commitlint/config-conventional`).
- [ ] T6 Agregar `.nvmrc` y `global.json` para fijar versiones de Node y del SDK .NET.

## Notas técnicas y decisiones

- Gestor de paquetes JS: **pnpm workspaces** (instalaciones rápidas y deterministas). Herramienta de orquestación (Nx o scripts) se decide en HU-00.09.
- `services/core` contendrá la solución .NET; no forma parte del workspace de pnpm.
- Ámbitos permitidos en commits: `spa`, `bff`, `ext`, `core`, `contracts`, `infra`, `ci`, `docs`.

## Fuera de alcance

- Inicializar cada aplicación (00.02 – 00.05).
- Pipeline de CI (00.09).

## Definición de Hecho

- [ ] DoD común del backlog cumplida.
- [ ] La prueba de estructura corre en CI (cuando exista HU-00.09) y está en verde.
- [ ] Otro miembro del equipo clonó el repo y siguió el README sin ayuda.
