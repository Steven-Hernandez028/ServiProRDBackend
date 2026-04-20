# ServiPro API - Endpoints Testing Guide

## Resumen de Endpoints

Total: **33 endpoints** distribuidos en 7 controladores.

---

## 1. Auth Controller (`/api/auth`)

### Endpoints Públicos

| Método | Endpoint | Descripción | Estado |
|--------|----------|-------------|--------|
| POST | `/api/auth/login` | Login con email/password | ✅ |
| POST | `/api/auth/register/client` | Registro de clientes | ✅ |
| POST | `/api/auth/register/provider` | Registro de proveedores | ✅ |
| GET | `/api/auth/me` | Obtener usuario actual [Autenticado] | ✅ |

**Variables necesarias:**
- `email`: string
- `password`: string
- `firstName`, `lastName`: string (para registro)
- `phone`, `category`, `bio`: string (solo para providers)

---

## 2. Users Controller (`/api/users`) - [Admin only]

| Método | Endpoint | Descripción | Estado |
|--------|----------|-------------|--------|
| GET | `/api/users` | Obtener todos usuarios | ✅ |
| GET | `/api/users/clients` | Obtener todos clientes | ✅ |
| POST | `/api/users/{id}/toggle-active` | Activar/desactivar usuario | ✅ |

---

## 3. Providers Controller (`/api/providers`)

| Método | Endpoint | Descripción | Autenticación | Estado |
|--------|----------|-------------|----------------|--------|
| GET | `/api/providers` | Buscar proveedores | Pública | ✅ |
| GET | `/api/providers/{id}` | Obtener proveedor por ID | Pública | ✅ |
| GET | `/api/providers/me` | Mi perfil | Provider | ✅ |
| PUT | `/api/providers/me` | Actualizar mi perfil | Provider | ✅ |
| GET | `/api/providers/all` | Todos proveedores | Admin | ✅ |
| POST | `/api/providers/{id}/verify` | Verificar proveedor | Admin | ✅ |
| POST | `/api/providers/{id}/toggle-active` | Activar/desactivar | Admin | ✅ |

**Query params para búsqueda:**
- `search`: string (nombre/bio)
- `category`: string
- `minRating`: decimal
- `page`: int
- `pageSize`: int

---

## 4. Service Requests Controller (`/api/servicerequests`) - [Autenticado]

| Método | Endpoint | Descripción | Rol | Estado |
|--------|----------|-------------|-----|--------|
| POST | `/api/servicerequests` | Crear solicitud | Client | ✅ |
| GET | `/api/servicerequests/{id}` | Obtener solicitud | Cualquiera | ✅ |
| GET | `/api/servicerequests/client` | Mis solicitudes | Client | ✅ |
| GET | `/api/servicerequests/provider` | Solicitudes asignadas | Provider | ✅ |
| GET | `/api/servicerequests/available` | Disponibles | Provider | ✅ |
| GET | `/api/servicerequests/all` | Todas (admin) | Admin | ✅ |
| POST | `/api/servicerequests/{id}/accept` | Aceptar solicitud | Provider | ✅ |
| PUT | `/api/servicerequests/{id}/status` | Actualizar estado | Propietario/Provider | ✅ |

**TODO:** Implementar DELETE con soft delete

**Estados disponibles:**
- `Pending`
- `Accepted`
- `InProgress`
- `Completed`
- `Cancelled`

---

## 5. Reviews Controller (`/api/reviews`)

| Método | Endpoint | Descripción | Autenticación | Estado |
|--------|----------|-------------|----------------|--------|
| GET | `/api/reviews/provider/{providerId}` | Reseñas proveedor | Pública | ✅ |
| POST | `/api/reviews` | Crear reseña | Client | ✅ |
| POST | `/api/reviews/{id}/response` | Respuesta proveedor | Provider | ✅ |
| GET | `/api/reviews/all` | Todas reseñas | Admin | ✅ |

**Campos para crear reseña:**
- `providerId`: guid
- `serviceRequestId`: guid
- `rating`: int (1-5)
- `comment`: string

---

## 6. Advertisements Controller (`/api/advertisements`)

| Método | Endpoint | Descripción | Autenticación | Estado |
|--------|----------|-------------|----------------|--------|
| GET | `/api/advertisements` | Anuncios activos | Pública | ✅ |
| GET | `/api/advertisements/all` | Todos anuncios | Admin | ✅ |
| GET | `/api/advertisements/{id}` | Obtener anuncio | Admin | ✅ |
| POST | `/api/advertisements` | Crear anuncio | Admin | ✅ |
| PUT | `/api/advertisements/{id}` | Actualizar anuncio | Admin | ✅ |
| DELETE | `/api/advertisements/{id}` | Eliminar anuncio | Admin | ✅ |
| POST | `/api/advertisements/{id}/impression` | Registrar impresión | Pública | ✅ |
| POST | `/api/advertisements/{id}/click` | Registrar click | Pública | ✅ |

---

## 7. Messages Controller (`/api/messages`) - [Autenticado]

| Método | Endpoint | Descripción | Rol | Estado |
|--------|----------|-------------|-----|--------|
| GET | `/api/messages/conversations` | Mis conversaciones | Cualquiera | ✅ |
| GET | `/api/messages/conversations/{conversationId}/messages` | Mensajes | Cualquiera | ✅ |
| POST | `/api/messages` | Enviar mensaje | Cualquiera | ✅ |
| POST | `/api/messages/conversations` | Iniciar conversación | Client | ✅ |
| POST | `/api/messages/conversations/{id}/read` | Marcar como leído | Cualquiera | ✅ |

---

## 8. Social Media Controller (`/api/social-media`)

| Método | Endpoint | Descripción | Autenticación | Estado |
|--------|----------|-------------|----------------|--------|
| GET | `/api/social-media` | Links activos | Pública | ✅ |
| GET | `/api/social-media/all` | Todos links | Admin | ✅ |
| GET | `/api/social-media/{id}` | Obtener link | Admin | ✅ |
| POST | `/api/social-media` | Crear link | Admin | ✅ |
| PUT | `/api/social-media/{id}` | Actualizar link | Admin | ✅ |
| DELETE | `/api/social-media/{id}` | Eliminar link | Admin | ✅ |

---

## Cómo Usar la Colección Postman

### 1. Importar la Colección
- Abrir Postman
- Click en "Import"
- Seleccionar `ServiPro.API.postman_collection.json`

### 2. Configurar Variables

En la pestaña "Variables" establece:
- `base_url`: `https://localhost:7184` (o tu URL)
- `auth_token`: Obtener haciendo login primero

### 3. Flow de Testing Recomendado

#### Fase 1: Auth
1. POST `/api/auth/register/client` - Crear cliente de prueba
2. POST `/api/auth/register/provider` - Crear proveedor de prueba
3. POST `/api/auth/login` - Login y guardar token en `{{auth_token}}`
4. GET `/api/auth/me` - Verificar usuario actual

#### Fase 2: Users (Admin)
1. GET `/api/users` - Obtener todos
2. GET `/api/users/clients` - Obtener clientes
3. POST `/api/users/{id}/toggle-active` - Desactivar usuario

#### Fase 3: Providers
1. GET `/api/providers` - Búsqueda pública
2. GET `/api/providers/{id}` - Detalles
3. GET `/api/providers/me` - Mi perfil (con token provider)
4. PUT `/api/providers/me` - Actualizar (con token provider)

#### Fase 4: Service Requests
1. POST `/api/servicerequests` - Crear (con token client)
2. GET `/api/servicerequests/{id}` - Ver detalles
3. GET `/api/servicerequests/available` - Ver disponibles (token provider)
4. POST `/api/servicerequests/{id}/accept` - Aceptar (token provider)
5. PUT `/api/servicerequests/{id}/status` - Cambiar estado

#### Fase 5: Reviews
1. POST `/api/reviews` - Crear reseña (token client)
2. GET `/api/reviews/provider/{id}` - Ver reseñas
3. POST `/api/reviews/{id}/response` - Responder (token provider)

#### Fase 6: Advertisements
1. GET `/api/advertisements` - Ver activos (público)
2. POST `/api/advertisements/impression` - Registrar vista
3. POST `/api/advertisements/click` - Registrar click

#### Fase 7: Messages
1. POST `/api/messages/conversations` - Iniciar (token client)
2. POST `/api/messages` - Enviar mensaje
3. GET `/api/messages/conversations` - Ver conversaciones

#### Fase 8: Social Media
1. GET `/api/social-media` - Ver links activos (público)

---

## Casos de Error Esperados

### 401 Unauthorized
- Endpoint requiere autenticación y no se proporciona token
- Token inválido o expirado

### 403 Forbidden
- Rol insuficiente (ej: Client intentando Admin)

### 404 Not Found
- ID no existe

### 400 Bad Request
- Datos inválidos (email duplicado, campos requeridos faltantes)

---

## Testing Checklist

- [ ] Todos endpoints retornan el status correcto
- [ ] Autenticación funciona (login/register)
- [ ] Authorization por roles funciona (Admin, Client, Provider)
- [ ] Validación de datos (emails duplicados, campos requeridos)
- [ ] Paginación en búsquedas funciona
- [ ] Relaciones entre entidades se mantienen
- [ ] Soft deletes funcionan (Service Requests)
- [ ] JWT token expira correctamente
- [ ] CORS funciona con frontend

---

## Base URL

**Development:** `https://localhost:7184`

Asegúrate de tener el certificado SSL aceptado o usar `http://localhost:5000` si está habilitado.

---

## Notas Importantes

1. **Autenticación:** Usa Bearer token en header `Authorization: Bearer {token}`
2. **CORS:** Solo permite `http://localhost:3000` y `https://localhost:3000`
3. **Database:** Usa PostgreSQL (configurado en appsettings)
4. **JWT:** Validación de Issuer, Audience y Lifetime
5. **Soft Deletes:** Reviews, Ads, etc. usan soft delete (no retorna deleted items)
