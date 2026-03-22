import * as React from "react"
import { Slot } from "@radix-ui/react-slot"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap font-bold font-body transition-brutal focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 cursor-pointer border-[3px] shadow-brutal hover:shadow-brutal-hover active:shadow-none active:translate-x-1 active:translate-y-1",
  {
    variants: {
      variant: {
        default:
          "bg-primary text-primary-foreground border-border hover:-translate-y-1",
        destructive:
          "bg-destructive text-destructive-foreground border-border hover:-translate-y-1",
        outline:
          "border-border bg-background hover:bg-accent hover:text-accent-foreground hover:-translate-y-1",
        secondary:
          "bg-secondary text-secondary-foreground border-border hover:-translate-y-1",
        ghost:
          "border-transparent shadow-none hover:bg-accent hover:text-accent-foreground hover:shadow-none",
        link:
          "text-primary underline-offset-4 hover:underline border-transparent shadow-none hover:shadow-none",
      },
      size: {
        default: "h-10 px-5 py-2 text-sm",
        sm: "h-8 px-3 text-xs",
        lg: "h-12 px-7 text-base",
        icon: "h-10 w-10",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
)

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : "button"
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      />
    )
  }
)
Button.displayName = "Button"

export { Button, buttonVariants }
