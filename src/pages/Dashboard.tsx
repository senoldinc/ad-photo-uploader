import React, { useState } from 'react'
import { useUsers } from '@hooks/useUsers'
import { useUploadPhoto } from '@hooks/useUploadPhoto'
import { useToast } from '@context/ToastContext'
import { UserCard } from '@components/UserCard'
import { CropModal } from '@components/CropModal'
import { SearchBar } from '@components/SearchBar'
import { StatsBar } from '@components/StatsBar'
import { ThemeToggle } from '@components/ThemeToggle'

export const Dashboard: React.FC = () => {
  const [search, setSearch] = useState('')
  const [department, setDepartment] = useState('')
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [cropModalOpen, setCropModalOpen] = useState(false)
  const [uploadingUserId, setUploadingUserId] = useState<string | null>(null)
  const [justUploadedUserId, setJustUploadedUserId] = useState<string | null>(null)

  const { data: users = [], isLoading } = useUsers(search, department)
  const { mutate: uploadPhoto, isPending: isUploading } = useUploadPhoto()
  const { addToast } = useToast()

  const handlePhotoSelect = (file: File) => {
    setSelectedFile(file)
    setCropModalOpen(true)
  }

  const handleCropConfirm = async (base64: string) => {
    setCropModalOpen(false)

    if (!selectedFile || !uploadingUserId) return

    addToast({
      type: 'info',
      message: 'Uploading photo...',
    })

    uploadPhoto(
      {
        userId: uploadingUserId,
        imageBase64: base64,
        fileSizeKb: Math.round(base64.length / 1024),
      },
      {
        onSuccess: () => {
          addToast({
            type: 'success',
            message: '✨ Photo uploaded successfully!',
          })
          setJustUploadedUserId(uploadingUserId)
          setTimeout(() => setJustUploadedUserId(null), 3000)
        },
        onError: () => {
          addToast({
            type: 'error',
            message: 'Failed to upload photo. Try again.',
          })
        },
      }
    )

    setSelectedFile(null)
    setUploadingUserId(null)
  }

  const handleUserCardPhotoSelect = (userId: string, file: File) => {
    setUploadingUserId(userId)
    handlePhotoSelect(file)
  }

  const withPhotoCount = users.filter((u) => u.hasPhoto).length
  const withoutPhotoCount = users.length - withPhotoCount

  return (
    <div className="min-h-screen bg-white dark:bg-gray-900 transition-colors">
      {/* Crop Modal */}
      <CropModal
        file={selectedFile}
        isOpen={cropModalOpen}
        onConfirm={handleCropConfirm}
        onCancel={() => {
          setCropModalOpen(false)
          setSelectedFile(null)
          setUploadingUserId(null)
        }}
      />

      {/* Header */}
      <header className="sticky top-0 z-40 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 shadow-sm">
        <div className="container-main py-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-display font-bold text-gray-900 dark:text-white">
                AD Photo Manager
              </h1>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Manage Active Directory user photos
              </p>
            </div>
            <ThemeToggle />
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="container-main py-8">
        {/* Stats Bar */}
        <div className="mb-8">
          <StatsBar
            totalUsers={users.length}
            withPhotoCount={withPhotoCount}
            withoutPhotoCount={withoutPhotoCount}
            uploadedThisSessionCount={justUploadedUserId ? 1 : 0}
          />
        </div>

        {/* Search and Filters */}
        <div className="mb-8 card p-6">
          <SearchBar
            onSearch={(s, d) => {
              setSearch(s)
              setDepartment(d)
            }}
            resultCount={users.length}
          />
        </div>

        {/* User Grid */}
        <div className="grid-users">
          {isLoading ? (
            // Skeleton Loading
            Array.from({ length: 8 }).map((_, i) => (
              <div key={i} className="card overflow-hidden">
                <div className="w-full aspect-square bg-gray-200 dark:bg-gray-700 animate-pulse" />
                <div className="p-4 space-y-3">
                  <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 animate-pulse" />
                  <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2 animate-pulse" />
                  <div className="h-10 bg-gray-200 dark:bg-gray-700 rounded animate-pulse" />
                </div>
              </div>
            ))
          ) : users.length === 0 ? (
            <div className="col-span-full text-center py-12">
              <p className="text-lg text-gray-600 dark:text-gray-400">
                No users found matching your criteria.
              </p>
            </div>
          ) : (
            users.map((user) => (
              <UserCard
                key={user.id}
                user={user}
                onPhotoSelect={(file) => handleUserCardPhotoSelect(user.id, file)}
                justUploaded={justUploadedUserId === user.id}
                isLoading={isUploading && uploadingUserId === user.id}
              />
            ))
          )}
        </div>
      </main>
    </div>
  )
}
