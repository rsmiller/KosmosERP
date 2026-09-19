"use client"

import React from 'react'
import { AuthContext } from '../auth-context'
import { AuthState } from '../types'

/**
 * Designed-for placeholder. Adding OIDC support means implementing this bridge
 * (e.g. with oidc-client-ts) exactly like the other providers — no page or
 * factory-consumer changes required. It intentionally does not authenticate;
 * the route guard will send the user back to the landing page.
 */
export default function OidcAuthBridge({ children }: { children: React.ReactNode }) {
  const notImplemented = () => {
    console.error('OIDC authentication is configured but not yet implemented.')
  }

  const value: AuthState = {
    ready: true,
    authenticated: false,
    token: '',
    roles: [],
    name: '',
    userId: null,
    login: notImplemented,
    logout: notImplemented,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
