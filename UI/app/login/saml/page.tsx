"use client"

import React, { Suspense, useEffect, useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Box, Button, Heading, Spinner, Stack, Text } from '@chakra-ui/react'
import { samlService } from '@/services/saml-service'

const SamlRedirect = () => {
  const router = useRouter()
  const searchParams = useSearchParams()

  // Where the user should land back in the app once SSO completes.
  const returnUrl = searchParams.get('returnUrl') || '/erp/'

  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    const begin = async () => {
      const result = await samlService.begin(returnUrl)

      if (cancelled) return

      if (result.success && result.data?.redirectUrl) {
        // Hand the browser off to the identity provider.
        window.location.href = result.data.redirectUrl
      } else {
        setError('Unable to start single sign-on. SAML may not be configured on the server.')
      }
    }

    begin()

    return () => {
      cancelled = true
    }
  }, [returnUrl])

  return (
    <Box minH="100vh" display="flex" alignItems="center" justifyContent="center" bg="gray.50" p={4}>
      <Box w="100%" maxW="420px" bg="white" borderRadius="lg" boxShadow="lg" p={8} textAlign="center">
        {error ? (
          <Stack gap={4} align="center">
            <Heading size="md">Single sign-on unavailable</Heading>
            <Text color="gray.600">{error}</Text>
            <Button onClick={() => router.push('/')}>Back to sign-in options</Button>
          </Stack>
        ) : (
          <Stack gap={4} align="center">
            <Spinner size="lg" />
            <Heading size="md">Redirecting to your identity provider…</Heading>
            <Text color="gray.600">Please wait while we hand you off to sign in.</Text>
          </Stack>
        )}
      </Box>
    </Box>
  )
}

const SamlLoginPage = () => {
  return (
    <Suspense fallback={null}>
      <SamlRedirect />
    </Suspense>
  )
}

export default SamlLoginPage
