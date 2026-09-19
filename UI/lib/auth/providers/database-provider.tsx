"use client"

import React, { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { AuthContext } from '../auth-context'
import { AuthState } from '../types'
import SessionStorage from '@/components/session-storage'

/**
 * Database (username/password) auth. The login page performs the API call and
 * persists the session to SessionStorage; this bridge hydrates from storage and
 * exposes it to the app. Token refresh is not applicable — the API-issued token
 * lives for the session lifetime.
 */
export default function DatabaseAuthBridge({ children }: { children: React.ReactNode }) {
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

  const logout = () => {
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
    login: () => router.push('/login/database'),
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
