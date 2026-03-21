import axios, { AxiosInstance, AxiosError } from 'axios'
import { useAuth } from 'react-oidc-context'
import { ADUser, UploadPhotoPayload, UploadPhotoResponse } from '@types'

let apiInstance: AxiosInstance | null = null
let authContextRef: ReturnType<typeof useAuth> | null = null

export const setAuthContext = (auth: ReturnType<typeof useAuth>) => {
  authContextRef = auth
}

export const getApiClient = (): AxiosInstance => {
  if (!apiInstance) {
    apiInstance = axios.create({
      baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    // Request interceptor - Add auth token
    apiInstance.interceptors.request.use(
      (config) => {
        if (authContextRef?.user?.access_token) {
          config.headers.Authorization = `Bearer ${authContextRef.user.access_token}`
        }
        return config
      },
      (error) => Promise.reject(error)
    )

    // Response interceptor - Handle errors
    apiInstance.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        if (error.response?.status === 401) {
          // Unauthorized - trigger login redirect
          if (authContextRef?.signinRedirect) {
            authContextRef.signinRedirect()
          }
        }

        if (error.response?.status === 413) {
          // Payload too large
          error.message = 'File too large (max 5MB)'
        }

        return Promise.reject(error)
      }
    )
  }

  return apiInstance
}

// API Methods
export const apiClient = {
  // Get users list
  getUsers: async (search?: string, department?: string): Promise<ADUser[]> => {
    const client = getApiClient()
    const params = new URLSearchParams()
    if (search) params.append('search', search)
    if (department) params.append('dept', department)

    const response = await client.get<ADUser[]>(
      `/users?${params.toString()}`
    )
    return response.data
  },

  // Get single user
  getUser: async (userId: string): Promise<ADUser> => {
    const client = getApiClient()
    const response = await client.get<ADUser>(`/users/${userId}`)
    return response.data
  },

  // Upload photo
  uploadPhoto: async (userId: string, payload: UploadPhotoPayload): Promise<UploadPhotoResponse> => {
    const client = getApiClient()
    const response = await client.post<UploadPhotoResponse>(
      `/users/${userId}/photo`,
      { imageBase64: payload.imageBase64 }
    )
    return response.data
  },

  // Delete photo
  deletePhoto: async (userId: string): Promise<{ success: boolean }> => {
    const client = getApiClient()
    const response = await client.delete(`/users/${userId}/photo`)
    return response.data
  },
}
