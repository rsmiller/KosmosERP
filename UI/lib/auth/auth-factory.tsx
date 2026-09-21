"use client"

import React, { useEffect, useState } from 'react'
import appConfigurationInstance from '@/services/app-configuration-instance'
import { AuthMethod, AuthState } from './types'
import { AuthContext } from './auth-context'
import KeycloakAuthBridge from './providers/keycloak-provider'
import DatabaseAuthBridge from './providers/database-provider'
import SamlAuthBridge from './providers/saml-provider'
import OidcAuthBridge from './providers/oidc-provider'

// State supplied during SSR / static prerender and the first client render,
// before any provider mounts. `ready: false` keeps pages in their loading state.
const ssrAuthState: AuthState = {
  ready: false,
  authenticated: false,
  token: '',
  roles: [],
  name: '',
  userId: null,
  login: () => {},
  logout: () => {},
}

/**
 * Selects the auth provider bridge from the configured method. Adding a new
 * method (e.g. a real OIDC provider) is a single case here plus its bridge file
 * — no consumer/page changes.
 */
function AuthBridge({ children }: { children: React.ReactNode }) {
  const method = (appConfigurationInstance.authMethod || 'keycloak') as AuthMethod

  switch (method) {
    case 'database':
      return <DatabaseAuthBridge>{children}</DatabaseAuthBridge>
    case 'saml':
      return <SamlAuthBridge>{children}</SamlAuthBridge>
    case 'oidc':
      return <OidcAuthBridge>{children}</OidcAuthBridge>
    case 'keycloak':
    default:
      return <KeycloakAuthBridge>{children}</KeycloakAuthBridge>
  }
}

/**
 * Wrap authenticated areas of the app with this. It instantiates the configured
 * provider once and supplies auth state to everything below via useAuth().
 */
export function AuthShell({ children }: { children: React.ReactNode }) {
  // Auth providers (Keycloak in particular) are browser-only and must never run
  // during SSR/prerender. Mount the real provider after hydration; on the server
  // and first client render, supply a not-ready context so markup matches.
  const [mounted, setMounted] = useState(false)
  useEffect(() => {
    setMounted(true)
  }, [])

  if (!mounted) {
    return <AuthContext.Provider value={ssrAuthState}>{children}</AuthContext.Provider>
  }

  return <AuthBridge>{children}</AuthBridge>
}
