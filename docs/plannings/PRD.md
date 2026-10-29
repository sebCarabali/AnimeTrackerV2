# Documento de Requisitos del Producto (PRD)
## Plataforma de Seguimiento de Anime (Anime Tracker)

Este documento describe las especificaciones para una aplicación web y su ecosistema, diseñada para la gestión integral del consumo de anime. El sistema permite a los usuarios mantener un registro preciso de las series que están viendo, controlar el progreso por episodios, recibir notificaciones de nuevos lanzamientos y gestionar listas de favoritos, con capacidad de automatizar el tracking de visualización.

### 1. Visión y Objetivos
* **Control de Consumo:** Proveer una interfaz sin fricción para registrar manualmente el progreso de visualización de capítulos.
* **Automatización del Tracking:** Habilitar una integración nativa con una extensión de navegador complementaria para registrar automáticamente el progreso cuando el usuario consume contenido en plataformas de streaming de terceros.
* **Descubrimiento y Planificación:** Mostrar un calendario de emisión dinámico en tiempo real para animes en temporada (simulcasts).

### 2. Historias de Usuario (Core Scope)
* **Gestión de Catálogo:** Como usuario, quiero buscar animes por título o género y agregarlos a mi biblioteca personal para mantener mi registro al día.
* **Seguimiento de Progreso:** Como usuario, quiero marcar capítulos específicos como "vistos" (manual o automáticamente) para saber exactamente dónde retomarlos.
* **Calendario de Emisiones:** Como usuario, quiero visualizar cuándo se estrena el próximo capítulo de los animes que sigo, ajustado a mi zona horaria.
* **Favoritos:** Como usuario, quiero marcar animes con un estado de "Favorito" para destacarlos y acceder a ellos rápidamente en el dashboard principal.

### 3. Requisitos Funcionales
* **Módulo de Sincronización Externa:**
  * Integración con una API pública de metadatos (ej. Jikan REST API o AniList GraphQL) para hidratar el sistema con información oficial (sinopsis, cover art, fechas de estreno, total de capítulos).
* **Módulo de Tracking (WatchHistory):**
  * Estados soportados: *Viendo, Completado, En Espera, Abandonado, Plan para ver*.
  * Contador incremental de episodios (X vistos de Y totales).
* **Módulo de Calendario:**
  * Vista de cronograma semanal para emisiones actuales.
  * Cálculo de desfase horario para el timezone del cliente.

### 4. Arquitectura de Software Sugerida
Para garantizar un sistema escalable y mantenible, la arquitectura se diseñará bajo principios de **Domain-Driven Design (DDD)** y **Clean Architecture** (Arquitectura Hexagonal).

| Componente | Especificación Técnica Recomendada |
| :--- | :--- |
| **Frontend (SPA)** | React, Vue o Angular, estructurado para consumir servicios inyectados y manejar el estado de la biblioteca de forma reactiva. |
| **Backend / BFF** | Node.js, .NET 8, o Java (Spring Boot) que orqueste la lógica de negocio y actúe como Backend-For-Frontend para la SPA. |
| **Extensión de Navegador** | Script ligero inyectado que detecte eventos de finalización en reproductores de video web y envíe mutaciones de progreso vía REST al backend. |
| **Persistencia** | PostgreSQL para el modelo relacional del `WatchHistory` del usuario, apoyado por Redis para cachear las respuestas pesadas del catálogo semanal. |

### 5. Metodología de Desarrollo
El ciclo de vida del desarrollo priorizará un enfoque de diseño por contratos:
1. **Contratos Primero:** Se definirán las especificaciones de API (OpenAPI) antes de escribir la lógica de negocio.
2. **Estructura del Proyecto:** Un monorepo para gestionar fácilmente tanto los componentes frontend como los microservicios backend si fuera necesario.

### 6. Criterios de Aceptación
* El endpoint de mutación de progreso de episodios debe responder en menos de 200ms para evitar interrupciones en la experiencia de la extensión del navegador.
* El modelo de dominio de `Anime` no debe acoplarse estrictamente a las respuestas de la API externa (implementar capa anticorrupción).
* El calendario debe renderizar correctamente los días de la semana según la hora local del usuario.