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
  borderClass: string
}

export const StatsBar: React.FC<StatsBarProps> = ({
  totalUsers,
  withPhotoCount,
  withoutPhotoCount,
  uploadedThisSessionCount,
}) => {
  const stats: StatItem[] = [
    {
      label: 'TOPLAM',
      value: totalUsers,
      icon: Users,
      colorClass: 'text-foreground',
      bgClass: 'bg-secondary',
      borderClass: 'border-secondary',
    },
    {
      label: 'HAZIR',
      value: withPhotoCount,
      icon: CheckCircle2,
      colorClass: 'text-success',
      bgClass: 'bg-success',
      borderClass: 'border-success',
    },
    {
      label: 'BEKLİYOR',
      value: withoutPhotoCount,
      icon: Clock,
      colorClass: 'text-muted-foreground',
      bgClass: 'bg-muted',
      borderClass: 'border-muted',
    },
    {
      label: 'OTURUM',
      value: uploadedThisSessionCount,
      icon: Upload,
      colorClass: 'text-primary',
      bgClass: 'bg-primary',
      borderClass: 'border-primary',
    },
  ]

  return (
    <div className="border-b-[3px] border-border bg-background">
      <div className="grid grid-cols-2 lg:grid-cols-4">
        {stats.map((stat, index) => {
          const Icon = stat.icon
          return (
            <div
              key={stat.label}
              className={cn(
                "px-6 py-6 flex items-center gap-4 relative",
                index < stats.length - 1 && "border-r-[3px] border-border"
              )}
            >
              <div className={cn(
                "p-3 border-[3px] flex-shrink-0 shadow-brutal transition-brutal hover:shadow-brutal-hover hover:-translate-y-0.5",
                stat.borderClass
              )}>
                <Icon className={cn("w-6 h-6", stat.colorClass)} strokeWidth={2.5} />
              </div>
              <div>
                <div className={cn("font-display text-4xl leading-none tracking-tight", stat.colorClass)}>
                  {stat.value}
                </div>
                <div className="text-xs text-muted-foreground mt-1.5 font-bold tracking-wider uppercase">
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
