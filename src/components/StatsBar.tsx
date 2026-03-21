import React from 'react'
import { Users, CheckCircle2, Clock, Upload } from 'lucide-react'
import { cn } from '@/lib/utils'

interface StatsBarProps {
  totalUsers: number
  withPhotoCount: number
  withoutPhotoCount: number
  uploadedThisSessionCount: number
}

interface StatItem {
  label: string
  value: number
  icon: React.ElementType
  colorClass: string
  bgClass: string
}

export const StatsBar: React.FC<StatsBarProps> = ({
  totalUsers,
  withPhotoCount,
  withoutPhotoCount,
  uploadedThisSessionCount,
}) => {
  const stats: StatItem[] = [
    {
      label: 'Toplam Kullanıcı',
      value: totalUsers,
      icon: Users,
      colorClass: 'text-foreground',
      bgClass: 'bg-secondary',
    },
    {
      label: 'Fotoğraflı',
      value: withPhotoCount,
      icon: CheckCircle2,
      colorClass: 'text-emerald-500 dark:text-emerald-400',
      bgClass: 'bg-emerald-50 dark:bg-emerald-950/30',
    },
    {
      label: 'Fotoğrafsız',
      value: withoutPhotoCount,
      icon: Clock,
      colorClass: 'text-muted-foreground',
      bgClass: 'bg-secondary',
    },
    {
      label: 'Bu Oturumda',
      value: uploadedThisSessionCount,
      icon: Upload,
      colorClass: 'text-primary',
      bgClass: 'bg-primary/5',
    },
  ]

  return (
    <div className="border-b border-border bg-background">
      <div className="grid grid-cols-2 lg:grid-cols-4">
        {stats.map((stat, index) => {
          const Icon = stat.icon
          return (
            <div
              key={stat.label}
              className={cn(
                "px-6 py-5 flex items-center gap-4",
                index < stats.length - 1 && "border-r border-border"
              )}
            >
              <div className={cn("p-2 rounded-md flex-shrink-0", stat.bgClass)}>
                <Icon className={cn("w-4 h-4", stat.colorClass)} />
              </div>
              <div>
                <div className={cn("font-display text-3xl leading-none tracking-wide", stat.colorClass)}>
                  {stat.value}
                </div>
                <div className="text-xs text-muted-foreground mt-1 font-medium tracking-wide uppercase">
                  {stat.label}
                </div>
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
