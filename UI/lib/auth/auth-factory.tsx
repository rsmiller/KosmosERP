"use client"

import React from 'react'
import appConfigurationInstance from '@/services/app-configuration-instance'
import { AuthMethod } from './types'
import KeycloakAuthBridge from './providers/keycloak-provider'
import DatabaseAuthBridge from './providers/database-provider'
import SamlAuthBridge from './providers/saml-provider'
import OidcAuthBridge from './providers/oidc-provider'

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
  return <AuthBridge>{children}</AuthBridge>
}
