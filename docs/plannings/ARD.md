# Documento de Arquitectura de Software (ARD)
## Plataforma de Seguimiento de Anime (Anime Tracker)

### 1. Visión General
Este documento define la arquitectura de software para la plataforma **Anime Tracker**, detallada en el `PRD.md`. El sistema permite gestionar el consumo de anime, automatizar el tracking mediante una extensión de navegador y proveer herramientas de descubrimiento y calendario. La arquitectura está diseñada bajo los principios de **Domain-Driven Design (DDD)** y **Clean Architecture**, asegurando alta cohesión, bajo acoplamiento y facilidad de evolución.

---

### 2. Decisiones Arquitectónicas Principales (ADRs)

| Componente | Tecnología Seleccionada | Justificación y Estrategia |
| :--- | :--- | :--- |
| **Backend Core** | **.NET 8 (C#)** | Implementado como un **Monolito Modular**. Provee alto rendimiento y un ecosistema robusto (Entity Framework Core, inyección de dependencias nativa). Los módulos estarán lógicamente separados (proyectos distintos en la solución) para permitir un despliegue unificado pero habilitando la futura extracción a microservicios si el tráfico de tracking o catálogo lo requiere. |
| **BFF (Backend for Frontend)** | **NestJS (Node.js)** | Actúa como un *API Gateway* y agregador. NestJS (usando TypeScript) comparte filosofía arquitectónica (inyección de dependencias, decoradores) con Angular, facilitando el desarrollo Full-Stack. El BFF orquestará llamadas complejas al backend .NET, manejará la sesión de usuario y adaptará los payloads (REST/GraphQL) específicamente para la SPA y la extensión. |
| **Frontend (SPA)** | **Angular** | Ideal para aplicaciones empresariales y estructuradas. Su sistema de módulos, servicios reactivos (RxJS) y tipado estricto (TypeScript) asegura un manejo de estado robusto para la biblioteca de animes y el calendario en tiempo real. |
| **Persistencia** | **PostgreSQL & Redis** | PostgreSQL manejará el estado transaccional (WatchHistory, Usuarios). Redis actuará como capa de caché distribuida para consultas pesadas como el catálogo de temporada (simulcasts) y el mapeo rápido de IDs de la API externa. |

---

### 3. Vista de Contenedores (C4 Model)

El siguiente diagrama ilustra cómo interactúan los componentes principales del ecosistema:

```mermaid
graph TD
    %% Nodos de Usuario
    User((Usuario))
    Browser[Navegador Web\nReproductor de Video]
    
    %% Frontend y Extensión
    SPA[Angular SPA\nGestión y Calendario]
    Ext[Extensión de Navegador\nAuto-Tracker]
    
    %% Capa Intermedia
    BFF[NestJS BFF\nGateway & Agregador]
    
    %% Backend Core
    NetBackend[.NET 8 Monolito Modular\nClean Architecture]
    
    %% Persistencia y Externos
    PG[(PostgreSQL\nWatchHistory & Auth)]
    Redis[(Redis\nCaché de Catálogo)]
    Jikan[API Externa\nJikan/AniList]

    %% Relaciones
    User -->|Usa| SPA
    User -->|Ve Anime| Browser
    Browser -->|Inyecta| Ext
    SPA -->|REST / JSON| BFF
    Ext -->|REST Mutaciones de Progreso| BFF
    BFF -->|REST / gRPC| NetBackend
    NetBackend -->|EF Core| PG
    NetBackend -->|Lectura/Escritura| Redis
    NetBackend -->|Capa Anticorrupción (HTTP)| Jikan
```

---

### 4. Diseño del Monolito Modular (.NET 8)

El backend en .NET no será una "bola de lodo" (Big Ball of Mud), sino que estará estrictamente dividido en **Módulos de Dominio** independientes. Cada módulo tendrá su propia capa de API, Aplicación, Dominio e Infraestructura.

#### 4.1. Módulos Definidos
1. **Módulo de Catálogo (Catalog Module):**
   * Encargado de consultar, sincronizar y almacenar metadatos de los animes.
   * Implementa una **Capa Anticorrupción (ACL)**: Traduce los modelos externos (ej. Jikan REST API) al modelo de dominio interno (`Anime`). Esto garantiza que los cambios en la API externa no rompan el sistema.
2. **Módulo de Seguimiento (Tracking Module):**
   * Core del negocio (`WatchHistory`). Gestiona los estados (*Viendo, Completado*, etc.) y los contadores de episodios.
   * Optimizado para escrituras rápidas para cumplir con el SLA de **< 200ms** requerido por la extensión de navegador.
3. **Módulo de Calendario y Descubrimiento (Calendar Module):**
   * Encargado de calcular las fechas de emisión (simulcasts) ajustando el *timezone* (UTC internamente, delegando el cálculo de desfase al BFF/SPA).
   * Altamente dependiente de **Redis** para servir los cronogramas semanales cacheados.

#### 4.2. Reglas de Comunicación entre Módulos
* Los módulos **no comparten tablas de base de datos**. Si el `Tracking Module` necesita el nombre de un anime, lo obtiene a través de una interfaz en memoria (Mediator pattern) provista por el `Catalog Module`, o almacena una proyección de solo lectura.
* Esto garantiza que si mañana el `Tracking Module` se convierte en un microservicio autónomo, la separación ya está hecha.

---

### 5. Backend For Frontend (NestJS)

El BFF asume las siguientes responsabilidades para simplificar la SPA de Angular y la extensión:
* **Agregación de Datos:** En la vista del "Dashboard", el BFF hace una llamada al `Catalog Module` en .NET y otra al `Tracking Module`, une los datos y devuelve un único JSON a Angular.
* **Manejo de Sesión y Seguridad:** Mantiene los tokens JWT o cookies HttpOnly, verificando la autenticación antes de dejar pasar la petición hacia el backend de .NET.
* **Ruta Rápida (Fast-Track) para la Extensión:** Expone un endpoint `/api/v1/track` optimizado que simplemente valida el token y encola el evento de actualización de progreso directamente hacia el backend .NET, asegurando la baja latencia (SLA < 200ms).

---

### 6. Criterios de Aceptación y Atributos de Calidad (NFRs)

* **Rendimiento (Performance):** 
  * El endpoint de tracking (`POST /progress`) debe resolverse en **menos de 200ms**. *Solución: El BFF y el módulo .NET validarán la petición y retornarán `202 Accepted` de inmediato, procesando la actualización de la DB asíncronamente si es necesario.*
* **Mantenibilidad (API-First):**
  * Toda la comunicación entre Angular <-> NestJS y NestJS <-> .NET estará definida por contratos **OpenAPI (Swagger)** antes de su implementación (Metodología Contratos Primero).
* **Desacoplamiento Externo:**
  * El modelo de dominio `Anime` usará un UUID interno. El ID de la API externa (ej. `mal_id`) se guardará como un atributo secundario `ExternalId` dentro del módulo de catálogo.
* **Precisión Horaria:**
  * Todas las fechas y horas se almacenarán y transmitirán en **UTC (ISO 8601)** desde el backend .NET y la BD.
  * El BFF enviará la fecha UTC a Angular, y Angular utilizará pipes nativos (ej. `date:'short':'local'`) para aplicar el cálculo de desfase horario en el cliente.