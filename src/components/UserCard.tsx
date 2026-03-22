import React, { useRef } from 'react'
import { Camera, CheckCircle2, Clock, RefreshCw } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface SimpleUser {
  id: string
  displayName: string
  employeeId: string
  title: string
  organization: string
  department: string
  email: string
  photo: string | null
}

interface UserCardProps {
  user: SimpleUser
  onPhotoSelect: (user: SimpleUser, file: File) => void
  justUploaded?: boolean
  isLoading?: boolean
}

/** Deterministic hue from a string — used for initials gradient */
function nameToHue(name: string) {
  let h = 0
  for (let i = 0; i < name.length; i++) h = (h * 31 + name.charCodeAt(i)) % 360
  return h
}

export const UserCard: React.FC<UserCardProps> = ({
  user,
  onPhotoSelect,
  justUploaded = false,
  isLoading = false,
}) => {
  const fileRef = useRef<HTMLInputElement>(null)

  const initials = user.displayName
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)

  const hue = nameToHue(user.displayName)

  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0]
    if (f) onPhotoSelect(user, f)
    e.target.value = ''
  }

  return (
    <div
      className={cn(
        "group relative bg-card border-[3px] border-border overflow-hidden transition-brutal",
        "hover:shadow-brutal-hover hover:-translate-y-1",
        justUploaded ? "shadow-brutal-lg animate-fade-up" : "shadow-brutal"
      )}
    >
      {/* New badge */}
      {justUploaded && (
        <div className="absolute top-3 right-3 z-10 animate-slide-in">
          <Badge className="font-display text-xs px-3 py-1 bg-accent text-accent-foreground border-2 border-border shadow-brutal">
            YENİ
          </Badge>
        </div>
      )}

      {/* Avatar area — clickable */}
      <div
        className="relative cursor-pointer overflow-hidden bg-muted"
        style={{ paddingTop: '100%' }}
        onClick={() => fileRef.current?.click()}
      >
        {user.photo ? (
          <img
            src={user.photo}
            alt={user.displayName}
            className="absolute inset-0 w-full h-full object-cover"
          />
        ) : (
          <div
            className="absolute inset-0 flex items-center justify-center"
            style={{
              background: `linear-gradient(135deg, hsl(${hue} 70% 85%) 0%, hsl(${hue + 60} 70% 85%) 100%)`,
            }}
          >
            <div
              className="w-24 h-24 rounded-full flex items-center justify-center border-[3px] border-border shadow-brutal"
              style={{
                background: `linear-gradient(135deg, hsl(${hue} 60% 75%) 0%, hsl(${hue + 60} 60% 75%) 100%)`,
              }}
            >
              <span className="font-display text-4xl text-foreground/80">
                {initials}
              </span>
            </div>
          </div>
        )}

        {/* Hover overlay */}
        <div className="absolute inset-0 bg-primary/90 flex flex-col items-center justify-center gap-2 opacity-0 group-hover:opacity-100 transition-all duration-200">
          <Camera className="w-8 h-8 text-primary-foreground" strokeWidth={2.5} />
          <span className="font-display text-sm tracking-wider text-primary-foreground">
            {user.photo ? 'DEĞİŞTİR' : 'YÜKLE'}
          </span>
        </div>
      </div>

      {/* Info block */}
      <div className="p-4 space-y-3 bg-card">
        {/* Name + ID */}
        <div>
          <h3 className="font-display text-lg leading-tight text-card-foreground truncate">
            {user.displayName}
          </h3>
          <p className="text-primary text-xs font-bold tracking-wider mt-1">
            {user.employeeId}
          </p>
        </div>

        {/* Title + org */}
        <div className="space-y-1">
          <p className="text-sm text-foreground/80 font-medium leading-tight truncate">
            {user.title}
          </p>
          <p className="text-xs text-muted-foreground truncate">
            {user.organization}
          </p>
        </div>

        {/* Status + action */}
        <div className="flex items-center justify-between pt-2 border-t-2 border-border">
          <div className="flex items-center gap-2">
            {user.photo ? (
              <div className="flex items-center gap-1.5 px-2 py-1 bg-success/10 border-2 border-success rounded">
                <CheckCircle2 className="w-3.5 h-3.5 text-success" strokeWidth={2.5} />
                <span className="text-xs font-bold text-success">HAZIR</span>
              </div>
            ) : (
              <div className="flex items-center gap-1.5 px-2 py-1 bg-muted border-2 border-border rounded">
                <Clock className="w-3.5 h-3.5 text-muted-foreground" strokeWidth={2.5} />
                <span className="text-xs font-bold text-muted-foreground">BEKLİYOR</span>
              </div>
            )}
          </div>

          <Button
            size="sm"
            variant={user.photo ? "outline" : "default"}
            onClick={() => fileRef.current?.click()}
            disabled={isLoading}
            className={cn(
              "h-8 px-3 font-display text-xs tracking-wider border-2 transition-brutal",
              !isLoading && "hover:-translate-y-0.5"
            )}
          >
            {isLoading ? (
              <>
                <RefreshCw className="w-3.5 h-3.5 animate-spin" strokeWidth={2.5} />
                <span className="ml-1.5">YÜKLÜYOR</span>
              </>
            ) : user.photo ? (
              'DEĞİŞTİR'
            ) : (
              'YÜKLE'
            )}
          </Button>
        </div>
      </div>

      <input
        ref={fileRef}
        type="file"
        accept=".jpg,.jpeg,image/jpeg"
        className="hidden"
        onChange={handleFile}
      />
    </div>
  )
}
