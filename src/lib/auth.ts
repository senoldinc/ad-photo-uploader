import type { OidcClientSettingsStore } from 'oidc-client-ts'

export const oidcConfig = {
  authority: import.meta.env.VITE_OIDC_AUTHORITY || 'https://localhost:5000',
  client_id: import.meta.env.VITE_OIDC_CLIENT_ID || 'ad-photo-manager',
  redirect_uri: `${window.location.origin}/callback`,
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  scope: 'openid profile email',
} as OidcClientSettingsStore

export const handleSignoutRedirect = () => {
  // Optional: Clear app state before logout
  localStorage.removeItem('theme')
}
