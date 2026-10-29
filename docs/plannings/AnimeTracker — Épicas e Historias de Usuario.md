# AnimeTracker — Épicas e Historias de Usuario

Oct 28, 2026 · @Sebastian Carabali

El backlog de AnimeTracker se organiza en 8 épicas y 51 historias de usuario (195 puntos estimados), derivadas del PRD y del ARD. EP-00 deja lista la base técnica: monorepo, monolito modular .NET 8, BFF NestJS, SPA Angular, contratos OpenAPI y CI/CD.

## Convenciones

- **Formato:** Como \<rol>, quiero \<capacidad>, para \<beneficio>.
- **Criterios de aceptación:** escenarios Gherkin (Dado / Cuando / Entonces).
- **Prioridad:** MoSCoW (Must, Should, Could, Won't en esta versión).
- **Estimación:** puntos de historia en escala Fibonacci (1, 2, 3, 5, 8). Una historia de 8 puntos es candidata a dividirse en refinamiento.
- **Historias técnicas:** marcadas como *\[Técnica\]*; su rol es el equipo de desarrollo y existen para cumplir un requisito no funcional o arquitectónico del ARD.
- **Definición de Hecho (DoD) común:** código revisado en PR, contrato OpenAPI actualizado antes de implementar, pruebas unitarias y de integración en verde en CI, fechas en UTC ISO 8601 y desplegado en el entorno de staging.

## Mapa de épicas

| ID | Épica | Objetivo | Componentes | Historias | Puntos | Prioridad |
| --- | --- | --- | --- | --- | --- | --- |
| EP-00 | Configuración e inicialización | Base técnica lista para desarrollar y desplegar | Todo el monorepo | 12 | 45 | Must |
| EP-01 | Identidad y autenticación | Cuentas, sesión y vinculación de la extensión | BFF, módulo de identidad | 5 | 18 | Must |
| EP-02 | Catálogo y sincronización externa | Buscar animes con datos oficiales desacoplados de Jikan | Catalog Module, Redis | 6 | 24 | Must |
| EP-03 | Biblioteca y seguimiento | Registrar estados y progreso por episodio | Tracking Module | 8 | 23 | Must |
| EP-04 | Favoritos y dashboard | Acceso rápido a lo que más importa | BFF (agregación), SPA | 3 | 10 | Must |
| EP-05 | Calendario de emisiones | Saber cuándo sale el próximo episodio, en hora local | Calendar Module, Redis, SPA | 6 | 24 | Must |
| EP-06 | Extensión auto-tracker | Registrar progreso automáticamente al ver un episodio | Extensión, BFF fast-track | 7 | 34 | Must |
| EP-07 | Calidad transversal (NFR) | Rendimiento, seguridad y pruebas extremo a extremo | Todos | 4 | 17 | Must / Should |

## EP-00 · Configuración e inicialización del proyecto

Objetivo: que cualquier desarrollador clone el repositorio y, con un comando, tenga los cinco componentes (SPA, BFF, núcleo .NET, extensión, base de datos y caché) corriendo, probados en CI y respetando las fronteras de módulos del ARD.

### HU-00.01 · Estructura del monorepo

**Como** desarrollador, **quiero** un monorepo con una estructura de carpetas estándar, **para** gestionar frontend, BFF, backend y extensión en un solo lugar.

*Must · 3 pts*

- **Dado** que clono el repositorio, **cuando** reviso la raíz, **entonces** encuentro `apps/spa`, `apps/bff`, `apps/extension`, `services/core`, `contracts/` e `infra/`.
- **Dado** el repositorio, **cuando** abro el README, **entonces** describe requisitos previos, cómo levantar el entorno y la convención de ramas y commits (Conventional Commits).
- **Dado** cualquier archivo, **cuando** lo edito, **entonces** `.editorconfig` y `.gitignore` comunes aplican a todos los proyectos.

### HU-00.02 · Solución .NET 8 como monolito modular *\[Técnica\]*

**Como** desarrollador backend, **quiero** una solución .NET 8 con un proyecto por módulo y capa, **para** mantener las fronteras de Clean Architecture y poder extraer módulos a microservicios en el futuro.

*Must · 5 pts*

- **Dado** la solución, **cuando** la abro, **entonces** existen los módulos Catalog, Tracking y Calendar, cada uno con proyectos Api, Application, Domain e Infrastructure, más un proyecto Host y un SharedKernel.
- **Dado** que un proyecto Domain referencia a otro módulo o a Infrastructure, **cuando** se ejecutan las pruebas de arquitectura (p. ej. NetArchTest), **entonces** la compilación en CI falla.
- **Dado** que un módulo necesita datos de otro, **cuando** lo implementa, **entonces** usa una interfaz pública expuesta por Mediator (MediatR) y nunca sus repositorios o tablas.

### HU-00.03 · Proyecto BFF NestJS inicial

**Como** desarrollador full-stack, **quiero** un BFF NestJS configurado, **para** empezar a exponer endpoints a la SPA y a la extensión.

*Must · 3 pts*

- **Dado** el BFF arrancado, **cuando** llamo `GET /health`, **entonces** responde 200 e indica el estado de la conexión con el núcleo .NET.
- **Dado** el contrato OpenAPI del núcleo, **cuando** ejecuto el script de generación, **entonces** obtengo un cliente HTTP tipado que el BFF usa para llamar a .NET.
- **Dado** un entorno (dev, staging, prod), **cuando** arranca el BFF, **entonces** carga su configuración con `@nestjs/config` y falla rápido si falta una variable obligatoria.

### HU-00.04 · SPA Angular inicial

**Como** desarrollador frontend, **quiero** una aplicación Angular con enrutamiento, layout base y cliente API generado, **para** construir las pantallas sobre una base común.

*Must · 3 pts*

- **Dado** la SPA, **cuando** ejecuto `ng serve`, **entonces** veo el layout con navegación a Biblioteca, Buscar, Calendario y Dashboard (páginas vacías).
- **Dado** el contrato OpenAPI público del BFF, **cuando** ejecuto la generación, **entonces** obtengo servicios Angular tipados para consumirlo.
- **Dado** un PR, **cuando** corre CI, **entonces** ESLint y las pruebas unitarias de la SPA se ejecutan y bloquean el merge si fallan.

### HU-00.05 · Esqueleto de la extensión de navegador

**Como** desarrollador, **quiero** un proyecto de extensión Manifest V3 en TypeScript con su build, **para** desarrollar el auto-tracker sobre una base empaquetable.

*Should · 3 pts*

- **Dado** el proyecto, **cuando** ejecuto el build, **entonces** se genera una carpeta instalable en Chrome en modo desarrollador.
- **Dado** la extensión instalada, **cuando** abro su popup, **entonces** muestra la versión y el entorno de BFF configurado.

### HU-00.06 · Entorno local con Docker Compose

**Como** desarrollador, **quiero** levantar PostgreSQL, Redis y los servicios con un solo comando, **para** reproducir el sistema completo en mi máquina.

*Must · 3 pts*

- **Dado** Docker instalado, **cuando** ejecuto `docker compose up`, **entonces** arrancan PostgreSQL, Redis, el núcleo .NET y el BFF, y sus health checks pasan.
- **Dado** una base vacía, **cuando** arranca el entorno, **entonces** se aplican las migraciones y un seed con usuarios y animes de prueba.

### HU-00.07 · Contratos OpenAPI primero *\[Técnica\]*

**Como** equipo, **quiero** que los contratos OpenAPI se definan y versionen antes de implementar, **para** cumplir la metodología Contratos Primero del PRD.

*Must · 5 pts*

- **Dado** `contracts/`, **cuando** lo reviso, **entonces** hay dos especificaciones versionadas: la pública (SPA y extensión ↔ BFF) y la interna (BFF ↔ .NET).
- **Dado** un cambio en un contrato, **cuando** abro un PR, **entonces** CI ejecuta un linter (p. ej. Spectral) y detecta cambios incompatibles frente a la versión publicada.
- **Dado** una implementación, **cuando** corren las pruebas de contrato, **entonces** fallan si las respuestas no cumplen el esquema.

### HU-00.08 · Persistencia y migraciones por módulo *\[Técnica\]*

**Como** desarrollador backend, **quiero** que cada módulo tenga su propio esquema de PostgreSQL y sus migraciones de EF Core, **para** que los módulos no compartan tablas.

*Must · 3 pts*

- **Dado** la base de datos, **cuando** se aplican las migraciones, **entonces** existen los esquemas `catalog`, `tracking`, `calendar` e `identity`, cada uno con su propio DbContext.
- **Dado** un módulo, **cuando** intenta mapear una tabla de otro esquema, **entonces** las pruebas de arquitectura lo rechazan.

### HU-00.09 · Pipeline de integración continua

**Como** equipo, **quiero** que cada PR compile, pruebe y analice solo los proyectos afectados, **para** detectar errores antes del merge sin ralentizar el flujo.

*Must · 5 pts*

- **Dado** un PR, **cuando** cambia un proyecto, **entonces** CI ejecuta build, lint y pruebas de ese proyecto y de sus dependientes.
- **Dado** que una prueba falla o la cobertura baja del umbral acordado, **cuando** termina CI, **entonces** el merge queda bloqueado.

### HU-00.10 · Despliegue continuo a staging

**Como** equipo, **quiero** que cada merge a `main` publique imágenes y despliegue en staging, **para** validar los cambios en un entorno real.

*Should · 5 pts*

- **Dado** un merge a `main`, **cuando** termina el pipeline, **entonces** las imágenes de BFF y núcleo .NET quedan versionadas en el registro y desplegadas en staging.
- **Dado** un despliegue fallido, **cuando** fallan los health checks, **entonces** se mantiene la versión anterior.

### HU-00.11 · Observabilidad base *\[Técnica\]*

**Como** desarrollador, **quiero** logs estructurados, trazas y un identificador de correlación de punta a punta, **para** diagnosticar problemas entre SPA, BFF y .NET.

*Should · 5 pts*

- **Dado** una petición desde la SPA, **cuando** atraviesa BFF y núcleo, **entonces** todos los logs comparten el mismo `correlation-id`.
- **Dado** el núcleo y el BFF, **cuando** procesan peticiones, **entonces** emiten trazas OpenTelemetry con la duración de cada salto.

### HU-00.12 · Configuración y secretos

**Como** equipo, **quiero** que la configuración venga de variables de entorno y los secretos nunca estén en el repositorio, **para** desplegar de forma segura en cada entorno.

*Must · 2 pts*

- **Dado** un PR, **cuando** CI analiza el código, **entonces** un escáner de secretos bloquea credenciales expuestas.
- **Dado** un servicio, **cuando** arranca sin una variable obligatoria, **entonces** termina con un error que nombra la variable faltante.

## EP-01 · Identidad y autenticación

Objetivo: que cada usuario tenga una cuenta propia, una sesión segura en la SPA (cookie HttpOnly gestionada por el BFF, según el ARD) y una forma revocable de vincular la extensión. El PRD no detalla el método de autenticación; se asume email y contraseña.

### HU-01.01 · Registro de cuenta

**Como** visitante, **quiero** registrarme con email y contraseña, **para** tener una biblioteca personal.

*Must · 3 pts*

- **Dado** un email no registrado y una contraseña válida, **cuando** envío el formulario, **entonces** se crea la cuenta y entro al dashboard con sesión iniciada.
- **Dado** un email ya registrado, **cuando** envío el formulario, **entonces** veo un mensaje de error y no se crea una cuenta duplicada.
- **Dado** una contraseña que no cumple la política, **cuando** escribo en el campo, **entonces** veo los requisitos que faltan antes de enviar.

### HU-01.02 · Inicio y cierre de sesión

**Como** usuario, **quiero** iniciar y cerrar sesión, **para** acceder a mis datos de forma segura.

*Must · 5 pts*

- **Dado** credenciales válidas, **cuando** inicio sesión, **entonces** el BFF emite una cookie HttpOnly, Secure y SameSite, y la SPA nunca accede al token.
- **Dado** una sesión activa, **cuando** cierro sesión, **entonces** la cookie se invalida y cualquier petición posterior responde 401.
- **Dado** credenciales inválidas repetidas, **cuando** supero el límite de intentos, **entonces** el BFF bloquea temporalmente nuevos intentos para esa cuenta.

### HU-01.03 · Vincular la extensión con mi cuenta

**Como** usuario, **quiero** vincular la extensión a mi cuenta sin escribir mi contraseña en ella, **para** que registre mi progreso de forma segura.

*Must · 5 pts*

- **Dado** que tengo sesión en la SPA, **cuando** pulso "Vincular extensión", **entonces** se genera un token de dispositivo de alcance limitado (solo escritura de progreso) que la extensión guarda.
- **Dado** mis dispositivos vinculados, **cuando** reviso mi perfil, **entonces** veo la lista con fecha de último uso y puedo revocar cualquiera.
- **Dado** un token revocado, **cuando** la extensión lo usa, **entonces** recibe 401 y me pide vincular de nuevo.

### HU-01.04 · Preferencias de perfil y zona horaria

**Como** usuario, **quiero** confirmar o cambiar mi zona horaria, **para** que el calendario muestre horas correctas aunque viaje.

*Should · 2 pts*

- **Dado** mi primer inicio de sesión, **cuando** abro el perfil, **entonces** la zona horaria propuesta es la del navegador (IANA, p. ej. America/Bogota).
- **Dado** que cambio la zona horaria, **cuando** guardo, **entonces** el calendario y las fechas de la SPA se recalculan sin recargar.

### HU-01.05 · Recuperar contraseña

**Como** usuario, **quiero** restablecer mi contraseña por email, **para** recuperar el acceso si la olvido.

*Could · 3 pts*

- **Dado** un email registrado, **cuando** solicito el restablecimiento, **entonces** recibo un enlace de un solo uso que caduca en un tiempo definido.
- **Dado** un email no registrado, **cuando** solicito el restablecimiento, **entonces** veo el mismo mensaje genérico, sin revelar si la cuenta existe.

## EP-02 · Catálogo y sincronización externa

Objetivo: que el usuario encuentre cualquier anime con datos oficiales (sinopsis, portada, fecha de estreno, total de episodios), mientras el dominio `Anime` queda aislado de Jikan/AniList mediante una capa anticorrupción y un UUID interno.

### HU-02.01 · Buscar anime por título

**Como** usuario, **quiero** buscar animes por título, **para** encontrarlos y agregarlos a mi biblioteca.

*Must · 5 pts*

- **Dado** que escribo al menos 3 caracteres, **cuando** pauso la escritura, **entonces** veo resultados paginados con portada, título, año, tipo y número de episodios.
- **Dado** un anime que ya está en mi biblioteca, **cuando** aparece en los resultados, **entonces** se muestra su estado actual en lugar del botón "Agregar".
- **Dado** una búsqueda sin resultados, **cuando** termina, **entonces** veo un mensaje claro y sugerencias para ajustar el término.

### HU-02.02 · Filtrar por género

**Como** usuario, **quiero** filtrar el catálogo por uno o varios géneros, **para** descubrir series de mi interés.

*Must · 3 pts*

- **Dado** la pantalla de búsqueda, **cuando** selecciono géneros, **entonces** los resultados solo incluyen animes que tengan todos los géneros elegidos.
- **Dado** filtros activos, **cuando** comparto la URL, **entonces** otra persona ve la misma búsqueda (filtros en query string).

### HU-02.03 · Ver ficha de detalle

**Como** usuario, **quiero** ver la ficha completa de un anime, **para** decidir si lo sigo.

*Must · 3 pts*

- **Dado** un resultado, **cuando** lo abro, **entonces** veo sinopsis, portada, géneros, fecha de estreno, estado de emisión y total de episodios (o "desconocido" si sigue en emisión sin total).
- **Dado** la ficha, **cuando** pulso "Agregar a mi biblioteca", **entonces** se agrega sin salir de la página (ver HU-03.01).

### HU-02.04 · Capa anticorrupción para la API externa *\[Técnica\]*

**Como** equipo, **quiero** un adaptador que traduzca los modelos de Jikan al modelo de dominio `Anime`, **para** que un cambio en la API externa no rompa el sistema.

*Must · 5 pts*

- **Dado** una respuesta de Jikan, **cuando** el adaptador la procesa, **entonces** produce un `Anime` con UUID interno y guarda el `mal_id` como `ExternalId` secundario.
- **Dado** el dominio y la capa Application, **cuando** se ejecutan las pruebas de arquitectura, **entonces** ninguna clase fuera de Infrastructure referencia DTOs de Jikan.
- **Dado** un campo nuevo o renombrado en la API externa, **cuando** se adapta el sistema, **entonces** el cambio se limita al adaptador y sus pruebas de contrato.

### HU-02.05 · Hidratación y caché del catálogo *\[Técnica\]*

**Como** equipo, **quiero** almacenar localmente los animes consultados y cachear las consultas pesadas en Redis, **para** responder rápido y respetar los límites de tasa de la API externa.

*Must · 5 pts*

- **Dado** un anime no presente en la base local, **cuando** se consulta por primera vez, **entonces** se obtiene de la API externa, se persiste en el esquema `catalog` y se registra el mapeo `ExternalId → UUID` en Redis.
- **Dado** una consulta repetida dentro del TTL configurado, **cuando** llega, **entonces** se sirve desde Redis sin llamar a la API externa.
- **Dado** un volumen de llamadas cercano al límite publicado por la API externa, **cuando** llegan más, **entonces** el cliente las regula en lugar de recibir errores 429.

### HU-02.06 · Resiliencia ante caídas de la API externa

**Como** usuario, **quiero** seguir usando el catálogo aunque la API externa falle, **para** no perder acceso a mis animes.

*Should · 3 pts*

- **Dado** que la API externa no responde, **cuando** busco un anime ya almacenado, **entonces** obtengo los datos locales y un aviso de que podrían no estar actualizados.
- **Dado** fallos consecutivos, **cuando** se supera el umbral, **entonces** un circuit breaker (p. ej. Polly) corta las llamadas durante un periodo y luego reintenta.

## EP-03 · Biblioteca personal y seguimiento de progreso

Objetivo: que el usuario sepa en todo momento dónde retomar cada serie. El agregado `WatchHistory` del Tracking Module gestiona los cinco estados del PRD (Viendo, Completado, En Espera, Abandonado, Plan para ver) y el contador "X vistos de Y totales".

Reglas de dominio comunes a las historias de esta épica:

- El contador de episodios vistos nunca es negativo ni supera el total conocido.
- Marcar un episodio en un anime en "Plan para ver" lo pasa a "Viendo".
- Alcanzar el último episodio de un anime finalizado lo pasa a "Completado".
- Marcar "Completado" fija el contador en el total de episodios.

### HU-03.01 · Agregar un anime a mi biblioteca

**Como** usuario, **quiero** agregar un anime a mi biblioteca con un estado inicial, **para** empezar a registrar mi seguimiento.

*Must · 3 pts*

- **Dado** un anime que no está en mi biblioteca, **cuando** lo agrego sin elegir estado, **entonces** queda en "Plan para ver" con 0 episodios vistos.
- **Dado** un anime que ya está en mi biblioteca, **cuando** intento agregarlo otra vez, **entonces** no se crea un duplicado y se muestra su entrada existente.

### HU-03.02 · Cambiar el estado de un anime

**Como** usuario, **quiero** cambiar el estado de un anime entre los cinco estados soportados, **para** reflejar cómo va mi consumo.

*Must · 3 pts*

- **Dado** un anime en mi biblioteca, **cuando** elijo un nuevo estado, **entonces** se guarda y se refleja en todas las vistas sin recargar.
- **Dado** un anime con 12 episodios y 7 vistos, **cuando** lo marco como "Completado", **entonces** el contador pasa a 12 de 12.

### HU-03.03 · Marcar un episodio como visto

**Como** usuario, **quiero** marcar el siguiente episodio como visto con un clic, **para** registrar mi progreso manualmente sin fricción.

*Must · 3 pts*

- **Dado** un anime con 4 de 12 episodios vistos, **cuando** pulso "+1", **entonces** el contador muestra 5 de 12.
- **Dado** un anime finalizado con 11 de 12 vistos, **cuando** pulso "+1", **entonces** el contador muestra 12 de 12 y el estado pasa a "Completado".
- **Dado** un anime en emisión sin total conocido, **cuando** pulso "+1", **entonces** el contador solo se limita por el último episodio emitido.

### HU-03.04 · Corregir el progreso

**Como** usuario, **quiero** fijar directamente el número de episodios vistos o restar uno, **para** corregir errores de registro.

*Should · 2 pts*

- **Dado** un anime con 8 vistos, **cuando** fijo el valor en 5, **entonces** el contador muestra 5 y el estado no cambia.
- **Dado** un valor fuera de rango, **cuando** intento guardarlo, **entonces** veo un error de validación y no se guarda.

### HU-03.05 · Ver mi biblioteca por estado

**Como** usuario, **quiero** ver mi biblioteca agrupada o filtrada por estado, **para** encontrar rápido lo que estoy viendo.

*Must · 3 pts*

- **Dado** mi biblioteca, **cuando** la abro, **entonces** veo pestañas por estado con su recuento y, por cada anime, portada, título y "X de Y".
- **Dado** una pestaña, **cuando** ordeno por título o por última actualización, **entonces** el orden se aplica y se recuerda en mi siguiente visita.

### HU-03.06 · Quitar un anime de mi biblioteca

**Como** usuario, **quiero** quitar un anime de mi biblioteca, **para** mantenerla limpia.

*Should · 1 pt*

- **Dado** un anime en mi biblioteca, **cuando** lo quito y confirmo, **entonces** desaparece de todas las vistas, incluido el calendario personal.

### HU-03.07 · Proyección de catálogo en el Tracking Module *\[Técnica\]*

**Como** equipo, **quiero** que el Tracking Module mantenga una proyección de solo lectura con los datos del catálogo que necesita, **para** no compartir tablas con el Catalog Module.

*Must · 5 pts*

- **Dado** un anime agregado a una biblioteca, **cuando** se crea la entrada, **entonces** el Tracking Module guarda en su esquema el UUID, título, portada y total de episodios obtenidos vía Mediator.
- **Dado** que el catálogo actualiza el total de episodios, **cuando** publica el evento de integración, **entonces** la proyección se actualiza sin consultas cruzadas entre esquemas.

### HU-03.08 · Historial de episodios vistos

**Como** usuario, **quiero** ver cuándo vi cada episodio, **para** recordar mi ritmo de visualización.

*Could · 3 pts*

- **Dado** un anime con progreso, **cuando** abro su historial, **entonces** veo cada episodio con su fecha de registro en mi hora local y su origen (manual o extensión).

## EP-04 · Favoritos y dashboard

Objetivo: que al entrar el usuario vea de inmediato sus favoritos y qué episodio le toca ver, en una sola respuesta agregada por el BFF.

### HU-04.01 · Marcar un anime como favorito

**Como** usuario, **quiero** marcar o desmarcar un anime de mi biblioteca como favorito, **para** destacarlo y acceder rápido a él.

*Must · 2 pts*

- **Dado** un anime en mi biblioteca, **cuando** pulso la estrella, **entonces** queda marcado como favorito sin cambiar su estado ni su progreso.
- **Dado** un anime que no está en mi biblioteca, **cuando** pulso la estrella, **entonces** se agrega en "Plan para ver" y queda marcado como favorito.
- **Dado** mi biblioteca, **cuando** filtro por "Favoritos", **entonces** veo solo los marcados, de cualquier estado.

### HU-04.02 · Dashboard principal

**Como** usuario, **quiero** un dashboard con mis favoritos y una sección "Continuar viendo", **para** retomar mis series sin buscarlas.

*Must · 5 pts*

- **Dado** que inicio sesión, **cuando** abro el dashboard, **entonces** veo mis favoritos destacados y los animes en "Viendo" ordenados por última actividad.
- **Dado** un anime en "Continuar viendo", **cuando** lo miro, **entonces** veo el siguiente episodio a ver y un botón "+1" (HU-03.03).
- **Dado** un usuario nuevo sin biblioteca, **cuando** abre el dashboard, **entonces** ve un estado vacío que lo lleva a buscar animes.

### HU-04.03 · Endpoint agregador del dashboard *\[Técnica\]*

**Como** equipo, **quiero** que el BFF combine en una sola llamada los datos del Catalog Module y del Tracking Module, **para** que la SPA haga una única petición al cargar el dashboard.

*Must · 3 pts*

- **Dado** una petición a `GET /api/v1/dashboard`, **cuando** el BFF la procesa, **entonces** llama en paralelo a los módulos necesarios y devuelve un único JSON definido en el contrato OpenAPI.
- **Dado** que un módulo falla, **cuando** el BFF responde, **entonces** devuelve las secciones disponibles e indica cuál no se pudo cargar, en lugar de un error total.

## EP-05 · Calendario de emisiones

Objetivo: que el usuario sepa cuándo sale el próximo episodio de lo que sigue, en su hora local. El backend calcula y transmite todo en UTC (ISO 8601) y la SPA aplica el desfase horario del cliente, como define el ARD.

### HU-05.01 · Ver el cronograma semanal de simulcasts

**Como** usuario, **quiero** ver un cronograma semanal de los animes en emisión, **para** planificar qué ver cada día.

*Must · 5 pts*

- **Dado** la temporada actual, **cuando** abro el calendario, **entonces** veo 7 columnas (lunes a domingo) con los animes ordenados por hora de emisión.
- **Dado** el calendario, **cuando** navego a la semana siguiente o anterior, **entonces** se cargan las emisiones de esa semana.
- **Dado** un anime del calendario, **cuando** lo abro, **entonces** veo su ficha (HU-02.03) y puedo agregarlo a mi biblioteca.

### HU-05.02 · Filtrar por los animes que sigo

**Como** usuario, **quiero** ver en el calendario solo los animes de mi biblioteca, **para** no perderme mis episodios.

*Must · 3 pts*

- **Dado** el interruptor "Solo los que sigo", **cuando** lo activo, **entonces** solo aparecen animes en estado "Viendo" o "Plan para ver".
- **Dado** el filtro activo, **cuando** vuelvo al calendario otro día, **entonces** se mantiene mi elección.

### HU-05.03 · Horarios ajustados a mi zona horaria

**Como** usuario, **quiero** que días y horas se muestren en mi zona horaria, **para** saber exactamente cuándo puedo ver cada episodio.

*Must · 3 pts*

- **Dado** un episodio que se emite el sábado 00:30 en Japón (UTC+9), **cuando** lo veo con zona America/Bogota (UTC−5), **entonces** aparece el viernes a las 10:30.
- **Dado** una emisión que cruza la medianoche al convertir, **cuando** se renderiza, **entonces** aparece en la columna del día local correcto.
- **Dado** que cambio mi zona horaria en el perfil (HU-01.04), **cuando** vuelvo al calendario, **entonces** todas las horas se recalculan.

### HU-05.04 · Próximo episodio con cuenta regresiva

**Como** usuario, **quiero** ver cuánto falta para el próximo episodio de cada anime que sigo, **para** anticipar los estrenos.

*Should · 3 pts*

- **Dado** un anime en emisión en mi biblioteca, **cuando** abro su ficha o el dashboard, **entonces** veo el número del próximo episodio, su fecha local y una cuenta regresiva.
- **Dado** que la emisión ya ocurrió, **cuando** se actualiza la vista, **entonces** la cuenta regresiva pasa al episodio siguiente.

### HU-05.05 · Cálculo y caché del cronograma *\[Técnica\]*

**Como** equipo, **quiero** que el Calendar Module calcule las emisiones semanales en UTC y las cachee en Redis, **para** servir el calendario rápido sin recalcular en cada petición.

*Must · 5 pts*

- **Dado** los horarios de emisión del catálogo, **cuando** se calcula la semana, **entonces** cada emisión se guarda como instante UTC en ISO 8601.
- **Dado** un cronograma cacheado, **cuando** se refresca la temporada o cambia un horario, **entonces** se invalida la entrada correspondiente en Redis.
- **Dado** que Redis no está disponible, **cuando** se pide el calendario, **entonces** se calcula desde PostgreSQL y se registra la degradación.

### HU-05.06 · Notificación de nuevo episodio

**Como** usuario, **quiero** recibir un aviso cuando salga un episodio nuevo de un anime que sigo, **para** verlo en cuanto esté disponible.

*Could · 5 pts*

- **Dado** un anime en "Viendo", **cuando** llega su hora de emisión, **entonces** recibo una notificación (web push o de la extensión) con el número de episodio.
- **Dado** mis preferencias, **cuando** desactivo las notificaciones de un anime o de todos, **entonces** dejo de recibirlas.

Nota: el PRD menciona las notificaciones en su descripción pero no en el alcance principal; se propone fuera del MVP.

## EP-06 · Extensión de navegador auto-tracker

Objetivo: que el progreso se registre solo cuando el usuario termina un episodio en una plataforma de streaming, sin interrumpir la reproducción. La ruta rápida del BFF responde `202 Accepted` en menos de 200 ms y el núcleo .NET procesa la actualización de forma asíncrona.

### HU-06.01 · Instalar y vincular la extensión

**Como** usuario, **quiero** instalar la extensión y vincularla a mi cuenta en pocos pasos, **para** activar el seguimiento automático.

*Must · 3 pts*

- **Dado** la extensión recién instalada, **cuando** abro su popup, **entonces** me guía a vincular la cuenta mediante el flujo de HU-01.03.
- **Dado** la extensión vinculada, **cuando** abro su popup, **entonces** veo mi usuario, el estado "Seguimiento activo" y el último episodio registrado.

### HU-06.02 · Detectar el final de un episodio

**Como** usuario, **quiero** que la extensión detecte cuando termino un episodio, **para** no tener que marcarlo a mano.

*Must · 8 pts*

- **Dado** un reproductor de video en un sitio soportado, **cuando** se dispara el evento `ended` o la reproducción supera el umbral configurado de la duración, **entonces** la extensión genera un evento de progreso una sola vez por episodio.
- **Dado** que salto hasta el final sin ver el episodio, **cuando** el tiempo real visto es menor al mínimo configurado, **entonces** no se registra el progreso.
- **Dado** un sitio no soportado, **cuando** reproduzco un video, **entonces** la extensión no envía nada.

Nota: candidata a dividirse por plataforma de streaming en refinamiento; el PRD no fija qué sitios se soportan en el MVP.

### HU-06.03 · Identificar el anime y el episodio

**Como** usuario, **quiero** que la extensión reconozca qué anime y qué episodio estoy viendo, **para** que el progreso se registre en la serie correcta.

*Must · 8 pts*

- **Dado** la página de reproducción, **cuando** la extensión lee título y número de episodio, **entonces** los resuelve contra el catálogo a un UUID interno.
- **Dado** una coincidencia ambigua o de baja confianza, **cuando** termina el episodio, **entonces** la extensión me pide confirmar la serie y recuerda mi elección para ese título.
- **Dado** un título ya resuelto antes, **cuando** vuelvo a verlo, **entonces** se usa el mapeo guardado sin nueva búsqueda.

### HU-06.04 · Confirmación no intrusiva con deshacer

**Como** usuario, **quiero** ver un aviso breve cuando se registra un episodio, con opción de deshacer, **para** saber que funcionó y corregir errores.

*Should · 3 pts*

- **Dado** un episodio registrado, **cuando** se confirma, **entonces** aparece un aviso discreto (p. ej. "Episodio 5 registrado") que desaparece solo y no pausa el video.
- **Dado** el aviso visible, **cuando** pulso "Deshacer", **entonces** el progreso vuelve al valor anterior.

### HU-06.05 · Cola sin conexión y reintentos

**Como** usuario, **quiero** que los episodios vistos sin conexión se registren al recuperarla, **para** no perder progreso.

*Should · 5 pts*

- **Dado** que no hay red al terminar un episodio, **cuando** se genera el evento, **entonces** se guarda en una cola local de la extensión.
- **Dado** eventos en cola, **cuando** vuelve la conexión, **entonces** se envían en orden con reintentos y espera exponencial.
- **Dado** un evento reenviado dos veces, **cuando** llega al backend, **entonces** se procesa una sola vez gracias a su clave de idempotencia.

### HU-06.06 · Endpoint de ruta rápida para tracking *\[Técnica\]*

**Como** equipo, **quiero** un endpoint `POST /api/v1/track` en el BFF que valide el token y encole el evento, **para** cumplir el SLA de menos de 200 ms del PRD.

*Must · 5 pts*

- **Dado** un evento válido con token de dispositivo, **cuando** llega al BFF, **entonces** responde `202 Accepted` y el Tracking Module lo procesa de forma asíncrona.
- **Dado** un evento de un episodio igual o anterior al progreso actual, **cuando** se procesa, **entonces** el contador no cambia (progreso = máximo entre actual y recibido).
- **Dado** un token inválido o un cuerpo que no cumple el contrato, **cuando** llega al BFF, **entonces** responde 401 o 400 sin encolar nada.

### HU-06.07 · Pausar el seguimiento por sitio

**Como** usuario, **quiero** pausar el seguimiento automático en general o en un sitio concreto, **para** controlar mi privacidad.

*Could · 2 pts*

- **Dado** el popup, **cuando** desactivo el seguimiento para un sitio, **entonces** la extensión no lee ni envía datos de ese sitio hasta que lo reactive.

## EP-07 · Atributos de calidad transversales

Objetivo: verificar con pruebas automatizadas los requisitos no funcionales del PRD y el ARD (rendimiento, precisión horaria, seguridad) en lugar de darlos por supuestos.

### HU-07.01 · Prueba de carga del endpoint de tracking *\[Técnica\]*

**Como** equipo, **quiero** una prueba de carga automatizada del endpoint de tracking, **para** demostrar que cumple el SLA de menos de 200 ms.

*Should · 5 pts*

- **Dado** el entorno de staging, **cuando** se ejecuta la prueba de carga (p. ej. k6) con la concurrencia objetivo, **entonces** el p95 de `POST /api/v1/track` es menor a 200 ms.
- **Dado** una regresión de rendimiento, **cuando** el p95 supera el umbral, **entonces** el pipeline marca la ejecución como fallida.

Pregunta abierta: la concurrencia objetivo (usuarios simultáneos) no está definida en el PRD.

### HU-07.02 · Fechas en UTC en todos los contratos *\[Técnica\]*

**Como** equipo, **quiero** que todas las fechas se almacenen y transmitan en UTC ISO 8601, **para** evitar errores de zona horaria.

*Must · 2 pts*

- **Dado** los contratos OpenAPI, **cuando** se valida un campo de fecha, **entonces** su formato es `date-time` con sufijo `Z`.
- **Dado** las columnas de fecha en PostgreSQL, **cuando** se revisan las migraciones, **entonces** usan `timestamptz`.

### HU-07.03 · Seguridad básica del perímetro *\[Técnica\]*

**Como** equipo, **quiero** proteger el BFF con CORS restringido, límite de peticiones y cabeceras de seguridad, **para** reducir la superficie de ataque.

*Must · 5 pts*

- **Dado** una petición desde un origen distinto de la SPA o la extensión, **cuando** llega al BFF, **entonces** se rechaza por CORS.
- **Dado** un cliente que supera el límite de peticiones, **cuando** sigue enviando, **entonces** recibe 429.
- **Dado** el núcleo .NET, **cuando** se despliega, **entonces** solo es accesible desde la red interna del BFF.

### HU-07.04 · Pruebas extremo a extremo de flujos clave

**Como** equipo, **quiero** pruebas e2e automatizadas de los flujos principales, **para** detectar regresiones entre SPA, BFF y núcleo.

*Should · 5 pts*

- **Dado** el entorno de pruebas, **cuando** corre la suite (p. ej. Playwright), **entonces** cubre: registro, buscar y agregar un anime, marcar un episodio, ver el dashboard y ver el calendario.
- **Dado** un fallo en la suite, **cuando** termina, **entonces** se adjuntan capturas y trazas al resultado del pipeline.

## Priorización por releases y dependencias

Se proponen cuatro releases incrementales: el MVP (R1) cubre todo el alcance manual del PRD, y la extensión llega en R3 cuando catálogo, seguimiento e identidad ya son estables.

&#91;embedded content: hoja de ruta · 4 releases\]

Cada release entrega valor usable por sí sola; las historias Could quedan fuera del MVP.

Dependencias críticas entre historias:

- HU-00.07 (contratos OpenAPI) precede a cualquier endpoint nuevo.
- HU-02.04 y HU-02.05 (ACL y caché) preceden a HU-03.01 y a HU-05.05.
- HU-03.07 (proyección de catálogo) precede a HU-04.03 (agregador del dashboard).
- HU-01.03 (token de dispositivo) precede a HU-06.01 y HU-06.06.
- HU-06.06 (ruta rápida) precede a la integración de HU-06.02 y HU-06.03.

## Preguntas abiertas

- [ ] ¿Qué plataformas de streaming soportará la extensión en R3?
- [ ] ¿Cuál es la concurrencia objetivo para la prueba de carga (HU-07.01)?
- [ ] ¿Se usará Jikan, AniList o ambas como fuente de catálogo y horarios?
- [ ] ¿Se requiere inicio de sesión con proveedores externos (Google, Discord) además de email?
