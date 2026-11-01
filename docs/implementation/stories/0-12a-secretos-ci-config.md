# HU-00.12a · Gitleaks + ConfigOptions (CI y configuración)

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-12a-secretos-ci-config` |
| Tipo | Habilitadora |
| Prioridad | Must |
| Estimación | 1.5 pts |
| Sprint | sprint-1 |
| Estado | ready-for-dev |
| Depende de | HU-00.01, HU-00.09 |
| Bloquea | HU-00.06, HU-00.10 |

## Historia

**Como** equipo, **quiero** que los secretos nunca lleguen al repositorio y que la configuración se valide al arrancar los servicios .NET, **para** desplegar de forma segura en cada entorno.

## Contexto

- Twelve-Factor: configuración en el entorno, secretos nunca versionados.
- La DoD de EP-00 exige que los servicios fallen rápido ante configuración incompleta.
- Esta historia cubre CI/governanza (gitleaks) + configuración .NET (ConfigOptions distribuidas).

## Criterios de aceptación

```gherkin
Característica: Configuración por entorno y secretos protegidos (CI + .NET)

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
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA3 | Integración | `Host_sin_connection_string_falla_nombrando_la_variable` | xUnit + `WebApplicationFactory` con configuración vacía |
| 2 | CA3 | Unitaria | `Opciones_de_cada_modulo_tienen_validacion_al_arrancar` | xUnit (`ValidateOnStart`) |
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
- [ ] T6 Documentar dónde viven los secretos por entorno: GitHub Environments para CI/CD + HashiCorp Vault (Docker) para staging (acceso interno; credenciales de Vault en github.com/settings/secrets para CD). Producción fuera de scope.

## Notas técnicas y decisiones

- En local se usa `.env` (no versionado) y, para .NET, opcionalmente `dotnet user-secrets`.
- ConfigOptions distribuidas (patrón modular): un archivo por módulo en `<Módulo>.Application/Options/`.
- T1 usa ruleslist estándar de gitleaks; allowlist se proporciona en `.gitleaks.toml` template para evitar falsos positivos en fixtures.

## Fuera de alcance

- Rotación de secretos y gestor de secretos de producción.
- Validación de configuración del BFF (Jest + validateEnv) — ver HU-00.12b.
- Sincronización de variables (`.env.example`) — ver HU-00.12b (T4b) y HU-00.12b sprint-2 (T5).

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Job `secrets` como check requerido (vía `ci-ok`).
- [ ] Historial de git revisado con gitleaks sin hallazgos.
- [ ] ConfigOptions para Catalog, Tracking, Calendar modules con `ValidateOnStart` en Host.Program.cs.

## Readiness Review

- Fecha: 2026-11-02 (Sprint Planning — descomposición HU-00.12)
- Resultado: ready-for-dev
- Notas: Descompuesta de HU-00.12 original. T1, T2, T3, T6 clara y sin ambigüedad tras story-gaps (2026-11-01). T4a, T4b, T5 movidas a HU-00.12b.
