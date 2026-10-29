# HU-00.04 · SPA Angular inicial

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-4-spa-angular-inicial` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 3 pts |
| Sprint | sprint-2 |
| Estado | ready-for-dev |
| Depende de | HU-00.01, HU-00.07, HU-00.09 |
| Bloquea | EP-01 (UI), EP-02 (UI), EP-03 (UI), EP-04, EP-05 (UI) |

## Historia

**Como** desarrollador frontend, **quiero** una aplicación Angular con enrutamiento, layout base y cliente API generado, **para** construir las pantallas sobre una base común.

## Contexto

- ARD §2: Angular con servicios reactivos (RxJS) y TypeScript estricto.
- ARD §6: la SPA aplica el desfase horario con pipes; el layout debe considerar desde ya la configuración de locale.
- El cliente de la API pública se genera desde `contracts/public`.

## Criterios de aceptación

```gherkin
Característica: SPA Angular base

  Escenario: CA1 — Layout con navegación principal
    Dado la SPA en ejecución con "ng serve"
    Cuando abro la raíz "/"
    Entonces se redirige a "/dashboard"
    Y veo un layout con enlaces a Biblioteca, Buscar, Calendario y Dashboard

  Escenario: CA2 — Rutas con páginas vacías y carga diferida
    Dado el layout principal
    Cuando navego a "/biblioteca", "/buscar", "/calendario" o "/dashboard"
    Entonces se muestra la página correspondiente con su título
    Y cada página se carga de forma diferida (lazy)

  Escenario: CA3 — Ruta inexistente
    Dado la SPA
    Cuando navego a "/no-existe"
    Entonces veo una página "No encontrado" con un enlace al dashboard

  Escenario: CA4 — Servicios tipados generados desde el contrato público
    Dado el contrato "contracts/public/bff.v1.yaml"
    Cuando ejecuto "pnpm --filter spa gen:api"
    Entonces se generan servicios Angular tipados en "apps/spa/src/app/core/api/generated"
    Y la URL base se inyecta por configuración de entorno, no está fija en el código

  Escenario: CA5 — Calidad en CI
    Dado un PR que modifica "apps/spa"
    Cuando corre CI
    Entonces se ejecutan ESLint y las pruebas unitarias de la SPA
    Y si alguna falla el merge queda bloqueado
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA1 | Componente | `ShellComponent muestra los 4 enlaces de navegación` | Vitest/Jest + TestBed |
| 2 | CA1 | Router | `"/" redirige a "/dashboard"` | `RouterTestingHarness` |
| 3 | CA2 | Router | `cada ruta renderiza su página con título` (prueba parametrizada) | `RouterTestingHarness` |
| 4 | CA3 | Router | `ruta desconocida muestra NotFoundPage` | `RouterTestingHarness` |
| 5 | CA4 | Unitaria | `API_BASE_URL se provee desde environment` | TestBed |

Ejemplo (rojo):

```ts
it.each([
  ['/biblioteca', 'Biblioteca'],
  ['/buscar', 'Buscar'],
  ['/calendario', 'Calendario'],
  ['/dashboard', 'Dashboard'],
])('navega a %s y muestra "%s"', async (url, title) => {
  const harness = await RouterTestingHarness.create();
  await harness.navigateByUrl(url);
  expect(harness.routeNativeElement?.querySelector('h1')?.textContent).toContain(title);
});
```

## Tareas

- [ ] T1 Generar app Angular en `apps/spa` (standalone components, `strict: true`, SCSS).
- [ ] T2 (CA1, CA2, CA3) `app.routes.ts` con `loadComponent` por página, redirección y comodín `**`.
- [ ] T3 (CA1) `ShellComponent` con barra de navegación accesible (`nav`, `aria-current`).
- [ ] T4 (CA4) Script `gen:api` (ng-openapi-gen u openapi-generator `typescript-angular`) + token `API_BASE_URL`.
- [ ] T5 (CA5) Configurar angular-eslint y el runner de pruebas; registrar el job en el pipeline de HU-00.09.
- [ ] T6 Registrar `provideHttpClient(withInterceptorsFromDi())` como base para el interceptor de correlación (HU-00.11).

## Notas técnicas y decisiones

- Runner de pruebas: el que traiga por defecto la versión de Angular instalada (Vitest en las versiones recientes); si se prefiere Jest, documentarlo en el README de la app.
- Las páginas son placeholders; el diseño visual llega con cada épica.

## Fuera de alcance

- Autenticación y guards (EP-01).
- Diseño visual y sistema de componentes.

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] `ng build --configuration production` sin errores.
- [ ] Lighthouse de accesibilidad ≥ 90 en el layout vacío.
