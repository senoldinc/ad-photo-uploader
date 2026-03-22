import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const badgeVariants = cva(
  "inline-flex items-center border-[2px] px-3 py-1 text-xs font-bold font-body uppercase tracking-wide transition-brutal focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2",
  {
    variants: {
      variant: {
        default:
          "border-border bg-primary text-primary-foreground shadow-brutal",
        secondary:
          "border-border bg-secondary text-secondary-foreground shadow-brutal",
        destructive:
          "border-border bg-destructive text-destructive-foreground shadow-brutal",
        outline:
          "border-border bg-background text-foreground shadow-brutal",
        success:
          "border-border bg-success text-success-foreground shadow-brutal",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  }
)

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return (
    <div className={cn(badgeVariants({ variant }), className)} {...props} />
  )
}

export { Badge, badgeVariants }
