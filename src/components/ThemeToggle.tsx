import React from 'react'
import { Sun, Moon } from 'lucide-react'
import { useTheme } from '@/context/ThemeContext'
import { Button } from '@/components/ui/button'

export const ThemeToggle: React.FC = () => {
  const { theme, toggleTheme } = useTheme()

  return (
    <Button
      variant="outline"
      size="icon"
      onClick={toggleTheme}
      aria-label={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}
      className="bg-accent text-accent-foreground border-[3px] shadow-brutal hover:shadow-brutal-hover"
    >
      {theme === 'light' ? (
        <Moon className="h-5 w-5" strokeWidth={2.5} />
      ) : (
        <Sun className="h-5 w-5" strokeWidth={2.5} />
      )}
    </Button>
  )
}
