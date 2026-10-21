# Implementation Plan: 001-auth-rbac (Autenticación y RBAC)

**Branch**: `001-auth-rbac` | **Date**: 2026-10-16 | **Spec**: [spec.md](file:///c:/SIGEP/specs/001-auth-rbac/spec.md)

---

## 1. Summary
Implementación de la capa transversal de seguridad, autenticación y control de acceso basado en roles (RBAC) para SIGEP. Provee inicio de sesión con JWT y contraseñas hasheadas con bcrypt, administración de usuarios por el Jefe, asignación de roles (`Jefe` y `Encargado`), middlewares de autorización y su conexión con la interfaz React de frontend.

---

## 2. Technical Context

* **Language/Runtime**: Node.js v22.x LTS, TypeScript / ES Modules.
* **Backend Framework**: Express.js con tipado estricto y validaciones con Zod.
* **Database**: PostgreSQL 15+ (`pg` pool con transacciones y migraciones DDL SQL).
* **Security & Auth**: `bcryptjs` (salt 10), `jsonwebtoken` (HMAC SHA-256), `cors`, `helmet`.
* **Testing**: Vitest / Jest para pruebas unitarias de casos de uso y Supertest para integración HTTP.
* **Target Platforms**: Servidor Linux / Windows; frontend Web SPA accesible en Chrome/Firefox/Edge.
* **Constraints**: Cumplimiento de la Ley N.º 25.326 de Protección de Datos Personales. Tiempos de respuesta de autenticación $< 300\text{ ms}$.

---

## 3. Constitution Check
* **Clean Architecture & SoC:** ✅ Dominio (`Usuario`, `Rol`) y Casos de Uso (`LoginUseCase`, etc.) sin dependencias de Express ni SQL.
* **Dependency Rule:** ✅ `presentation` e `infrastructure` dependen de `application` y `domain`.
* **Seguridad (Ley 25.326):** ✅ Sin contraseñas en texto plano, tokens JWT con expiración definida.
* **Preservación Frontend:** ✅ Mantiene el layout existente de [`frontend/app`](file:///c:/SIGEP/frontend/app) agregando el estado de sesión y modal/vista de login.

---

## 4. Architecture & Concrete Layout

### Backend (`c:\SIGEP\backend\src\`)
```text
backend/
├── package.json
├── tsconfig.json
├── .env.example
├── src/
│   ├── index.ts                           # Entrada del servidor HTTP Express
│   ├── config/                            # Variables de entorno y constantes
│   │   └── env.ts
│   ├── domain/                            # Capa de Dominio (Puro)
│   │   ├── entities/
│   │   │   ├── Usuario.ts
│   │   │   └── Rol.ts
│   │   └── repositories/
│   │       ├── IUsuarioRepository.ts
│   │       └── IRolRepository.ts
│   ├── application/                       # Casos de Uso
│   │   ├── dtos/
│   │   │   ├── LoginDto.ts
│   │   │   └── UsuarioDto.ts
│   │   ├── ports/
│   │   │   ├── IPasswordHasher.ts
│   │   │   └── ITokenService.ts
│   │   └── use-cases/
│   │       ├── LoginUseCase.ts
│   │       ├── ObtenerUsuarioActualUseCase.ts
│   │       ├── CrearUsuarioUseCase.ts
│   │       ├── ListarUsuariosUseCase.ts
│   │       └── CambiarEstadoUsuarioUseCase.ts
│   ├── infrastructure/                    # Implementaciones concretas
│   │   ├── database/
│   │   │   ├── db.ts                      # Conexión pg Pool
│   │   │   ├── migrations/                # 001_create_roles_usuarios.sql
│   │   │   ├── seeds/                     # 001_seed_roles_admin.sql
│   │   │   └── repositories/
│   │   │       ├── PostgresUsuarioRepository.ts
│   │   │       └── PostgresRolRepository.ts
│   │   └── security/
│   │       ├── BcryptPasswordHasher.ts
│   │       └── JwtTokenService.ts
│   └── presentation/                      # Interfaz HTTP
│       └── http/
│           ├── controllers/
│           │   ├── AuthController.ts
│           │   └── UsuarioController.ts
│           ├── middlewares/
│           │   ├── authMiddleware.ts
│           │   ├── requireRole.ts
│           │   └── errorHandler.ts
│           └── routes/
│               ├── authRoutes.ts
│               ├── usuarioRoutes.ts
│               └── index.ts
```

### Frontend (`c:\SIGEP\frontend\app\src\`)
```text
frontend/app/src/
├── services/
│   └── api.js                   # Cliente HTTP centralizado (Axios o fetch con Auth Header)
├── context/
│   └── AuthContext.jsx          # Estado de sesión (usuario, rol, token, login, logout)
├── components/
│   └── LoginModal.jsx           # Formulario modal de login estilizado según la UI de SIGEP
└── App.jsx                      # Conexión con AuthContext y control de visualización según rol
```

---

## 5. API Contracts

### `POST /api/auth/login`
* **Request Body:**
  ```json
  {
    "username": "jefe_personal",
    "password": "Password123!"
  }
  ```
* **Success Response (200 OK):**
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "usuario": {
      "id": 1,
      "username": "jefe_personal",
      "rol": "Jefe",
      "id_empleado": null
    }
  }
  ```
* **Error Response (401 Unauthorized):**
  ```json
  { "error": "Credenciales inválidas" }
  ```

### `GET /api/auth/me`
* **Header:** `Authorization: Bearer <token>`
* **Success Response (200 OK):** Datos del usuario autenticado.

### `POST /api/usuarios` (Solo rol 'Jefe')
* **Header:** `Authorization: Bearer <token>`
* **Request Body:**
  ```json
  {
    "username": "encargado1",
    "password": "ClaveSegura2026!",
    "id_rol": 2,
    "id_empleado": 4
  }
  ```
* **Success Response (201 Created):** Objeto usuario creado (sin hash de contraseña).

