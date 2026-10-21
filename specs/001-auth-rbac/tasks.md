# Tasks: 001-auth-rbac (Autenticación y RBAC)

**Input**: Design documents from `specs/001-auth-rbac/`  
**Prerequisites**: [spec.md](file:///c:/SIGEP/specs/001-auth-rbac/spec.md), [plan.md](file:///c:/SIGEP/specs/001-auth-rbac/plan.md)  
**Status**: Ready for Implementation  

---

## Phase 1: Setup & Estructura Clean Architecture Backend

- [x] **T001**: Inicializar `package.json` en `backend/` con scripts (`dev`, `build`, `start`, `test`) y dependencias (`express`, `pg`, `dotenv`, `cors`, `helmet`, `bcryptjs`, `jsonwebtoken`, `zod`).
- [x] **T002**: Configurar `tsconfig.json` en `backend/` para TypeScript y ES Modules.
- [x] **T003**: [P] Crear el árbol de directorios de Clean Architecture en `backend/src/` (`domain`, `application`, `infrastructure`, `presentation`).
- [x] **T004**: [P] Crear archivo de configuración `.env.example` y módulo `backend/src/config/env.ts` para carga y tipado de variables de entorno.

---

## Phase 2: Foundational (Infraestructura de Base de Datos y Seguridad)

- [x] **T005**: Crear cliente de conexión de PostgreSQL con pool `backend/src/infrastructure/database/db.ts`.
- [x] **T006**: Crear script DDL SQL de migraciones `backend/src/infrastructure/database/migrations/001_roles_usuarios.sql`.
- [x] **T007**: Crear script de seed `001_seed_roles_admin.sql` con roles iniciales (`Jefe`, `Encargado`) y usuario administrador inicial con hash `bcrypt`.
- [x] **T008**: Implementar adaptador de hash `backend/src/infrastructure/security/BcryptPasswordHasher.ts` que implemente `IPasswordHasher`.
- [x] **T009**: Implementar adaptador de tokens `backend/src/infrastructure/security/JwtTokenService.ts` que implemente `ITokenService`.

---

## Phase 3: User Story 1 (P1) — Inicio de Sesión Seguro & Emisión de Token

- [x] **T010** [US1]: Crear entidades de dominio `backend/src/domain/entities/Usuario.ts` y `Rol.ts`.
- [x] **T011** [US1]: Definir interfaces de repositorio `backend/src/domain/repositories/IUsuarioRepository.ts` y `IRolRepository.ts`.
- [x] **T012** [US1]: Implementar `PostgresUsuarioRepository.ts` con métodos `buscarPorUsername(username)`, `buscarPorId(id)`.
- [x] **T013** [US1]: Implementar caso de uso `LoginUseCase.ts` en `backend/src/application/use-cases/auth/`.
- [x] **T014** [US1]: Implementar caso de uso `ObtenerUsuarioActualUseCase.ts` para validar token y devolver perfil.
- [x] **T015** [US1]: Crear controlador `AuthController.ts` y router `backend/src/presentation/http/routes/authRoutes.ts`.
- [x] **T016** [US1]: Configurar servidor principal `backend/src/index.ts` levantando Express y montando rutas `/api/auth`.

---

## Phase 4: User Story 2 (P1) — Middlewares de Control de Acceso (RBAC)

- [x] **T017** [US2]: Implementar middleware de autenticación `authMiddleware.ts` para verificar `Bearer <token>` y adjuntar `req.user`.
- [x] **T018** [US2]: Implementar middleware de autorización `requireRole.ts` que valide si el rol del usuario coincide con los roles permitidos (ej. `['Jefe']`).
- [x] **T019** [US2]: Crear middleware de manejo centralizado de errores `errorHandler.ts` para respuestas HTTP consistentes.

---

## Phase 5: User Story 3 (P2) — Administración de Usuarios por el Jefe

- [x] **T020** [US3]: Implementar en `PostgresUsuarioRepository.ts` los métodos `crear(usuario)`, `listarTodos()`, `actualizarEstado(id, estado)`.
- [x] **T021** [US3]: Implementar casos de uso `CrearUsuarioUseCase.ts`, `ListarUsuariosUseCase.ts` y `CambiarEstadoUsuarioUseCase.ts`.
- [x] **T022** [US3]: Crear controlador `UsuarioController.ts` y router protegido `/api/usuarios` (restringido a rol `Jefe`).

---

## Phase 6: Frontend Integration (Preservando la UI existente)

- [x] **T023**: Crear servicio API `frontend/app/src/services/api.js` configurado con baseURL e interceptor para inyectar token JWT.
- [x] **T024**: Crear contexto de sesión `frontend/app/src/context/AuthContext.jsx` con persistencia en `localStorage`.
- [x] **T025**: Crear componente de inicio de sesión visual `LoginModal.jsx` (o vista de login integrada a la cabecera) manteniendo el diseño municipal existente.
- [x] **T026**: Conectar en [`App.jsx`](file:///c:/SIGEP/frontend/app/src/App.jsx) el estado de sesión para alternar entre usuario anónimo y autenticado, mostrando el nombre y rol del operador logueado.

---

## Phase 7: Verification & Testing

- [x] **T027**: Pruebas unitarias de `LoginUseCase` validando: credenciales correctas, clave errónea y usuario inactivo.
- [ ] **T028**: Pruebas de integración con Supertest sobre endpoints `/api/auth/login` y verificación de rechazo 403 con rol Encargado en rutas del Jefe.
