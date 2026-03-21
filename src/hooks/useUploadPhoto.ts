import { useMutation } from '@tanstack/react-query'
import { UploadPhotoResponse } from '@types'

interface UploadPhotoParams {
  userId: string
  imageBase64: string
  fileSizeKb: number
}

export const useUploadPhoto = () => {
  return useMutation({
    mutationFn: async (params: UploadPhotoParams): Promise<UploadPhotoResponse> => {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1500))

      // Mock success response
      return {
        success: true,
        message: 'Photo uploaded successfully',
        thumbnailPhotoUrl: params.imageBase64,
      }
    },
  })
}
