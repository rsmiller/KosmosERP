"use client"

import React, { useEffect, useState } from 'react'
import { ReactKeycloakProvider, useKeycloak } from '@react-keycloak/web'
import { keycloakAuth } from '@/lib/keycloak'
import { AuthContext } from '../auth-context'
import { AuthState } from '../types'
import SessionStorage from '@/components/session-storage'

function KeycloakInner({ children }: { children: React.ReactNode }) {
  const { keycloak, initialized } = useKeycloak()

  // Bumped whenever the token changes so the context value (read live off the
  // keycloak instance) re-renders for consumers.
  const [, setTick] = useState(0)
  const bump = () => setTick((t) => t + 1)

  useEffect(() => {
    if (!initialized || !keycloak?.authenticated) return

    SessionStorage.setToken(keycloak.token || '')
    SessionStorage.setName(keycloak.tokenParsed?.name || '')

    // Proactive refresh loop (moved out of the ERP shell): every 30s, refresh
    // if under 120s remain; on failure, clear the session and log out.
    const refreshInterval = setInterval(() => {
      if (!keycloak.authenticated) return

      keycloak
        .updateToken(120)
        .then((refreshed) => {
          if (refreshed) {
            SessionStorage.setToken(keycloak.token || '')
            SessionStorage.setName(keycloak.tokenParsed?.name || '')
            bump()
          }
        })
        .catch(() => {
          SessionStorage.removeName()
          SessionStorage.removeToken()
          keycloak.logout({ redirectUri: window.location.origin })
        })
    }, 30000)

    return () => clearInterval(refreshInterval)
  }, [keycloak?.authenticated, initialized])

  const value: AuthState = {
    ready: initialized,
    authenticated: !!keycloak?.authenticated,
    token: keycloak?.token || '',
    roles: keycloak?.tokenParsed?.realm_access?.roles || [],
    name: keycloak?.tokenParsed?.name || '',
    userId: keycloak?.tokenParsed?.sub || null,
    login: (returnUrl?: string) =>
      keycloak?.login({
        redirectUri:
          typeof window !== 'undefined'
            ? `${window.location.origin}${returnUrl ?? '/erp/'}`
            : undefined,
      }),
    logout: () => {
      SessionStorage.removeName()
      SessionStorage.removeToken()
      keycloak?.logout({ redirectUri: window.location.origin })
    },
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

/**
 * Keycloak auth via @react-keycloak/web. Preserves the original ERP-shell init
 * options (check-sso, silent SSO, PKCE) and bridges keycloak token/roles/name
 * into the shared AuthContext.
 */
export default function KeycloakAuthBridge({ children }: { children: React.ReactNode }) {
  const handleKeycloakEvent = (event: string) => {
    if (event === 'onReady' || event === 'onAuthSuccess') {
      if (typeof window !== 'undefined' && window.location.hash) {
        window.history.replaceState(null, '', window.location.pathname)
      }
    }
  }

  return (
    <ReactKeycloakProvider
      authClient={keycloakAuth}
      initOptions={{
        onLoad: 'check-sso',
        silentCheckSsoRedirectUri:
          typeof window !== 'undefined' ? `${window.location.origin}/silent-check-sso.html` : undefined,
        pkceMethod: 'S256',
        checkLoginIframe: true,
        redirectUri: typeof window !== 'undefined' ? `${window.location.origin}/erp/` : undefined,
      }}
      onEvent={handleKeycloakEvent}
    >
      <KeycloakInner>{children}</KeycloakInner>
    </ReactKeycloakProvider>
  )
}
