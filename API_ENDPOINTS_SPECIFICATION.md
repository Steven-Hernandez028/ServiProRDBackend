# ServiPro API - Especificación Completa de Endpoints

## 1. Auth Controller (`/api/auth`)

### POST `/api/auth/login`
**Público** | Sin autenticación
```json
{
  "email": "client@example.com",
  "password": "Password123!"
}
```
**Response:** 200 OK
```json
{
  "token": "eyJhbGc...",
  "user": {
    "id": "guid",
    "email": "test@example.com",
    "name": "Juan Pérez",
    "phone": "+1234567890",
    "role": "Client",
    "avatar": null,
    "createdAt": "2026-04-18T00:00:00Z",
    "isActive": true
  }
}
```

---

### POST `/api/auth/register/client`
**Público** | Sin autenticación
```json
{
  "email": "client@example.com",
  "password": "Password123!",
  "name": "Juan Pérez",           // [Required] MaxLength: 100
  "phone": "+1234567890",         // MaxLength: 20 (opcional)
  "city": "Santo Domingo",        // [Required] MaxLength: 100
  "address": "Calle 123 Apto 4"   // MaxLength: 500 (opcional)
}
```
**Response:** 201 Created
```json
{
  "token": "eyJhbGc...",
  "user": { ... }
}
```

**Errores:**
- 400: `{"message": "El email ya esta registrado"}` (email duplicado)
- 400: Campos requeridos faltantes (name, city)

---

### POST `/api/auth/register/provider`
**Público** | Sin autenticación
```json
{
  "email": "provider@example.com",
  "password": "Password123!",
  "name": "Carlos López",              // [Required] MaxLength: 100
  "phone": "+1234567890",              // MaxLength: 20 (opcional)
  "city": "Santo Domingo",             // [Required] MaxLength: 100
  "zone": "Zona Colonial",             // [Required] MaxLength: 100
  "description": "Especialista en...", // [Required] MaxLength: 1000
  "categories": ["electricista", "plomero"],  // [Required] MinLength: 1 (array de strings)
  "hourlyRate": 50.00,                 // (opcional) decimal
  "experience": "10 años",             // MaxLength: 100 (opcional)
  "availability": "Lunes a Viernes"    // MaxLength: 200 (opcional)
}
```
**Response:** 201 Created
```json
{
  "token": "eyJhbGc...",
  "user": { ... }
}
```

**Errores:**
- 400: `{"message": "El email ya esta registrado"}`
- 400: Campos requeridos faltantes (name, city, zone, description, categories)

---

### GET `/api/auth/me`
**Autenticado** | Bearer Token
**Headers:**
```
Authorization: Bearer {token}
```
**Response:** 200 OK
```json
{
  "id": "guid",
  "email": "test@example.com",
  "name": "Juan Pérez",
  "phone": "+1234567890",
  "role": "Client",
  "avatar": null,
  "createdAt": "2026-04-18T00:00:00Z",
  "isActive": true
}
```

**Errores:**
- 401: Sin token o token inválido

---

## 2. Users Controller (`/api/users`) - [Admin Only]

### GET `/api/users`
**[Admin Only]** | Bearer Token
**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "email": "user@example.com",
    "name": "Juan Pérez",
    "phone": "+1234567890",
    "role": "Client",
    "isActive": true,
    "createdAt": "2026-04-18T00:00:00Z"
  }
]
```

---

### GET `/api/users/clients`
**[Admin Only]** | Bearer Token
**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "email": "client@example.com",
    "name": "Juan Pérez",
    "phone": "+1234567890",
    "city": "Santo Domingo",
    "address": "Calle 123",
    "isActive": true,
    "createdAt": "2026-04-18T00:00:00Z"
  }
]
```

---

### POST `/api/users/{id}/toggle-active`
**[Admin Only]** | Bearer Token
**URL Parameter:**
```
{id} = GUID del usuario
```
**Body:** (vacío)
**Response:** 204 No Content

**Errores:**
- 401: Sin autenticación
- 403: No es Admin
- 404: Usuario no existe

---

## 3. Providers Controller (`/api/providers`)

### GET `/api/providers`
**Público** | Sin autenticación
**Query Parameters:**
```
?search=string         // Búsqueda en nombre/bio (opcional)
&categoryId=int        // ID de categoría (opcional)
&city=string           // Filtrar por ciudad (opcional)
&page=1                // Número de página (default: 1)
&pageSize=10           // Resultados por página (default: 10)
```
**Response:** 200 OK
```json
{
  "data": [
    {
      "id": "guid",
      "name": "Carlos López",
      "description": "Especialista en...",
      "city": "Santo Domingo",
      "zone": "Zona Colonial",
      "rating": 4.5,
      "reviewCount": 12,
      "hourlyRate": 50.00,
      "isVerified": true,
      "categories": [
        {
          "id": 1,
          "name": "Electricista",
          "code": "electricista"
        }
      ]
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3
}
```

---

### GET `/api/providers/{id}`
**Público** | Sin autenticación
**URL Parameter:**
```
{id} = GUID del proveedor
```
**Response:** 200 OK
```json
{
  "id": "guid",
  "userId": "guid",
  "email": "provider@example.com",
  "name": "Carlos López",
  "phone": "+1234567890",
  "description": "Especialista en...",
  "city": "Santo Domingo",
  "zone": "Zona Colonial",
  "rating": 4.5,
  "reviewCount": 12,
  "hourlyRate": 50.00,
  "isVerified": true,
  "experience": "10 años",
  "availability": "Lunes a Viernes",
  "categories": [
    {
      "id": 1,
      "name": "Electricista",
      "code": "electricista"
    }
  ],
  "createdAt": "2026-04-18T00:00:00Z",
  "isActive": true
}
```

**Errores:**
- 404: Proveedor no existe

---

### GET `/api/providers/me`
**[Provider Only]** | Bearer Token
**Response:** 200 OK (mismo formato que GET /{id})

**Errores:**
- 401: Sin autenticación
- 403: No es Provider
- 404: Provider no existe

---

### PUT `/api/providers/me`
**[Provider Only]** | Bearer Token
```json
{
  "description": "Actualización...",     // MaxLength: 1000 (opcional)
  "city": "Santo Domingo",               // MaxLength: 100 (opcional)
  "zone": "Nueva Zona",                  // MaxLength: 100 (opcional)
  "hourlyRate": 60.00,                   // (opcional) decimal
  "experience": "15 años",               // MaxLength: 100 (opcional)
  "availability": "Lunes a Domingo",     // MaxLength: 200 (opcional)
  "categoryIds": [1, 2, 3]               // (opcional) array de IDs
}
```
**Response:** 200 OK (mismo formato que GET /{id})

---

### GET `/api/providers/all`
**[Admin Only]** | Bearer Token
**Response:** 200 OK
```json
[
  {
    // Mismo formato que GET /{id}
  }
]
```

---

### POST `/api/providers/{id}/verify`
**[Admin Only]** | Bearer Token
**URL Parameter:**
```
{id} = GUID del proveedor
```
**Body:** (vacío)
**Response:** 204 No Content

**Errores:**
- 404: Proveedor no existe

---

### POST `/api/providers/{id}/toggle-active`
**[Admin Only]** | Bearer Token
**Response:** 204 No Content

---

## 4. Service Requests Controller (`/api/servicerequests`) - [Autenticado]

### POST `/api/servicerequests`
**[Client Only]** | Bearer Token
```json
{
  "categoryId": 1,                   // [Required] ID de categoría
  "title": "Reparación de tuberías", // [Required] MaxLength: 200
  "description": "Las tuberías...",  // [Required]
  "city": "Santo Domingo",           // [Required] MaxLength: 100
  "zone": "Zona Colonial",           // MaxLength: 100 (opcional)
  "address": "Calle Principal 123",  // [Required]
  "budget": 100.00,                  // (opcional) decimal
  "preferredDate": "2026-05-01",     // (opcional) formato: YYYY-MM-DD
  "providerId": "guid"               // (opcional) para asignar directamente
}
```
**Response:** 201 Created
```json
{
  "id": "guid",
  "clientId": "guid",
  "clientName": "Juan Pérez",
  "clientPhone": "+1234567890",
  "providerId": null,
  "providerName": null,
  "category": {
    "id": 1,
    "name": "Plomería",
    "code": "plomeria"
  },
  "title": "Reparación de tuberías",
  "description": "Las tuberías...",
  "city": "Santo Domingo",
  "zone": "Zona Colonial",
  "address": "Calle Principal 123",
  "status": "Pending",
  "budget": 100.00,
  "preferredDate": "2026-05-01",
  "createdAt": "2026-04-18T00:00:00Z",
  "updatedAt": "2026-04-18T00:00:00Z"
}
```

**Errores:**
- 400: Campos requeridos faltantes (categoryId, title, description, city, address)
- 401: Sin autenticación

---

### GET `/api/servicerequests/{id}`
**Autenticado** | Bearer Token
**Response:** 200 OK (mismo formato que POST response)

**Errores:**
- 404: Solicitud no existe

---

### GET `/api/servicerequests/client`
**[Client Only]** | Bearer Token
**Query Parameters:**
```
?categoryId=int
&city=string
&status=Pending|Accepted|InProgress|Completed|Cancelled
&page=1
&pageSize=10
```
**Response:** 200 OK
```json
{
  "data": [ /* array de ServiceRequestDTO */ ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

---

### GET `/api/servicerequests/provider`
**[Provider Only]** | Bearer Token
**Query Parameters:** (igual que /client)
**Response:** 200 OK (mismo formato)

---

### GET `/api/servicerequests/available`
**[Provider Only]** | Bearer Token
Solicitudes sin asignar disponibles para el proveedor
**Query Parameters:** (igual que /client)
**Response:** 200 OK (mismo formato)

---

### GET `/api/servicerequests/all`
**[Admin Only]** | Bearer Token
**Query Parameters:** (igual que /client)
**Response:** 200 OK (mismo formato)

---

### POST `/api/servicerequests/{id}/accept`
**[Provider Only]** | Bearer Token
**Body:** (vacío)
**Response:** 204 No Content

**Errores:**
- 400: No se pudo aceptar (ya aceptada, no disponible)
- 404: Solicitud no existe

---

### PUT `/api/servicerequests/{id}/status`
**Autenticado** | Bearer Token
```json
{
  "status": "Completed"  // [Required] Pending|Accepted|InProgress|Completed|Cancelled
}
```
**Response:** 204 No Content

**Errores:**
- 400: Cambio de estado inválido
- 404: Solicitud no existe

---

## 5. Reviews Controller (`/api/reviews`)

### GET `/api/reviews/provider/{providerId}`
**Público** | Sin autenticación
**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "providerId": "guid",
    "clientId": "guid",
    "clientName": "Juan Pérez",
    "rating": 5,
    "comment": "Excelente trabajo",
    "providerResponse": "Gracias por tu reseña",
    "createdAt": "2026-04-18T00:00:00Z"
  }
]
```

---

### POST `/api/reviews`
**[Client Only]** | Bearer Token
```json
{
  "providerId": "guid",              // [Required] GUID del proveedor
  "serviceRequestId": "guid",        // (opcional) GUID de la solicitud
  "rating": 5,                       // [Required] Range: 1-5
  "comment": "Excelente trabajo"     // MaxLength: 1000 (opcional)
}
```
**Response:** 201 Created
```json
{
  "id": "guid",
  "providerId": "guid",
  "clientId": "guid",
  "clientName": "Juan Pérez",
  "rating": 5,
  "comment": "Excelente trabajo",
  "providerResponse": null,
  "createdAt": "2026-04-18T00:00:00Z"
}
```

**Errores:**
- 400: Rating fuera de rango (1-5)
- 400: Ya existe reseña del cliente para este proveedor
- 401: Sin autenticación

---

### POST `/api/reviews/{id}/response`
**[Provider Only]** | Bearer Token
```json
{
  "response": "Gracias por tu reseña"  // [Required] MaxLength: 1000
}
```
**Response:** 204 No Content

**Errores:**
- 400: Solo el proveedor puede responder a sus reseñas
- 404: Reseña no existe

---

### GET `/api/reviews/all`
**[Admin Only]** | Bearer Token
**Response:** 200 OK
```json
[
  {
    // Mismo formato que GET /provider/{providerId}
  }
]
```

---

## 6. Advertisements Controller (`/api/advertisements`)

### GET `/api/advertisements`
**Público** | Sin autenticación
**Query Parameters:**
```
?position=Homepage|Sidebar|Footer  // (opcional)
```
**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "title": "Promo Especial",
    "description": "Descripción...",
    "imageUrl": "https://example.com/img.jpg",
    "linkUrl": "https://example.com",
    "position": "Homepage",
    "status": "Active",
    "impressions": 100,
    "clicks": 10,
    "startDate": "2026-04-18T00:00:00Z",
    "endDate": "2026-05-18T00:00:00Z",
    "advertiser": "Empresa XYZ",
    "createdAt": "2026-04-18T00:00:00Z"
  }
]
```

---

### GET `/api/advertisements/all`
**[Admin Only]** | Bearer Token
**Response:** 200 OK (mismo formato que GET /)

---

### GET `/api/advertisements/{id}`
**[Admin Only]** | Bearer Token
**Response:** 200 OK (mismo objeto que array anterior)

**Errores:**
- 404: Anuncio no existe

---

### POST `/api/advertisements`
**[Admin Only]** | Bearer Token
```json
{
  "title": "Promo",                          // [Required] MaxLength: 200
  "description": "Descripción...",           // (opcional)
  "imageUrl": "https://example.com/img.jpg", // [Required]
  "linkUrl": "https://example.com",          // [Required]
  "position": "Homepage",                    // [Required] MaxLength: 50
  "startDate": "2026-04-18T00:00:00Z",       // [Required] DateTime
  "endDate": "2026-05-18T00:00:00Z",         // [Required] DateTime
  "advertiser": "Empresa XYZ"                // [Required] MaxLength: 200
}
```
**Response:** 201 Created (mismo objeto que GET)

---

### PUT `/api/advertisements/{id}`
**[Admin Only]** | Bearer Token
```json
{
  "title": "Promo Actualizada",              // MaxLength: 200 (opcional)
  "description": "Desc...",                  // (opcional)
  "imageUrl": "https://...",                 // (opcional)
  "linkUrl": "https://...",                  // (opcional)
  "position": "Sidebar",                     // MaxLength: 50 (opcional)
  "status": "Inactive",                      // MaxLength: 20 (opcional)
  "startDate": "2026-04-18T00:00:00Z",       // (opcional)
  "endDate": "2026-05-18T00:00:00Z",         // (opcional)
  "advertiser": "Empresa ABC"                // MaxLength: 200 (opcional)
}
```
**Response:** 200 OK

---

### DELETE `/api/advertisements/{id}`
**[Admin Only]** | Bearer Token
**Response:** 204 No Content

---

### POST `/api/advertisements/{id}/impression`
**Público** | Sin autenticación
Registra una vista del anuncio
**Body:** (vacío)
**Response:** 204 No Content

---

### POST `/api/advertisements/{id}/click`
**Público** | Sin autenticación
Registra un click del anuncio
**Body:** (vacío)
**Response:** 204 No Content

---

## 7. Messages Controller (`/api/messages`) - [Autenticado]

### GET `/api/messages/conversations`
**Autenticado** | Bearer Token
**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "clientId": "guid",
    "clientName": "Juan Pérez",
    "providerId": "guid",
    "providerName": "Carlos López",
    "serviceRequestId": "guid",
    "serviceRequestTitle": "Reparación de tuberías",
    "lastMessage": "Hola, ¿cuándo puedes?",
    "lastMessageAt": "2026-04-18T10:30:00Z",
    "unreadCount": 3,
    "isActive": true
  }
]
```

---

### GET `/api/messages/conversations/{conversationId}/messages`
**Autenticado** | Bearer Token
**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "conversationId": "guid",
    "senderId": "guid",
    "senderName": "Juan Pérez",
    "senderRole": "Client",
    "content": "Hola, ¿cuándo puedes?",
    "isRead": true,
    "createdAt": "2026-04-18T10:30:00Z"
  }
]
```

---

### POST `/api/messages`
**Autenticado** | Bearer Token
```json
{
  "conversationId": "guid",      // [Required]
  "content": "Hola, ¿cómo estás?" // [Required] MaxLength: 2000
}
```
**Response:** 200 OK
```json
{
  "id": "guid",
  "conversationId": "guid",
  "senderId": "guid",
  "senderName": "Juan Pérez",
  "senderRole": "Client",
  "content": "Hola, ¿cómo estás?",
  "isRead": false,
  "createdAt": "2026-04-18T10:30:00Z"
}
```

**Errores:**
- 400: Conversación no existe

---

### POST `/api/messages/conversations`
**[Client Only]** | Bearer Token
```json
{
  "providerId": "guid",              // [Required]
  "serviceRequestId": "guid",        // (opcional)
  "initialMessage": "Hola, necesito..." // [Required] MaxLength: 2000
}
```
**Response:** 201 Created
```json
{
  "id": "guid",
  "clientId": "guid",
  "clientName": "Juan Pérez",
  "providerId": "guid",
  "providerName": "Carlos López",
  "serviceRequestId": null,
  "serviceRequestTitle": null,
  "lastMessage": "Hola, necesito...",
  "lastMessageAt": "2026-04-18T10:30:00Z",
  "unreadCount": 0,
  "isActive": true
}
```

**Errores:**
- 400: Proveedor no existe

---

### POST `/api/messages/conversations/{conversationId}/read`
**Autenticado** | Bearer Token
Marca todos los mensajes como leídos
**Body:** (vacío)
**Response:** 204 No Content

---

## 8. Social Media Controller (`/api/social-media`)

### GET `/api/social-media`
**Público** | Sin autenticación
**Response:** 200 OK
```json
[
  {
    "id": "guid",
    "platform": "Facebook",
    "url": "https://facebook.com/servipro",
    "label": "Síguenos en Facebook",
    "isActive": true,
    "order": 1,
    "followers": "5K",
    "createdAt": "2026-04-18T00:00:00Z",
    "updatedAt": "2026-04-18T00:00:00Z"
  }
]
```

---

### GET `/api/social-media/all`
**[Admin Only]** | Bearer Token
**Response:** 200 OK (mismo formato que GET /)

---

### GET `/api/social-media/{id}`
**[Admin Only]** | Bearer Token
**Response:** 200 OK (mismo objeto que array anterior)

---

### POST `/api/social-media`
**[Admin Only]** | Bearer Token
```json
{
  "platform": "Facebook",                    // [Required] MaxLength: 50
  "url": "https://facebook.com/servipro",   // [Required]
  "label": "Síguenos en Facebook",          // [Required] MaxLength: 200
  "isActive": true,                         // default: true
  "order": 1,                               // default: 0
  "followers": "5K"                         // MaxLength: 20 (opcional)
}
```
**Response:** 201 Created (mismo formato que GET)

---

### PUT `/api/social-media/{id}`
**[Admin Only]** | Bearer Token
```json
{
  "url": "https://facebook.com/servipro",   // (opcional)
  "label": "Síguenos en Facebook",          // MaxLength: 200 (opcional)
  "isActive": false,                        // (opcional)
  "order": 2,                               // (opcional)
  "followers": "10K"                        // MaxLength: 20 (opcional)
}
```
**Response:** 200 OK

---

### DELETE `/api/social-media/{id}`
**[Admin Only]** | Bearer Token
**Response:** 204 No Content

---

## Códigos de Estado HTTP

| Código | Significado |
|--------|-------------|
| 200 | OK - Operación exitosa (GET, PUT) |
| 201 | Created - Recurso creado (POST) |
| 204 | No Content - Operación exitosa sin contenido (DELETE, POST sin respuesta) |
| 400 | Bad Request - Datos inválidos, campos faltantes, validación fallida |
| 401 | Unauthorized - Sin autenticación o token inválido |
| 403 | Forbidden - Sin autorización (rol insuficiente) |
| 404 | Not Found - Recurso no existe |
| 500 | Internal Server Error - Error del servidor |

---

## Enums y Valores Constantes

### RequestStatus
```csharp
Pending      // Solicitado
Accepted     // Aceptado por proveedor
InProgress   // En progreso
Completed    // Completado
Cancelled    // Cancelado
```

### UserRole
```csharp
Client       // Cliente
Provider     // Proveedor
Admin        // Administrador
```

### Posiciones de Anuncios
```
Homepage
Sidebar
Footer
```

---

## Headers Requeridos

### Autenticación
```
Authorization: Bearer {jwt_token}
```

### Content-Type
```
Content-Type: application/json
```

---

## Variables de Ambiente (Postman)

```
{{base_url}}          = https://localhost:7184
{{auth_token}}        = JWT token obtenido del login
{{user_id}}           = GUID del usuario
{{provider_id}}       = GUID del proveedor
{{request_id}}        = GUID de la solicitud
{{review_id}}         = GUID de la reseña
{{ad_id}}             = GUID del anuncio
{{conversation_id}}   = GUID de la conversación
{{social_id}}         = GUID del link social
```

---

## Ejemplos de Errores

### Validación Fallida (400)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "City": ["The City field is required."],
    "Name": ["The Name field is required."]
  },
  "traceId": "00-xxx-xxx-00"
}
```

### No Autorizado (401)
```json
{
  "message": "Unauthorized"
}
```

### Recurso No Encontrado (404)
```json
{
  "message": "Not Found"
}
```

---

## Notas Importantes

1. **Todos los GUID** deben ser válidos en formato UUID v4
2. **DateTime** en formato ISO 8601: `YYYY-MM-DDTHH:mm:ssZ`
3. **DateOnly** en formato: `YYYY-MM-DD`
4. **Paginación**: Siempre retorna `totalCount`, `page`, `pageSize`, `totalPages`
5. **Soft Delete**: Registros borrados no aparecen en búsquedas
6. **JWT Token**: Expira después del tiempo configurado en `appsettings.json`
7. **CORS**: Solo permite orígenes configurados en Program.cs
8. **MaxLength**: Validación en DB y API
9. **Required**: [Required] significa que el campo no puede ser null/empty
10. **Roles**: Solo Admin puede acceder a endpoints marcados con [Admin Only]
