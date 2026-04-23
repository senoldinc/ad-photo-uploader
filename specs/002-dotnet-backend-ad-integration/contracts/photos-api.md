# API Contracts: Photo Management Endpoints

**Version**: 1.0.0
**Base URL**: `/api/users/{userId}/photo`
**Authentication**: Required (Bearer token)

---

## POST /api/users/{userId}/photo

Upload user photo to Active Directory.

### Request

```http
POST /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/photo HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="photo"; filename="photo.jpg"
Content-Type: image/jpeg

[Binary JPEG data]
------WebKitFormBoundary
Content-Disposition: form-data; name="quality"

85
------WebKitFormBoundary--
```

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| userId | UUID | User ID |

**Form Data:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| photo | file | Yes | JPEG image file (max 500KB) |
| quality | integer | No | JPEG quality 30-100 (default: 85) |

**Constraints:**
- Format: JPEG/JPG only
- Max source size: 500 KB
- Output: 300×300px circular crop
- Max output size: 100 KB (enforced)

### Response

**Success (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "photoUploaded": true,
  "photoSize": 87654,
  "dimensions": {
    "width": 300,
    "height": 300
  },
  "quality": 85,
  "uploadedAt": "2026-03-26T15:45:00Z",
  "message": "Fotoğraf başarıyla yüklendi"
}
```

**Error (400 Bad Request - Invalid Format):**
```json
{
  "error": "INVALID_PHOTO_FORMAT",
  "message": "Sadece JPEG formatı desteklenmektedir",
  "details": "Received content type: image/png"
}
```

**Error (400 Bad Request - Source Too Large):**
```json
{
  "error": "PHOTO_TOO_LARGE",
  "message": "Fotoğraf boyutu 500 KB'dan küçük olmalıdır",
  "details": "Received size: 756 KB"
}
```

**Error (400 Bad Request - Output Too Large):**
```json
{
  "error": "OUTPUT_SIZE_EXCEEDED",
  "message": "İşlenmiş fotoğraf 100 KB limitini aşıyor. Lütfen kaliteyi düşürün.",
  "details": {
    "outputSize": 125000,
    "maxSize": 102400,
    "currentQuality": 95,
    "suggestedQuality": 75
  }
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

**Error (403 Forbidden):**
```json
{
  "error": "INSUFFICIENT_PERMISSIONS",
  "message": "Bu kullanıcının fotoğrafını yükleme yetkiniz yok",
  "details": "Users can only upload their own photos unless they have admin role"
}
```

**Error (500 Internal Server Error):**
```json
{
  "error": "AD_UPLOAD_FAILED",
  "message": "Active Directory'ye yükleme başarısız oldu",
  "details": "LDAP error: Insufficient access rights"
}
```

---

## GET /api/users/{userId}/photo

Retrieve user photo from Active Directory.

### Request

```http
GET /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/photo HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| userId | UUID | User ID |

**Query Parameters:**
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| thumbnail | boolean | No | false | Return thumbnail (96×96) instead of full size |

### Response

**Success (200 OK):**
```http
HTTP/1.1 200 OK
Content-Type: image/jpeg
Content-Length: 87654
Cache-Control: public, max-age=3600
ETag: "a1b2c3d4e5f6"

[Binary JPEG data]
```

**Success (200 OK - Base64 JSON):**
If `Accept: application/json` header is present:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "photoData": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD...",
  "size": 87654,
  "contentType": "image/jpeg",
  "lastModified": "2026-03-26T15:45:00Z"
}
```

**Error (404 Not Found - No Photo):**
```json
{
  "error": "PHOTO_NOT_FOUND",
  "message": "Kullanıcının fotoğrafı bulunamadı",
  "details": "User has no photo in Active Directory"
}
```

**Error (404 Not Found - User):**
```json
{
  "error": "USER_NOT_FOUND",
  "message": "Kullanıcı bulunamadı",
  "details": "User ID: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

## DELETE /api/users/{userId}/photo

Remove user photo from Active Directory.

### Request

```http
DELETE /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/photo HTTP/1.1
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| userId | UUID | User ID |

### Response

**Success (204 No Content):**
```
(Empty response body)
```

**Error (404 Not Found):**
```json
{
  "error": "USER_NOT_FOUND",
  "message": "Kullanıcı bulunamadı",
  "details": "User ID: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Error (403 Forbidden):**
```json
{
  "error": "INSUFFICIENT_PERMISSIONS",
  "message": "Bu kullanıcının fotoğrafını silme yetkiniz yok",
  "details": "Users can only delete their own photos unless they have admin role"
}
```

**Error (500 Internal Server Error):**
```json
{
  "error": "AD_DELETE_FAILED",
  "message": "Active Directory'den silme başarısız oldu",
  "details": "LDAP error: Insufficient access rights"
}
```

---

## Image Processing Details

### Circular Crop Algorithm

1. **Load source image** (JPEG, max 500KB)
2. **Resize to square** maintaining aspect ratio (crop to center)
3. **Scale to 300×300px**
4. **Apply circular mask** (transparent corners)
5. **Encode as JPEG** with specified quality (30-100%)
6. **Validate output size** (must be ≤100KB)
7. **Upload to AD** thumbnailPhoto attribute

### Quality Adjustment

If output exceeds 100KB:
- Automatically reduce quality in 5% increments
- Minimum quality: 30%
- Return error if still exceeds limit at minimum quality

### Caching Strategy

- Photos cached in-memory for 1 hour
- ETag support for conditional requests
- Cache invalidated on upload/delete

---

## Error Codes

| Code | HTTP Status | Description (Turkish) |
|------|-------------|----------------------|
| INVALID_PHOTO_FORMAT | 400 | Geçersiz fotoğraf formatı |
| PHOTO_TOO_LARGE | 400 | Fotoğraf çok büyük |
| OUTPUT_SIZE_EXCEEDED | 400 | İşlenmiş fotoğraf limiti aştı |
| INVALID_QUALITY | 400 | Geçersiz kalite değeri |
| PHOTO_NOT_FOUND | 404 | Fotoğraf bulunamadı |
| USER_NOT_FOUND | 404 | Kullanıcı bulunamadı |
| INSUFFICIENT_PERMISSIONS | 403 | Yetersiz yetki |
| AD_UPLOAD_FAILED | 500 | AD yükleme hatası |
| AD_DELETE_FAILED | 500 | AD silme hatası |
| IMAGE_PROCESSING_ERROR | 500 | Görüntü işleme hatası |
