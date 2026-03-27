# API Contracts: Authentication Endpoints

**Version**: 1.0.0
**Base URL**: `/api/auth`

---

## POST /api/auth/login

Initiates OIDC authentication flow.

### Request

```http
POST /api/auth/login HTTP/1.1
Content-Type: application/json

{
  "returnUrl": "/dashboard"
}
```

**Body Parameters:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| returnUrl | string | No | URL to redirect after successful login |

### Response

**Success (200 OK):**
```json
{
  "redirectUrl": "https://login.microsoftonline.com/...",
  "state": "random-state-token"
}
```

**Error (400 Bad Request):**
```json
{
  "error": "INVALID_RETURN_URL",
  "message": "Geçersiz yönlendirme URL'si",
  "details": null
}
```

---

## POST /api/auth/callback

Handles OIDC callback after authentication.

### Request

```http
POST /api/auth/callback HTTP/1.1
Content-Type: application/json

{
  "code": "authorization-code",
  "state": "state-token"
}
```

**Body Parameters:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| code | string | Yes | Authorization code from OIDC provider |
| state | string | Yes | State token for CSRF protection |

### Response

**Success (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-here",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "displayName": "Ahmet Yılmaz",
    "email": "ahmet.yilmaz@example.com",
    "organization": "Bilgi Teknolojileri"
  }
}
```

**Error (401 Unauthorized):**
```json
{
  "error": "UNAUTHORIZED_ORGANIZATION",
  "message": "Bu organizasyondan kullanıcılar sisteme erişemez",
  "details": "User organization: 'Muhasebe' is not in allowed list"
}
```

**Error (400 Bad Request):**
```json
{
  "error": "INVALID_AUTH_CODE",
  "message": "Geçersiz yetkilendirme kodu",
  "details": null
}
```

---

## POST /api/auth/refresh

Refreshes access token using refresh token.

### Request

```http
POST /api/auth/refresh HTTP/1.1
Content-Type: application/json

{
  "refreshToken": "refresh-token-here"
}
```

**Body Parameters:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| refreshToken | string | Yes | Valid refresh token |

### Response

**Success (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new-refresh-token",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error (401 Unauthorized):**
```json
{
  "error": "INVALID_REFRESH_TOKEN",
  "message": "Geçersiz veya süresi dolmuş yenileme token'ı",
  "details": null
}
```

---

## POST /api/auth/logout

Logs out user and invalidates tokens.

### Request

```http
POST /api/auth/logout HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Headers:**
| Header | Value | Required |
|--------|-------|----------|
| Authorization | Bearer {token} | Yes |

### Response

**Success (204 No Content):**
```
(Empty response body)
```

**Error (401 Unauthorized):**
```json
{
  "error": "INVALID_TOKEN",
  "message": "Geçersiz veya süresi dolmuş token",
  "details": null
}
```

---

## Error Codes

| Code | HTTP Status | Description (Turkish) |
|------|-------------|----------------------|
| INVALID_RETURN_URL | 400 | Geçersiz yönlendirme URL'si |
| INVALID_AUTH_CODE | 400 | Geçersiz yetkilendirme kodu |
| INVALID_STATE | 400 | Geçersiz state parametresi |
| UNAUTHORIZED_ORGANIZATION | 401 | Yetkisiz organizasyon |
| INVALID_TOKEN | 401 | Geçersiz veya süresi dolmuş token |
| INVALID_REFRESH_TOKEN | 401 | Geçersiz yenileme token'ı |
| AUTH_SERVICE_ERROR | 500 | Kimlik doğrulama servisi hatası |
