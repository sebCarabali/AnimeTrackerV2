# HU-00.12 · Configuración y secretos

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-12-configuracion-y-secretos` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 2 pts |
| Sprint | sprint-1 |
| Estado | ready-for-dev |
| Depende de | HU-00.01, HU-00.09 |
| Bloquea | HU-00.03, HU-00.06, HU-00.10 |

## Historia

**Como** equipo, **quiero** que la configuración venga de variables de entorno y los secretos nunca estén en el repositorio, **para** desplegar de forma segura en cada entorno.

## Contexto

- Twelve-Factor: configuración en el entorno.
- La DoD de EP-00 exige que los servicios fallen rápido ante configuración incompleta.

## Criterios de aceptación

```gherkin
Característica: Configuración por entorno y secretos protegidos

  Escenario: CA1 — Escáner de secretos en CI
    Dado un PR que agrega una cadena con forma de credencial (p. ej. una clave privada o un token de API)
    Cuando CI analiza el código
    Entonces el job de secretos falla e indica archivo y línea
    Y el merge queda bloqueado

  Escenario: CA2 — Escáner local antes del commit
    Dado el hook de pre-commit instalado
    Cuando intento commitear un archivo con una credencial
    Entonces el commit se rechaza

  Escenario: CA3 — Núcleo .NET falla rápido
    Dado el núcleo .NET
    Cuando arranca sin la variable obligatoria "ConnectionStrings__Postgres"
    Entonces termina con código distinto de 0
    Y el error nombra la variable faltante

  Escenario: CA4 — Validación de configuración del BFF (scaffolding Jest)
    Dado el scaffolding mínimo de Jest en `apps/bff/src`
    Cuando llamo la función `validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV'])` sin una variable
    Entonces lanza error que nombra la variable faltante
    Y el mensaje es legible en logs del BFF

  Escenario: CA5 — Plantilla documentada
    Dado el repositorio
    Cuando reviso ".env.example"
    Entonces lista todas las variables de cada servicio con descripción y valor de ejemplo no sensible
    Y ".env" está en ".gitignore"
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA3 | Integración | `Host_sin_connection_string_falla_nombrando_la_variable` | xUnit + `WebApplicationFactory` con configuración vacía |
| 2 | CA3 | Unitaria | `Opciones_de_cada_modulo_tienen_validacion_al_arrancar` | xUnit (`ValidateOnStart`) |
| 3 | CA4 | Unitaria | `validateEnv(['CORE_BASE_URL', 'PORT', 'NODE_ENV']) lanza error con nombre de variable` | Jest en `apps/bff/src/__tests__/config.spec.ts` |
| 4 | CA5 | Estructura | `sync-env-example.spec.ts: (a) toda variable de services/core/*/Application/ConfigOptions/*.cs está en .env.example, (b) toda variable en .env.example está documentada` | Vitest en `tools/repo-checks/sync-env-example.spec.ts` |
| 5 | CA1 | Aceptación | PR de verificación con un token falso → job rojo | gitleaks |

Ejemplo (rojo):

```csharp
[Fact]
public void Host_sin_connection_string_falla_nombrando_la_variable()
{
    var factory = new CoreFactory().WithConfiguration(new Dictionary<string, string?>
    {
        ["ConnectionStrings:Postgres"] = null,
    });

    var ex = Should.Throw<OptionsValidationException>(() => factory.CreateClient());

    ex.Message.ShouldContain("ConnectionStrings__Postgres");
}
```

## Tareas

- [ ] T1 (CA1) Job `secrets` en CI con gitleaks (configuración `.gitleaks.toml` con allowlist para fixtures). Usar ruleslist estándar de gitleaks (detecta patrones de credenciales: claves privadas RSA, tokens JWT, API keys, etc.) + allowlist personalizado para test data y fixtures (p. ej. `allowed_secrets.txt`).
- [ ] T2 (CA2) Hook de pre-commit con gitleaks vía husky.
- [ ] T3 (CA3) Clases de opciones tipadas distribuidas en cada módulo: `<Módulo>.Application/Options/<ModuloNameOptions>.cs` con `ValidateDataAnnotations().ValidateOnStart()` y mensaje con el nombre de la variable de entorno. Ejemplo: `Catalog.Application/Options/CatalogOptions.cs`, `Tracking.Application/Options/TrackingOptions.cs`. Cada clase enlazada a sus variables de configuración con nombres consistentes (ej. `ConnectionStrings:Postgres` → `CatalogOptions.ConnectionString`).
- [ ] T4a (CA4) Scaffolding Jest mínimo en `apps/bff`: ejecutar `pnpm add --save-dev jest @types/jest ts-jest typescript` (crea `package.json`); crear `jest.config.js` mínimo; crear `src/__tests__/config.spec.ts` y función `src/common/config.ts:validateEnv(vars: string[])` que lanza error si una variable falta, con mensaje que incluye el nombre de la variable. HU-00.03 reutilizará `validateEnv()` sin reescribir; package.json base permanece. [Decisión sprint planning 2026-11-01: T4a crea package.json en sprint-1]
- [ ] T4b (CA4) Documentar variables obligatorias del BFF en `.env.example`: `CORE_BASE_URL`, `PORT`, `NODE_ENV` (reutilizable en HU-00.03 ConfigModule sin reescritura).
- [ ] T5 (CA5) Crear `.env.example` en raíz con todas las variables de `services/core` (ConnectionStrings__Postgres, etc.) y `apps/bff` (CORE_BASE_URL, PORT, NODE_ENV). Implementar prueba `tools/repo-checks/sync-env-example.spec.ts` que valida: (a) toda variable de archivos `*Options.cs` en `services/core/*/Application/Options/` tiene entrada en `.env.example`, (b) toda variable en `.env.example` está documentada con descripción. La prueba **falla el build** si hay inconsistencia.
- [ ] T6 Documentar dónde viven los secretos por entorno: GitHub Environments para CI/CD + HashiCorp Vault (Docker) para staging (acceso interno; credenciales de Vault en github.com/settings/secrets para CD). Producción fuera de scope.

## Notas técnicas y decisiones

- En local se usa `.env` (no versionado) y, para .NET, opcionalmente `dotnet user-secrets`.
- La prueba de sincronía de `.env.example` evita que una variable nueva se olvide en la documentación.

## Dev Notes

> **Decisión de diseño (Opción A):** T4a (scaffolding Jest + `validateEnv`) crea la función base que HU-00.03 integrará en NestJS ConfigModule. Esto rompe el acoplamiento temporal (HU-00.12 en sprint-1 ANTES de HU-00.03 en sprint-2) y asegura que la lógica de validación no se duplica. HU-00.03 reutilizará `apps/bff/src/common/config.ts:validateEnv()` directamente sin reescribirla.

> **Decisión Jest para BFF (Consciente):** T4a crea `apps/bff/package.json` con Jest + TypeScript + ts-jest en sprint-1. Jest es la herramienta estándar para el BFF porque ADR-003 especifica `jest-openapi` para pruebas de contrato BFF (línea 34). Vitest continúa siendo el estándar para `tools/*` y `apps/spa`. Registrado en ADR-003 como decisión consciente.

## Fuera de alcance

- Rotación de secretos y gestor de secretos de producción.

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Job `secrets` como check requerido (vía `ci-ok`).
- [ ] Historial de git revisado con gitleaks sin hallazgos.

## Readiness Review
- Fecha: 2026-11-01 (iteración 2 — story-gaps + story-readiness)
- Resultado: blocked (3)
- Técnico (Dev): 3 — Negocio (PM): 2

### Hallazgos técnicos
- [BLOQUEANTE] Estimación 2 pts sin confirmar — Desglose real: ~10–11h (T1: 1h, T2: 0.5h, T3: 1.5–2h, T4a: 1.5–2h, T4b: 0.5h, T5: 2–2.5h, T6: 0.5h). Sprint-1 comprometido 23/24 pts. Si 2 pts es subestimado, reventará timebox y desviará sprint. **Acción:** Sprint Planning 2026-11-02 confirmar si 2 pts es suficiente o descomponer en 2 historias (HU-00.12a gitleaks+hook ~1.5 pts, HU-00.12b config+docs ~2.5 pts) — `0-12-configuracion-y-secretos.md:9, línea 128`
- [BLOQUEANTE] T4a timing sin confirmar como no-negotiable — T4a crea validateEnv() en sprint-1; HU-00.03 (sprint-2, sprint_status.yml:123) depende de él sin alternativa de reescritura. Debe confirmarse que no será diferido a HU-00.03. **Acción:** Sprint Planning 2026-11-02 confirmar T4a como MVP de sprint-1 (no negociable) — `0-12-configuracion-y-secretos.md:91, §Dev Notes línea 103`
- [REVISAR] T1 gitleaks allowlist sin template concreto — Se especifica "ruleslist estándar gitleaks + allowlist para test data" pero no se proporciona `.gitleaks.toml` template. Sin template, implementador puede hacer allowlist permisivo (falsos negativos en fixtures) o muy restrictivo (falsos positivos). **Acción:** Proporcionar antes de sprint `.gitleaks.toml` template con ruleslist estándar + ejemplos allowlist (p.ej. fixtures/, TestResults/, test data) — `0-12-configuracion-y-secretos.md:88, línea 146`
- [REVISAR] T5 lógica de detección sin detalles de parsing — Especifica "patrón `*Options.cs` en `services/core/*/Application/Options/`" pero no detalla cómo parsea Vitest nombres exactos de variables: ¿busca [BinderName]?, ¿convención PascalCase→snake_case?, ¿cómo mapea `CatalogOptions.ConnectionString` → `ConnectionStrings__Postgres`?. **Acción:** Documentar antes de sprint parsing logic en T5 (p.ej., ejemplos mapping: ConnectionString→ConnectionStrings__Postgres, Port→Port) — `0-12-configuracion-y-secretos.md:93, línea 144`
- [REVISAR] T3 prueba de ubicación de ConfigOptions no validada en Matriz TDD — T2 valida que ConfigOptions tengan `ValidateOnStart`, pero NO valida que estén ubicadas en `<Módulo>.Application/Options/` (no centralizadas en Host/Options o Infrastructure). Riesgo: alguien podría crear `Host/Options/AppOptions.cs` y prueba pasaría, violando patrón distribuido. **Acción:** Agregar en Matriz TDD prueba de arquitectura (NetArchTest xUnit reflection) que valide "No existe *Options en Host/Options/ ni */Infrastructure/Options/, solo en */Application/Options/" — `0-12-configuracion-y-secretos.md:61–67`
- [INFO] Gaps resueltos en iteración 2 (story-gaps 2026-11-01): 8 de 8 especificados — (1) T3 patrón → distribuido, (2) T6 gestor → Vault, (3) T5 detección → *Options.cs, (4) CA4/T4b variables → exactas, (5) T1 allowlist → ruleslist, (6) T5 comportamiento → falla build, (7)–(8) ejemplos y docs actualizados. Decisiones de negocio (Vault, variables exactas) ahora claras — `§Resolución de gaps línea 149–168`
- [INFO] Alineación arquitectónica confirmada — T3 ConfigOptions distribuidas encajan con modularidad .NET; T1/T2 gitleaks integra en CI (HU-00.09 done); ADR-003 no contradice; T4a validateEnv reutilizable sin duplication (HU-00.03). Dependencias HU-00.01, HU-00.09 done. — `ARD.md, ADR-003-contratos-primero.md, sprint_status.yml:119,135`

### Hallazgos de negocio
- [BLOQUEANTE] T4a y T5 deben completarse en sprint-1 para desbloquear HU-00.03, HU-00.06, HU-00.10 (sprint-2). T4a exports validateEnv() que HU-00.03 reutiliza; T5 documenta variables para docker-compose/deploy. Estimación 2 pts no confirmada: si reventamos timebox, HU-00.03 (sprint-2) quedará bloqueada. — `sprint_status.yml:123 (HU-00.03 depende HU-00.12), §Dev Notes Opción A, línea 142`
- [REVISAR] T6 scope resuelto (Vault Docker) pero entrega DoD depende de que T3, T4a, T4b, T5 se cierren en sprint-1. Si cualquiera de estas se diferidas, T6 no podrá cerrar DoD (variables incompletas en docs). — `0-12-configuracion-y-secretos.md:94, §Definición de Hecho`
- [INFO] Criterios de aceptación específicos (CA3 ConnectionStrings__Postgres, CA4 exactas CORE_BASE_URL/PORT/NODE_ENV, CA5 .env.example documentado). Reconciliados tras story-gaps. Reducen riesgo de desacuerdo en demo — `§Criterios de aceptación línea 24–57`
- [INFO] Impacto en sprint-2: HU-00.03 (validateEnv), HU-00.06 (.env.example), HU-00.10 (documentación secretos) bloqueadas downstream. Todas críticas para R0 (Fundaciones). Sprint-1 es bloqueador. — `sprint_status.yml:100–107`

### Acciones para quedar Ready
- [x] **Sprint Planning (2026-11-02):** (1) Confirmar T4a como MVP no-negotiable en sprint-1 (no diferir a HU-00.03). (2) **RESUELTA:** T6 usa HashiCorp Vault (Docker) para staging. (3) Revisar estimación: ¿2 puntos es suficiente o descomponer en 2 historias? [Pendiente sprint planning]
- [x] **Antes del sprint:** Formalizar patrón de ConfigOptions en T3 — **RESUELTA:** Distribuido en `<Módulo>.Application/Options/<ModuloNameOptions>.cs` (uno por módulo: Catalog, Tracking, Calendar). Actualizar ejemplo CA3.
- [x] **Antes del sprint:** Especificar lógica de detección T5 — **RESUELTA:** Busca patrón `*Options.cs` en `services/core/*/Application/Options/`. Documentar en tarea T5.
- [x] **Antes del sprint:** Reconciliar CA4 con T4b — **RESUELTA:** Variables exactas: `['CORE_BASE_URL', 'PORT', 'NODE_ENV']` (sin CORS_ORIGINS; se añaden en HU-00.03 si es necesario).
- [x] **Para T1:** Proporcionar template .gitleaks.toml con ruleslist estándar + allowlist para fixtures. **RESUELTA:** Usar ruleslist estándar de gitleaks (patrones: claves privadas, tokens JWT, API keys, etc.) + allowlist para test data.
- [x] **Para T5:** Definir comportamiento de sync-env-example.spec.ts — **RESUELTA:** Falla el build si hay inconsistencia (variable en código sin doc, o doc sin variable).

### Resolución de gaps — 2026-11-01 (Iteración 2 — story-gaps)
- Gaps resueltos: 8
  1. **AMBIGÜEDAD:** T3 patrón ConfigOptions — Decisión: Distribuido en `<Módulo>.Application/Options/<ModuloNameOptions>.cs` (uno por módulo).
  2. **AMBIGÜEDAD:** T6 gestor de secretos staging — Decisión: HashiCorp Vault (Docker).
  3. **AMBIGÜEDAD:** T5 lógica detección ConfigOptions — Decisión: Patrón `*Options.cs` en `services/core/*/Application/Options/`.
  4. **AMBIGÜEDAD:** CA4/T4b variables inconsistentes — Decisión: Variables exactas `['CORE_BASE_URL', 'PORT', 'NODE_ENV']` (sin CORS_ORIGINS).
  5. **DOCUMENTACIÓN:** T1 template .gitleaks.toml — Resuelto: Ruleslist estándar gitleaks + allowlist para test data.
  6. **AMBIGÜEDAD:** T5 comportamiento sync-env-example.spec.ts — Decisión: Falla el build si hay inconsistencia.
  7. **DOCUMENTACIÓN:** T3 CA3 ejemplo — Actualizado con patrón distribuido.
  8. **DOCUMENTACIÓN:** T5 especificación detección — Actualizado con patrón *Options.cs.
- Gaps escalados (decisión pendiente): 1 (Revisión de estimación 2 pts — pendiente Sprint Planning 2026-11-02)
- Dependencias registradas: 0
- Cambios realizados en la historia:
  - T1: configuración estándar .gitleaks.toml con ruleslist + allowlist
  - T3: patrón ConfigOptions distribuido + ejemplo de nomenclatura
  - T4b: variables BFF exactas (sin CORS_ORIGINS)
  - T5: lógica detección (*Options.cs) + comportamiento (falla build)
  - T6: decisión Vault + GitHub Environments
  - Acciones para quedar Ready: 5 de 6 completadas; 1 pendiente Sprint Planning
