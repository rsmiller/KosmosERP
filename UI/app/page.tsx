"use client"

import React from 'react'
import { useRouter } from 'next/navigation'
import { Box, Button, Stack, Text, HStack, Image } from '@chakra-ui/react'

const authMethods = [
  {
    key: 'database',
    label: 'Username & Password',
    description: 'Sign in with your Kosmos ERP account',
    href: '/login/database',
  },
  {
    key: 'saml',
    label: 'Single Sign-On (SSO)',
    description: 'Redirect to your organization identity provider',
    href: '/login/saml',
  },
  {
    key: 'keycloak',
    label: 'Keycloak',
    description: 'Sign in through Keycloak',
    href: '/login/keycloak',
  },
]

const LandingPage = () => {
  const router = useRouter()

  return (
    <Box
      minH="100vh"
      display="flex"
      alignItems="center"
      justifyContent="center"
      bg="gray.50"
      p={4}
    >
      <Box
        w="100%"
        maxW="650px"
        bg="white"
        borderRadius="lg"
        boxShadow="lg"
        p={8}
      >
        <Stack gap={2} mb={8} textAlign="center">
          <Image src="./kosmos_erp_med.png" alt="Kosmos ERP Logo" mb={4} p="auto" m="auto" height="200px" width="200px" />
          <Text color="gray.600">Choose how you would like to sign in</Text>
        </Stack>

        <HStack 
          gap={4}
          justifyContent="center"
          textAlign="center"
          alignItems="center">
          {authMethods.map((method) => (
            <Button
              key={method.key}
              onClick={() => router.push(method.href)}
              variant="outline"
              size="lg"
              height="auto"
              py={4}
              justifyContent="center"
              textAlign="center"
              alignItems="center"
              whiteSpace="normal"
            >
              <Stack gap={0}>
                <Text fontWeight="bold">{method.label}</Text>
              </Stack>
            </Button>
          ))}
        </HStack>
      </Box>
    </Box>
  )
}

export default LandingPage
