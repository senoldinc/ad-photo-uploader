/**
 * AD User Entity
 */
export interface ADUser {
  id: string
  displayName: string
  employeeId: string // E.g., "SİC-00123"
  title: string
  department: string
  organization: string
  email: string
  thumbnailPhotoUrl: string | null
  hasPhoto: boolean
}

/**
 * Photo Upload Payload
 */
export interface UploadPhotoPayload {
  userId: string
  imageBase64: string // "data:image/jpeg;base64,..."
  fileSizeKb: number
}

/**
 * Photo Upload Response
 */
export interface UploadPhotoResponse {
  success: boolean
  message: string
  thumbnailPhotoUrl: string
}

/**
 * Image Crop State
 */
export interface CropState {
  position: { x: number; y: number }
  scale: number // 0.5 - 3.0
  quality: number // 0.3 - 1.0
}

/**
 * API Error Response
 */
export interface ApiErrorResponse {
  status: number
  message: string
  code?: string
}

/**
 * Toast Notification
 */
export interface ToastNotification {
  id: string
  type: 'success' | 'error' | 'info' | 'warning'
  message: string
  duration?: number
}

/**
 * Theme Type
 */
export type Theme = 'light' | 'dark'

/**
 * User List Query Options
 */
export interface UserListQueryOptions {
  search?: string
  department?: string
  limit?: number
  offset?: number
}

/**
 * Paginated Response
 */
export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  limit: number
}
