# HU-00.11 · Observabilidad base [Técnica]

| Campo | Valor |
|---|---|
| Épica | EP-00 · Configuración e inicialización del proyecto |
| Clave de sprint | `0-11-observabilidad-base` |
| Tipo | Técnica |
| Prioridad | Should |
| Estimación | 5 pts |
| Sprint | sprint-2 |
| Estado | ready-for-dev |
| Depende de | HU-00.02, HU-00.03, HU-00.04 (parcial, solo T6), HU-00.06 |
| Bloquea | HU-07.01 (medición por salto) |

## Historia

**Como** desarrollador, **quiero** logs estructurados, trazas y un identificador de correlación de punta a punta, **para** diagnosticar problemas entre SPA, BFF y .NET.

## Contexto

- El SLA de < 200 ms del endpoint de tracking (PRD §6) solo se puede defender si se mide la duración de cada salto.
- Se usa OpenTelemetry para no atarse a un proveedor.

## Criterios de aceptación

```gherkin
Característica: Correlación y trazas de punta a punta

  Escenario: CA1 — El BFF genera el identificador si no llega
    Dado una petición a "/api/v1/..." sin cabecera "X-Correlation-Id"
    Cuando el BFF la procesa
    Entonces genera un identificador UUID
    Y lo devuelve en la cabecera de respuesta "X-Correlation-Id"

  Escenario: CA2 — El identificador se respeta y propaga
    Dado una petición desde la SPA con "X-Correlation-Id: abc-123"
    Cuando atraviesa el BFF y el núcleo .NET
    Entonces la llamada del BFF al núcleo incluye "X-Correlation-Id: abc-123"
    Y todos los logs de esa petición en BFF y núcleo tienen "correlationId": "abc-123"

  Escenario: CA3 — Identificador inválido
    Dado una petición con "X-Correlation-Id" de más de 64 caracteres o con caracteres no permitidos
    Cuando llega al BFF
    Entonces se descarta y se genera uno nuevo

  Escenario: CA4 — Logs estructurados
    Dado cualquier servicio en ejecución
    Cuando escribe un log
    Entonces el log es JSON con "timestamp" en UTC ISO 8601, "level", "service", "message", "correlationId" y "traceId"
    Y no contiene contraseñas, tokens ni cookies

  Escenario: CA5 — Trazas por salto
    Dado una petición SPA → BFF → núcleo
    Cuando se procesa
    Entonces se emiten spans OpenTelemetry del BFF y del núcleo en la misma traza
    Y cada span registra la duración y la ruta HTTP

  Escenario: CA6 — Visualización local
    Dado el entorno de Docker Compose
    Cuando hago una petición
    Entonces puedo ver la traza completa en el visor local de trazas
```

## Estrategia TDD

| # | CA | Nivel | Prueba (rojo primero) | Herramienta |
|---|---|---|---|---|
| 1 | CA1 | E2E de módulo | `respuesta incluye X-Correlation-Id generado` | Jest + Supertest |
| 2 | CA2 | E2E de módulo | `llamada saliente al núcleo propaga el id` | Jest + nock (asserta cabecera) |
| 3 | CA3 | Unitaria | `sanitizeCorrelationId descarta valores inválidos` | Jest |
| 4 | CA2, CA4 | Integración | `Middleware_agrega_CorrelationId_al_scope_de_log` | xUnit + sink de Serilog en memoria |
| 5 | CA4 | Unitaria | `Log_no_contiene_cabeceras_sensibles` | xUnit / Jest |
| 6 | CA5 | Integración | `Peticion_genera_span_con_ruta_http` | xUnit + `InMemoryExporter` de OpenTelemetry |

Ejemplo .NET (rojo):

```csharp
[Fact]
public async Task Middleware_agrega_CorrelationId_al_scope_de_log()
{
    var sink = new InMemoryLogSink();
    var client = _factory.WithLogSink(sink).CreateClient();
    client.DefaultRequestHeaders.Add("X-Correlation-Id", "abc-123");

    await client.GetAsync("/health");

    sink.Events.ShouldAllBe(e => e.Properties["CorrelationId"].ToString() == "\"abc-123\"");
}
```

## Tareas

- [ ] T1 (CA1–CA3) BFF: middleware de correlación con `AsyncLocalStorage` (o `nestjs-cls`) e interceptor del cliente HTTP generado.
- [ ] T2 (CA4) BFF: logger JSON (`nestjs-pino`) con redacción de `authorization`, `cookie`, `set-cookie`.
- [ ] T3 (CA2, CA4) Núcleo: middleware que lee la cabecera y abre un scope de log; Serilog con salida JSON y enriquecedores.
- [ ] T4 (CA5) OpenTelemetry en BFF (`@opentelemetry/sdk-node` + instrumentación HTTP) y núcleo (`OpenTelemetry.Extensions.Hosting` + ASP.NET Core, HttpClient, EF Core, Npgsql).
- [ ] T5 (CA6) Agregar `otel-collector` y un visor (Jaeger o Aspire Dashboard) a `infra/docker-compose.yml` (entorno local).
- [ ] T6 SPA: interceptor HTTP que agrega `X-Correlation-Id` por petición (opcional, el BFF lo genera si falta); coordinar con HU-00.04, no bloquea el resto de esta historia.
- [ ] T7 (Q5) Stack de observabilidad en staging: Prometheus + Grafana + Loki/Promtail autohospedados en el servidor único (vía `docker-compose.staging.yml` de HU-00.06), expuestos a través del reverse proxy de HU-00.10 (Caddy/Traefik) con autenticación. Vigilar consumo de recursos compartido con `bff`/`core` en esa misma VM.

## Notas técnicas y decisiones

- Se propaga también `traceparent` (W3C Trace Context); `X-Correlation-Id` sigue existiendo porque es legible para soporte y para la extensión.
- La SPA no exporta trazas en esta historia (se evaluará en EP-07).
- **Q5 cerrada (2026-11-01):** al ser staging un servidor único self-hosted (no una plataforma gestionada), el stack de observabilidad de staging (T7) es autohospedado (Prometheus/Grafana/Loki) en vez de un servicio como Azure Monitor. Detalle completo en `sprint_status.yml` (preguntas_abiertas → Q5).

## Fuera de alcance

- Alertas y dashboards de producción.
- Métricas de negocio.

## Definición de Hecho

- [ ] DoD común cumplida.
- [ ] Captura de una traza completa SPA → BFF → núcleo adjunta al PR.
- [ ] Guía "cómo seguir una petición" en `docs/`.
