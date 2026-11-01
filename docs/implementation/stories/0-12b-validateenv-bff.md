# HU-00.12b · validateEnv + .env.example (BFF)

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-12b-validateenv-bff` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 1.5 pts |
| Sprint | sprint-1 |
| Estado | ready-for-dev |
| Depende de | HU-00.01, HU-00.09 |
| Bloquea | HU-00.03 |

## Historia

**Como** desarrollador BFF, **quiero** que mi aplicación valide configuración obligatoria y falle rápido si faltan variables de entorno, **para** evitar errores de arranque en producción.

## Contexto

- Esta historia es MVP de la validación de configuración del BFF (T4a + T4b).
- T4a (validateEnv scaffolding) es **hard-stop no-negotiable** para HU-00.03 (sprint-2), que reutiliza `validateEnv()` sin reescritura.
- T5 (sincronización .env.example completa) se diferida a sprint-2 (extensión, no bloquea HU-00.03).
- ADR-003 especifica Jest para pruebas de contrato BFF (`jest-openapi`).

## Criterios de aceptación

```gherkin
Característica: Validación de configuración BFF (Jest MVP)

  Escenario: CA4 — Validación de configuración del BFF (scaffolding Jest)
    Dado el scaffolding mínimo de Jest en `apps/bff/src`
    Cuando llamo la función `validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV'])` sin una variable
    Entonces lanza error que nombra la variable faltante
    Y el mensaje es legible en logs del BFF

  Escenario: CA5a — Plantilla .env.example para BFF
    Dado el repositorio
    Cuando reviso ".env.example"
    Entonces lista las variables del BFF (CORE_BASE_URL, PORT, NODE_ENV) con descripción y valor de ejemplo no sensible
    Y ".env" está en ".gitignore"
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 3 | CA4 | Unitaria | `validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV']) lanza error con nombre de variable` | Jest en `apps/bff/src/__tests__/config.spec.ts` |
| 4 | CA5a | Estructura | `.env.example` contiene CORE_BASE_URL, PORT, NODE_ENV documentadas | Lectura + validación manual |

## Tareas (Sprint-1: T4a MVP + T4b)

- [ ] T4a (CA4) **MVP Scaffolding Jest en `apps/bff`:** 
  - Ejecutar `pnpm add --save-dev jest @types/jest ts-jest typescript` (crea `apps/bff/package.json`)
  - Crear `apps/bff/jest.config.js` mínimo con ts-jest preset
  - Crear `apps/bff/src/common/config.ts` con función `validateEnv(vars: string[]): void` que:
    - Itera vars y verifica que existan en `process.env`
    - Si falta una, lanza `Error` con mensaje: `"Missing required environment variable: <VAR_NAME>"`
  - Crear `apps/bff/src/__tests__/config.spec.ts` con test en Jest que valida validateEnv lanza error si faltan variables
  - No incluye: ConfigModule de NestJS (eso es HU-00.03)
  - **Restricción:** HU-00.03 (sprint-2) reutiliza `validateEnv()` sin reescribir; package.json base permanece

- [ ] T4b (CA5a) **Documentar variables BFF en `.env.example`:**
  - Crear/actualizar `.env.example` en raíz con sección BFF:
    ```
    # BFF (apps/bff)
    CORE_BASE_URL=http://localhost:5000
    PORT=3000
    NODE_ENV=development
    ```
  - Cada variable con descripción y valor de ejemplo no sensible
  - `.env` ya está en `.gitignore` (verificar)

## Notas técnicas y decisiones

- **T4a es MVP no-negotiable en sprint-1** (confirma Sprint Planning 2026-11-02): Jest scaffolding + validateEnv() función pura, agnóstica a framework.
- **Diseño (Opción A de Dev Notes HU-00.12):** validateEnv es utilidad de infraestructura (`src/common/config.ts`), reutilizable sin modificación en NestJS ConfigModule (HU-00.03).
- **Jest para BFF (consciente):** ADR-003 especifica `jest-openapi` para pruebas de contrato BFF; Jest es el estándar. Vitest continúa para `tools/*` y `apps/spa`.

## Diferimientos a Sprint-2

- [ ] **T5 (CA5 completa):** Crear `.env.example` en raíz con **todas** las variables (servicios .NET + BFF) + prueba `tools/repo-checks/sync-env-example.spec.ts` (Vitest) que valida sincronización. Diferida porque:
  - HU-00.03 no depende de T5 (reutiliza T4a validateEnv, que sí está en sprint-1)
  - HU-00.06 (.env.example) puede hacerse en sprint-2 sin bloqueo
  - Reducción de riesgo en sprint-1: T4a MVP (1.5–2h) vs. T4a + T5 (4.5–5h)

## Fuera de alcance

- Integración con NestJS ConfigModule (HU-00.03)
- Sincronización automática de variables en CI
- Gestión de secretos por entorno (Vault, GitHub Environments) — ver HU-00.12a (T6)

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Jest scaffolding en `apps/bff` con `jest.config.js` y `package.json` (base para HU-00.03).
- [ ] Función `validateEnv()` en `src/common/config.ts` reutilizable sin reescritura.
- [ ] Test suite en Jest (`src/__tests__/config.spec.ts`) con casos rojo → verde.
- [ ] `.env.example` actualizado con variables BFF documentadas.

## Readiness Review

- Fecha: 2026-11-02 (Sprint Planning — descomposición HU-00.12)
- Resultado: ready-for-dev
- Notas: Descompuesta de HU-00.12 original. MVP claro (T4a + T4b); T5 diferida a sprint-2. T4a confirmada como MVP no-negotiable por Architect + PM + Dev. Desbloquea HU-00.03 sin alternativa de reescritura.
