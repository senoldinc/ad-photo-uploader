import React, { useEffect } from 'react'
import { useAuth } from 'react-oidc-context'

export const Callback: React.FC = () => {
  const auth = useAuth()

  useEffect(() => {
    // Handle OIDC redirect
    if (auth.isLoading) return

    if (auth.error) {
      console.error('OIDC Error:', auth.error)
      // Redirect to home on error
      window.location.href = '/'
    }

    if (auth.isAuthenticated) {
      // Redirect to dashboard on success
      window.location.href = '/'
    }
  }, [auth])

  return (
    <div className="flex items-center justify-center min-h-screen bg-bg">
      <div className="text-center">
        <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-accent border-t-transparent mb-4" />
        <h1 className="text-2xl font-display font-bold text-text mb-2">Authenticating...</h1>
        <p className="text-text-muted">Please wait while we verify your identity.</p>
      </div>
    </div>
  )
}
