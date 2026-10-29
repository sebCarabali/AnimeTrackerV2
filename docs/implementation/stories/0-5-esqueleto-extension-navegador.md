# HU-00.05 · Esqueleto de la extensión de navegador

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-5-esqueleto-extension-navegador` |
| Tipo | Habilitadora |
| Prioridad | Should |
| Estimación | 3 pts |
| Sprint | sprint-2 |
| Estado | ready-for-dev |
| Depende de | HU-00.01, HU-00.09 |
| Bloquea | EP-06 |

## Historia

**Como** desarrollador, **quiero** un proyecto de extensión Manifest V3 en TypeScript con su build, **para** desarrollar el auto-tracker sobre una base empaquetable.

## Contexto

- PRD §4: script ligero inyectado que detecta el final de un video y envía el progreso vía REST.
- ARD §5: la extensión habla solo con el BFF (`/api/v1/track`), nunca con el núcleo .NET.
- La extensión llega a producción en R3; aquí solo se deja la base.

## Criterios de aceptación

```gherkin
Característica: Extensión MV3 empaquetable

  Escenario: CA1 — Build instalable en Chrome
    Dado el proyecto "apps/extension"
    Cuando ejecuto "pnpm --filter extension build"
    Entonces se genera "apps/extension/dist" con un manifest.json válido con "manifest_version": 3
    Y la carpeta se puede cargar en Chrome en modo desarrollador sin errores

  Escenario: CA2 — Popup con versión y entorno
    Dado la extensión instalada con build de entorno "staging"
    Cuando abro su popup
    Entonces muestra la versión del manifest
    Y muestra el entorno "staging" y la URL del BFF configurada

  Escenario: CA3 — Permisos mínimos
    Dado el manifest generado
    Cuando lo reviso
    Entonces solo declara los permisos "storage" y los "host_permissions" del BFF del entorno
    Y no declara "<all_urls>"

  Escenario: CA4 — Service worker operativo
    Dado la extensión instalada
    Cuando el navegador la carga
    Entonces el service worker se registra y responde a un mensaje "ping" con "pong"
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA3 | Unitaria | `manifest generado es MV3 y no pide <all_urls>` | Vitest |
| 2 | CA2 | Unitaria | `popup renderiza versión y entorno desde config` | Vitest + jsdom |
| 3 | CA4 | Unitaria | `background responde pong a ping` (chrome API simulada) | Vitest + `vitest-chrome` |
| 4 | CA1, CA2 | E2E | `extensión cargada muestra el popup` | Playwright con contexto persistente |

Ejemplo (rojo):

```ts
it('no solicita permisos amplios', () => {
  const manifest = buildManifest({ env: 'staging', bffUrl: 'https://bff.staging.example' });
  expect(manifest.manifest_version).toBe(3);
  expect(manifest.host_permissions).toEqual(['https://bff.staging.example/*']);
  expect(JSON.stringify(manifest)).not.toContain('<all_urls>');
});
```

## Tareas

- [ ] T1 Crear proyecto con Vite (o esbuild) y TypeScript estricto en `apps/extension`.
- [ ] T2 (CA1, CA3) `manifest.ts` que genera el manifest por entorno (`buildManifest`).
- [ ] T3 (CA2) Popup mínimo (HTML + TS) que lee versión y entorno.
- [ ] T4 (CA4) `background.ts` (service worker) con manejador de mensajes.
- [ ] T5 (CA1) Prueba E2E con Playwright cargando `dist/` en Chromium; job de CI.
- [ ] T6 Script `zip` para empaquetar el build (preparado para la tienda en R3).

## Notas técnicas y decisiones

- Los sitios de streaming soportados y sus `content_scripts` se definen en EP-06 (pregunta abierta del backlog).
- La URL del BFF se inyecta en tiempo de build; no hay secretos en la extensión.

## Fuera de alcance

- Detección de video, vinculación de cuenta y cola sin conexión (EP-06).

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Artefacto `dist` o `.zip` publicado como artefacto del pipeline.
