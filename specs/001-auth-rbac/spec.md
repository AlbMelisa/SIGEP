# Feature Specification: Autenticación, Usuarios y Control de Acceso (RBAC)

**Feature Branch**: `001-auth-rbac`  
**Created**: 2026-10-16  
**Status**: Ready for Planning  
**Requerimientos Asociados**: RF01, RF23, RNF01, RNF03, RNF04  

---

## 1. User Scenarios & Testing *(Prioritized User Journeys)*

### User Story 1 - Inicio de Sesión Seguro (Priority: P1)
Como **Jefe** o **Encargado**, quiero autenticarme en la plataforma mediante mi nombre de usuario y contraseña para acceder a las funciones operativas de SIGEP según mis privilegios.

* **Why this priority**: Es la puerta de entrada indispensable del sistema. Sin autenticación y emisión de tokens JWT, no es posible garantizar la seguridad ni la trazabilidad exigida por la normativa.
* **Independent Test**: Se envía una solicitud POST con credenciales válidas y se recibe un token JWT firmado junto con el perfil del usuario (rol y datos básicos). Si las credenciales son inválidas, se rechaza con error 401 sin exponer detalles internos.

**Acceptance Scenarios**:
1. **Given** un usuario activo registrado con username `jefe_personal` y contraseña válida,  
   **When** ingresa sus credenciales en el formulario de inicio de sesión,  
   **Then** el sistema responde con HTTP 200, un token JWT válido y redirige al Dashboard general.
2. **Given** un usuario ingresa una contraseña incorrecta o un usuario inexistente,  
   **When** envía el formulario,  
   **Then** el sistema responde con HTTP 401 y muestra un mensaje claro: *"Usuario o contraseña incorrectos"*.
3. **Given** un usuario que ha sido dado de baja o desactivado por el Jefe,  
   **When** intenta iniciar sesión con credenciales correctas,  
   **Then** el sistema responde con HTTP 403 indicando *"Usuario inactivo. Comuníquese con la administración"*.

---

### User Story 2 - Control de Acceso Basado en Roles (RBAC) (Priority: P1)
Como sistema, quiero asegurar que cada endpoint y vista esté protegido según el rol del usuario autenticado (`Jefe` vs. `Encargado`) para evitar accesos no autorizados.

* **Why this priority**: Evita que un Encargado pueda confirmar asignaciones finales o modificar usuarios, reservando esas facultades críticas exclusivamente para el Jefe.
* **Independent Test**: Enviar una petición a un endpoint restringido (ej. crear usuario o confirmar sábado) usando un token con rol `Encargado` y comprobar la respuesta 403 Forbidden.

**Acceptance Scenarios**:
1. **Given** un usuario con rol `Encargado`,  
   **When** intenta invocar un endpoint exclusivo del Jefe (como `POST /api/usuarios` o `POST /api/planificaciones/:id/confirmar`),  
   **Then** el middleware de autorización devuelve HTTP 403 Forbidden.
2. **Given** un usuario con rol `Jefe`,  
   **When** invoca cualquier endpoint administrativo u operativo,  
   **Then** la solicitud es procesada exitosamente.

---

### User Story 3 - Administración de Usuarios por el Jefe (Priority: P2)
Como **Jefe**, quiero poder listar, crear, editar y desactivar usuarios del sistema, asignándoles rol y vinculándolos opcionalmente a un legajo de empleado.

* **Why this priority**: Permite delegar tareas a encargados de asistencia y mantener actualizado el personal habilitado para operar el software.
* **Independent Test**: El Jefe crea un nuevo usuario con rol `Encargado`, se verifica en base de datos que la contraseña esté hasheada con `bcrypt` y que el nuevo usuario pueda autenticarse.

**Acceptance Scenarios**:
1. **Given** el Jefe autenticado en la plataforma,  
   **When** completa el formulario de nuevo usuario con username, password, rol y empleado opcional,  
   **Then** se crea el registro con la contraseña cifrada (salt $\ge 10$) y devuelve HTTP 201 Created.
2. **Given** un intento de registro con un `username` ya existente,  
   **When** se envía el formulario,  
   **Then** el sistema retorna HTTP 409 Conflict indicando *"El nombre de usuario ya se encuentra en uso"*.

---

## 2. Requerimientos Funcionales y No Funcionales

* **RF01:** Inicio de sesión con usuario y contraseña, restringiendo según rol.
* **RF23:** Administración de usuarios (crear, editar, desactivar, asignar rol, vincular empleado).
* **RNF03:** Criptografía segura (JWT con expiración, contraseñas con bcrypt).
* **RNF04:** Protección de datos personales (Ley N.º 25.326).

---

## 3. Modelo de Dominio y Datos (Tablas Clave)

```sql
-- Tabla: roles
CREATE TABLE roles (
    id_rol SERIAL PRIMARY KEY,
    nombre VARCHAR(50) UNIQUE NOT NULL, -- 'Jefe', 'Encargado'
    descripcion VARCHAR(255)
);

-- Tabla: usuarios
CREATE TABLE usuarios (
    id_usuario SERIAL PRIMARY KEY,
    username VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    id_rol INT NOT NULL REFERENCES roles(id_rol),
    id_empleado INT UNIQUE REFERENCES empleados(id_empleado) ON DELETE SET NULL,
    telegram_chat_id BIGINT UNIQUE, -- Para vinculación con el bot
    estado VARCHAR(20) DEFAULT 'ACTIVO', -- 'ACTIVO', 'INACTIVO'
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ultimo_acceso TIMESTAMP
);
```

---

## 4. Contrato de API (Endpoints Propuestos)

| Método | Endpoint | Rol Requerido | Descripción |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Público | Autentica y entrega Access Token JWT. |
| `GET` | `/api/auth/me` | Autenticado | Devuelve los datos del usuario en sesión. |
| `POST` | `/api/auth/logout` | Autenticado | Invalida la sesión actual en cliente. |
| `GET` | `/api/usuarios` | Jefe | Lista todos los usuarios registrados. |
| `POST` | `/api/usuarios` | Jefe | Registra un nuevo operador (Encargado/Jefe). |
| `PUT` | `/api/usuarios/:id` | Jefe | Modifica datos o rol de un usuario. |
| `PATCH` | `/api/usuarios/:id/estado` | Jefe | Activa o desactiva un usuario. |
