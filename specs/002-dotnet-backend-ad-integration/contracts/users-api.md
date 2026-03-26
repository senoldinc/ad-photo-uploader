# API Contracts: User Management Endpoints

**Version**: 1.0.0
**Base URL**: `/api/users`
**Authentication**: Required (Bearer token)

---

## GET /api/users

List users with pagination, search, and filtering.

### Request

```http
GET /api/users?search=ahmet&organization=Bilgi&page=1&pageSize=20 HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| search | string | No | - | Search in displayName, employeeId, department |
| organization | string | No | - | Filter by organization name |
| department | string | No | - | Filter by department name |
| hasPhoto | boolean | No | - | Filter by photo status |
| page | integer | No | 1 | Page number (1-indexed) |
| pageSize | integer | No | 20 | Items per page (max: 100) |

### Response

**Success (200 OK):**
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "displayName": "Ahmet Yılmaz",
      "employeeId": "SİC-00001",
      "title": "Yazılım Geliştirici",
      "organization": "Bilgi Teknolojileri",
      "department": "Yazılım Geliştirme",
      "email": "ahmet.yilmaz@example.com",
      "hasPhoto": true,
      "photoUpdatedAt": "2026-03-20T10:30:00Z",
      "lastSyncedAt": "2026-03-26T08:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 156,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

**Error (401 Unauthorized):**
```json
{
  "error": "UNAUTHORIZED",
  "message": "Kimlik doğrulama gerekli",
  "details": null
}
```

**Error (400 Bad Request):**
```json
{
  "error": "INVALID_PAGE_SIZE",
  "message": "Sayfa boyutu 1 ile 100 arasında olmalıdır",
  "details": "Requested pageSize: 150"
}
```

---

## GET /api/users/{id}

Get user details by ID.

### Request

```http
GET /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | UUID | User ID |

### Response

**Success (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "adObjectId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "displayName": "Ahmet Yılmaz",
  "employeeId": "SİC-00001",
  "title": "Yazılım Geliştirici",
  "organization": "Bilgi Teknolojileri",
  "department": "Yazılım Geliştirme",
  "email": "ahmet.yilmaz@example.com",
  "hasPhoto": true,
  "photoUpdatedAt": "2026-03-20T10:30:00Z",
  "lastSyncedAt": "2026-03-26T08:00:00Z",
  "createdAt": "2025-01-15T09:00:00Z",
  "updatedAt": "2026-03-26T08:00:00Z"
}
```

**Error (404 Not Found):**
```json
{
  "error": "USER_NOT_FOUND",
  "message": "Kullanıcı bulunamadı",
  "details": "User ID: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

## POST /api/users/sync

Trigger manual AD synchronization (admin only).

### Request

```http
POST /api/users/sync HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "fullSync": true
}
```

**Body Parameters:**
| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| fullSync | boolean | No | false | If true, sync all users; if false, incremental sync |

### Response

**Success (202 Accepted):**
```json
{
  "syncId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": "Running",
  "startedAt": "2026-03-26T14:30:00Z",
  "message": "Senkronizasyon başlatıldı"
}
```

**Error (403 Forbidden):**
```json
{
  "error": "INSUFFICIENT_PERMISSIONS",
  "message": "Bu işlem için yeterli yetkiniz yok",
  "details": "Admin role required"
}
```

**Error (409 Conflict):**
```json
{
  "error": "SYNC_IN_PROGRESS",
  "message": "Bir senkronizasyon zaten devam ediyor",
  "details": "Sync ID: 7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

---

## GET /api/users/sync/status

Get current sync status.

### Request

```http
GET /api/users/sync/status HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response

**Success (200 OK):**
```json
{
  "currentSync": {
    "syncId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "status": "Running",
    "startedAt": "2026-03-26T14:30:00Z",
    "usersProcessed": 45,
    "triggerType": "Manual",
    "triggeredBy": "admin@example.com"
  },
  "lastSync": {
    "syncId": "6b8d5568-6314-39cd-833a-d06eb0e79bd6",
    "status": "Completed",
    "startedAt": "2026-03-26T08:00:00Z",
    "completedAt": "2026-03-26T08:03:15Z",
    "usersProcessed": 156,
    "usersAdded": 2,
    "usersUpdated": 8,
    "usersDeleted": 0,
    "triggerType": "Scheduled"
  }
}
```

---

## Error Codes

| Code | HTTP Status | Description (Turkish) |
|------|-------------|----------------------|
| UNAUTHORIZED | 401 | Kimlik doğrulama gerekli |
| INSUFFICIENT_PERMISSIONS | 403 | Yetersiz yetki |
| USER_NOT_FOUND | 404 | Kullanıcı bulunamadı |
| INVALID_PAGE_SIZE | 400 | Geçersiz sayfa boyutu |
| INVALID_SEARCH_QUERY | 400 | Geçersiz arama sorgusu |
| SYNC_IN_PROGRESS | 409 | Senkronizasyon devam ediyor |
| AD_CONNECTION_ERROR | 500 | Active Directory bağlantı hatası |
| DATABASE_ERROR | 500 | Veritabanı hatası |
