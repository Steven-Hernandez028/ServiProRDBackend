# Changelog — ServiPro API

## [1.1.0] - 2026-04-20

### Security
- **Cookie-based auth** — `AuthController` now sets `HttpOnly; SameSite=Strict` cookie on login/register instead of returning token in response body
- JWT middleware reads token from `auth_token` cookie via `JwtBearerEvents.OnMessageReceived`
- `Secure` flag enabled automatically when request is HTTPS (off in HTTP dev)
- Added `POST /api/auth/logout` — clears `auth_token` cookie

### Changed
- `POST /api/auth/login` — returns `UserDTO` (no token in body)
- `POST /api/auth/register/client` — returns `UserDTO` (no token in body)
- `POST /api/auth/register/provider` — returns `UserDTO` (no token in body)
- `LoginRequest` — removed unused `Role` field; added `[MinLength(6)]` to `Password`

## [1.0.0] - 2026-04-20

### Added
- Initial API: Auth, Providers, ServiceRequests, Messages, Reviews, Advertisements, SocialMedia, Users
- JWT Bearer authentication with role-based authorization
- PostgreSQL via Entity Framework Core
