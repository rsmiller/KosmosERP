"use client"

import React, { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Box, Button, Heading, Input, Stack, Text } from '@chakra-ui/react'
import { PasswordInput } from '@/components/ui/password-input'
import { Toaster, toaster } from '@/components/ui/toaster'
import { userService } from '@/services/user-service'
import SessionStorage from '@/components/session-storage'
import { rolesToPermissionStrings } from '@/lib/auth/role-mapping'

const DatabaseLoginPage = () => {
  const router = useRouter()

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!username || !password) {
      toaster.create({ title: 'Please enter a username and password', type: 'error' })
      return
    }

    setSubmitting(true)

    try {
      const result = await userService.authenticateUser({ username, password })

      if (!result.success || !result.data || !result.data.authenticated) {
        toaster.create({
          title: 'Sign in failed',
          description: 'The username or password is incorrect.',
          type: 'error',
        })
        return
      }

      const auth = result.data

      SessionStorage.setToken(auth.token?.access_token || '')
      if (auth.token?.refresh_token) SessionStorage.setRefreshToken(auth.token.refresh_token)
      SessionStorage.setSession(auth.session?.session_id || '')
      SessionStorage.setUserId(String(auth.id))
      // Normalize API roles to the permissionsService string scheme (empty for now).

      //console.log(auth.roles);
      SessionStorage.setRoles(rolesToPermissionStrings(auth.roles))

      const fullName = auth.user
        ? `${auth.user.first_name ?? ''} ${auth.user.last_name ?? ''}`.trim()
        : ''
      SessionStorage.setName(fullName || auth.user?.username || username)

      router.push('/erp/')
    } catch {
      toaster.create({
        title: 'Sign in failed',
        description: 'Unable to reach the authentication service.',
        type: 'error',
      })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Box minH="100vh" display="flex" alignItems="center" justifyContent="center" bg="gray.50" p={4}>
      <Toaster />
      <Box w="100%" maxW="420px" bg="white" borderRadius="lg" boxShadow="lg" p={8}>
        <Stack gap={2} mb={6} textAlign="center">
          <Heading size="lg">Sign in</Heading>
          <Text color="gray.600">Enter your Kosmos ERP credentials</Text>
        </Stack>

        <form onSubmit={handleSubmit}>
          <Stack gap={4}>
            <Stack gap={1}>
              <Text fontSize="sm" fontWeight="medium">Username</Text>
              <Input
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                autoComplete="username"
                autoFocus
              />
            </Stack>

            <Stack gap={1}>
              <Text fontSize="sm" fontWeight="medium">Password</Text>
              <PasswordInput
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="current-password"
              />
            </Stack>

            <Button type="submit" colorPalette="blue" loading={submitting} loadingText="Signing in…">
              Sign in
            </Button>

            <Button variant="ghost" onClick={() => router.push('/')} type="button">
              Back to sign-in options
            </Button>
          </Stack>
        </form>
      </Box>
    </Box>
  )
}

export default DatabaseLoginPage
