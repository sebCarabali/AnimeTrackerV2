# contracts — Contratos OpenAPI

**Propósito:** Definiciones de los contratos de API en formato OpenAPI 3.x. Sirven como fuente de verdad para la comunicación entre el BFF y el núcleo .NET, y entre la SPA/extensión y el BFF. El enfoque es API-first: los contratos se definen antes de implementar.

**Dueño:** Backend team (con revisión de Frontend)

## Estructura

```
contracts/
  bff/          # Contratos expuestos por el BFF hacia la SPA y la extensión
  core/         # Contratos del núcleo .NET expuestos al BFF
```

## Uso

Los contratos se usan para generar clientes TypeScript (para la SPA y la extensión) y modelos C# (para el núcleo .NET). Ver HU-00.07 para detalles del pipeline de generación de código.
