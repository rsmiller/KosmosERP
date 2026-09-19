"use client"

import React, { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { AuthContext } from '../auth-context'
import { AuthState } from '../types'
import SessionStorage from '@/components/session-storage'
import { samlService } from '@/services/saml-service'

/**
 * SAML SSO auth. login() begins SP-initiated SSO and redirects the browser to
 * the IdP; the SAML callback page persists the resulting session to
 * SessionStorage, from which this bridge hydrates.
 */
export default function SamlAuthBridge({ children }: { children: React.ReactNode }) {
  const router = useRouter()

  const [ready, setReady] = useState(false)
  const [token, setToken] = useState('')
  const [name, setName] = useState('')
  const [userId, setUserId] = useState<string | null>(null)
  const [roles, setRoles] = useState<string[]>([])

  useEffect(() => {
    setToken(SessionStorage.getToken() || '')
    setName(SessionStorage.getName() || '')
    setUserId(SessionStorage.getUserId())
    setRoles(SessionStorage.getRoles())
    setReady(true)
  }, [])

  const login = async (returnUrl: string = '/erp/') => {
    const result = await samlService.begin(returnUrl)

    if (result.success && result.data?.redirectUrl) {
      window.location.href = result.data.redirectUrl
    } else {
      router.push('/login/saml')
    }
  }

  const logout = () => {
    // Local session teardown. SP-initiated Single Logout (the IdP round-trip we
    // built server-side) can be wired in here later.
    SessionStorage.removeToken()
    SessionStorage.removeName()
    SessionStorage.removeSession()
    SessionStorage.removeUserId()
    SessionStorage.removeRoles()
    router.push('/')
  }

  const value: AuthState = {
    ready,
    authenticated: !!token,
    token,
    roles,
    name,
    userId,
    login,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
