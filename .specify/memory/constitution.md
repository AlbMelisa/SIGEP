# SIGEP Constitution — Sistema de Gestión, Asignación y Asistencia de Personal
**Entidad:** Municipalidad de Simoca  
**Marco:** Práctica Supervisada (UTN - FRT)  
**Versión:** 1.0.0 | **Ratificado:** 2026-10-16 | **Última Modificación:** 2026-10-16

---

## Principios Fundamentales

### I. Clean Architecture & Separación Estricta de Responsabilidades (SoC)
* La lógica de negocio pura (**Dominio**) y los casos de uso (**Aplicación**) residen en capas independientes y agnósticas de frameworks, librerías de interfaz, bases de datos o servicios externos.
* **Regla de Dependencia:** Las dependencias del código solo pueden apuntar hacia adentro:
  - `Presentation` y `Infrastructure` $\rightarrow$ `Application` $\rightarrow$ `Domain`.
* La persistencia (PostgreSQL), la mensajería (Telegram Bot API) y el modelo de IA (Google Gemini API) se conectan mediante interfaces (Puertos y Adaptadores).

### II. Integridad Operativa y Reglas de Dominio Inquebrantables
* **Régimen Histórico:** Toda asignación a turno Noche (19:00 a 07:00) impone obligatoriamente un descanso de 48 horas (los dos días hábiles posteriores). El sistema bloquea automáticamente cualquier asignación que viole este descanso.
* **Equidad en Sábados:** Las propuestas automáticas de sábados deben distribuir el personal activo en mitades, alternando estrictamente con quienes descansaron el sábado previo y evitando repetir el turno anterior (Mañana vs. Siesta/Intermedio). En dotaciones impares ($N$), se alterna $\lceil N/2 \rceil$ y $\lfloor N/2 \rfloor$ entre sábados consecutivos.
* **Eventos con Cupos Aislados:** Los turnos de eventos son propios del evento y no modifican el turno habitual de lunes a viernes del empleado.

### III. Seguridad, Trazabilidad y Protección de Datos (Ley N.º 25.326)
* Las contraseñas se almacenan únicamente mediante hash criptográfico (`bcrypt` con salt $\ge 10$).
* La autenticación opera sobre tokens `JWT` con expiración y control de acceso basado en roles (`Jefe` y `Encargado`).
* **Trazabilidad y Auditoría:** Toda confirmación de planificación o evento debe registrar el usuario responsable (`id_usuario_confirmacion`) y marca temporal (`fecha_confirmacion`). No se borran datos históricos; se aplica baja lógica.
* Las consultas mediante el Chatbot de Telegram deben autenticar al remitente y validar sus permisos antes de consultar datos a través del backend.

### IV. Preservación del Diseño de Frontend y Accesibilidad
* Se respeta la estructura visual y de interacción preexistente en [`frontend/app`](file:///c:/SIGEP/frontend/app) (pantallas de Inicio, Personal, Asistencia, Planificación de sábados, Asignación a eventos).
* La interfaz prioriza la claridad para usuarios con poca experiencia digital (validaciones en línea, mensajes comprensibles y flujos simples).

### V. Test-Driven Verification (Pruebas Automatizadas)
* Los algoritmos críticos de cálculo (Rotación de sábados y Scoring de asignación a eventos) deben contar con una suite de pruebas unitarias exhaustivas con cobertura de casos límite (dotaciones impares, personal con licencias, descansos de 48 hs).

---

## Gobernanza y Flujo de Trabajo
* Ninguna feature se codifica sin antes haber pasado por las fases de Spec Kit:
  1. `speckit-specify`: Especificación de requerimientos y casos de uso.
  2. `speckit-clarify`: Resolución de ambigüedades.
  3. `speckit-plan`: Diseño técnico y contratos de API.
  4. `speckit-tasks`: Desglose en tareas atómicas.
  5. `speckit-analyze`: Verificación de consistencia y casos de prueba.
  6. `speckit-implement`: Ejecución guiada de código y testing.
