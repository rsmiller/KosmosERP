"use client"

import { createContext, useContext } from 'react'
import { AuthState } from './types'

const defaultAuthState: AuthState = {
  ready: false,
  authenticated: false,
  token: '',
  roles: [],
  name: '',
  userId: null,
  login: () => {},
  logout: () => {},
}

export const AuthContext = createContext<AuthState>(defaultAuthState)

/**
 * The single hook every page uses to read auth state and trigger login/logout,
 * regardless of which authentication method is configured.
 */
export const useAuth = (): AuthState => useContext(AuthContext)
