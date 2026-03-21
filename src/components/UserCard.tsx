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
        "group relative bg-card border rounded-lg overflow-hidden transition-all duration-200",
        "hover:shadow-md hover:-translate-y-0.5",
        justUploaded ? "border-primary shadow-sm shadow-primary/20" : "border-border"
      )}
    >
      {/* New badge */}
      {justUploaded && (
        <div className="absolute top-3 right-3 z-10">
          <Badge className="font-display tracking-widest text-[10px] px-2 py-0.5 shadow-sm">
            YENİ
          </Badge>
        </div>
      )}

      {/* Avatar area — clickable, fills top of card */}
      <div
        className="relative cursor-pointer overflow-hidden"
        style={{ paddingTop: '75%' }} /* 4:3 aspect ratio container */
        onClick={() => fileRef.current?.click()}
      >
        {user.photo ? (
          /* Uploaded photo — fills container */
          <img
            src={user.photo}
            alt={user.displayName}
            className="absolute inset-0 w-full h-full object-cover"
          />
        ) : (
          /* No photo — gradient with initials */
          <div
            className="absolute inset-0 flex items-center justify-center"
            style={{
              background: `linear-gradient(135deg,
                hsl(${hue} 30% 92%) 0%,
                hsl(3.7 30% 92%) 100%)`,
            }}
          >
            {/* Big initials circle */}
            <div
              className="w-20 h-20 rounded-full flex items-center justify-center shadow-sm"
              style={{
                background: `linear-gradient(135deg,
                  hsl(${hue} 20% 85%) 0%,
                  hsl(3.7 20% 85%) 100%)`,
              }}
            >
              <span className="font-display text-3xl tracking-widest text-foreground/50">
                {initials}
              </span>
            </div>
          </div>
        )}

        {/* Hover overlay */}
        <div className="absolute inset-0 bg-black/40 flex flex-col items-center justify-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
          <Camera className="w-6 h-6 text-white drop-shadow" />
          <span className="font-display text-xs tracking-[0.2em] text-white/90 drop-shadow">
            {user.photo ? 'DEĞİŞTİR' : 'YÜKLE'}
          </span>
        </div>
      </div>

      {/* Info block */}
      <div className="px-4 pt-3 pb-4 space-y-3">

        {/* Name + ID row */}
        <div>
          <h3 className="font-display text-[1.05rem] tracking-wide leading-snug text-card-foreground truncate">
            {user.displayName}
          </h3>
          <p className="text-primary text-[11px] font-semibold tracking-[0.08em] mt-0.5">
            {user.employeeId}
          </p>
        </div>

        {/* Title + org — compact two-liner */}
        <div className="flex flex-col gap-0.5">
          <p className="text-[12px] text-foreground/70 leading-tight truncate">{user.title}</p>
          <p className="text-[11px] text-muted-foreground truncate">{user.organization}</p>
        </div>

        {/* Status + action — horizontal */}
        <div className="flex items-center justify-between pt-1 border-t border-border">
          <div className="flex items-center gap-1.5">
            {user.photo
              ? <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 shrink-0" />
              : <Clock className="w-3.5 h-3.5 text-muted-foreground/40 shrink-0" />
            }
            <span className={cn(
              "text-[11px] font-medium",
              user.photo ? "text-emerald-500 dark:text-emerald-400" : "text-muted-foreground/40"
            )}>
              {user.photo ? 'Hazır' : 'Bekleniyor'}
            </span>
          </div>

          <Button
            size="sm"
            variant={user.photo ? "outline" : "default"}
            onClick={() => fileRef.current?.click()}
            disabled={isLoading}
            className="h-7 px-3 text-[10px] font-display tracking-[0.12em]"
          >
            {isLoading
              ? <><RefreshCw className="w-3 h-3 animate-spin" /> Yükleniyor</>
              : user.photo ? 'DEĞİŞTİR' : 'YÜKLE'
            }
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
