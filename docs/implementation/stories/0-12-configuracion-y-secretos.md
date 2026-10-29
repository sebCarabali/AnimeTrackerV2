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

  Escenario: CA4 — BFF falla rápido
    Dado el BFF
    Cuando arranca sin una variable obligatoria
    Entonces termina con código distinto de 0 y el error nombra la variable

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
| 3 | CA4 | Unitaria | `validateEnv nombra cada variable faltante` | Jest |
| 4 | CA5 | Estructura | `.env.example contiene todas las claves del esquema del BFF y de las opciones del núcleo` | Vitest en `tools/repo-checks` |
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

- [ ] T1 (CA1) Job `secrets` en CI con gitleaks (configuración `.gitleaks.toml` con allowlist para fixtures).
- [ ] T2 (CA2) Hook de pre-commit con gitleaks vía husky.
- [ ] T3 (CA3) Clases de opciones tipadas por módulo con `ValidateDataAnnotations().ValidateOnStart()` y mensaje con el nombre de la variable de entorno.
- [ ] T4 (CA4) Reutilizar la validación del BFF de HU-00.03 y cubrir todas las variables.
- [ ] T5 (CA5) `.env.example` en la raíz y prueba de sincronía.
- [ ] T6 Documentar dónde viven los secretos por entorno (GitHub Environments para CI/CD; gestor de secretos de la plataforma de staging).

## Notas técnicas y decisiones

- En local se usa `.env` (no versionado) y, para .NET, opcionalmente `dotnet user-secrets`.
- La prueba de sincronía de `.env.example` evita que una variable nueva se olvide en la documentación.

## Fuera de alcance

- Rotación de secretos y gestor de secretos de producción.

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Job `secrets` como check requerido (vía `ci-ok`).
- [ ] Historial de git revisado con gitleaks sin hallazgos.
