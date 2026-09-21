"use client"

import React, { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Keycloak from 'keycloak-js'
import { ReactKeycloakProvider, useKeycloak } from '@react-keycloak/web'
import { Box, Heading, Spinner, Stack, Text } from '@chakra-ui/react'

const keycloak = new Keycloak({
  url: process.env.NEXT_PUBLIC_AUTHORIZATION_URL || "",
  realm: process.env.NEXT_PUBLIC_AUTHORIZATION_REALM || "",
  clientId: process.env.NEXT_PUBLIC_AUTHORIZATION_CLIENT_ID || "",
})

const LoginGate = () => {
  const { keycloak, initialized } = useKeycloak()
  const router = useRouter()

  useEffect(() => {
    if (initialized) {
      if (keycloak?.authenticated) {
        // User is logged in, redirect to ERP
        router.push('/erp/')
      } else {
        // User is not logged in, trigger login
        keycloak?.login()
      }
    }
  }, [keycloak?.authenticated, initialized])

  return (
    <Box minH="100vh" display="flex" alignItems="center" justifyContent="center" bg="gray.50" p={4}>
      <Box w="100%" maxW="420px" bg="white" borderRadius="lg" boxShadow="lg" p={8} textAlign="center">
        <Stack gap={4} align="center">
          <Spinner size="lg" />
          <Heading size="md">
            {!initialized ? 'Checking authentication status…' : 'Redirecting to Keycloak…'}
          </Heading>
          <Text color="gray.600">Please wait.</Text>
        </Stack>
      </Box>
    </Box>
  )
}

const KeycloakLoginPage = () => {
  // keycloak-js is browser-only; don't mount the provider during SSR/prerender.
  const [mounted, setMounted] = useState(false)
  useEffect(() => {
    setMounted(true)
  }, [])

  if (!mounted) return null

  return (
    <ReactKeycloakProvider
      authClient={keycloak}
      initOptions={{
        onLoad: 'login-required',
        pkceMethod: 'S256',
        checkLoginIframe: false,
      }}
    >
      <LoginGate />
    </ReactKeycloakProvider>
  )
}

export default KeycloakLoginPage
