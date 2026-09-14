"use client"

import React, { use, useEffect, useMemo } from 'react'
import { useRouter } from 'next/navigation'
import Keycloak from 'keycloak-js';
import { ReactKeycloakProvider, useKeycloak } from '@react-keycloak/web';

const keycloak = new Keycloak({
    url: process.env.NEXT_PUBLIC_AUTHORIZATION_URL || "",
    realm: process.env.NEXT_PUBLIC_AUTHORIZATION_REALM || "",
    clientId: process.env.NEXT_PUBLIC_AUTHORIZATION_CLIENT_ID || "",
});

const LoginGate = () => {
  const { keycloak, initialized } = useKeycloak();
  const router = useRouter();

  useEffect(() => {
    
    if (initialized) {
      if (keycloak?.authenticated) {
        // User is logged in, redirect to ERP
        router.push('/erp/');
      } else {
        // User is not logged in, trigger login
        keycloak?.login();
      }
    }
  }, [keycloak?.authenticated]);

  // Show loading while initializing
  if (!initialized) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh',
      }}>
        <div>
          <h2>Checking authentication status...</h2>
        </div>
      </div>
    );
  }

  // Show login message while redirecting
  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      height: '100vh',
    }}>
      <div>
        <h2>Redirecting to application...</h2>
      </div>
    </div>
  );
};



const App = () => {
  

  return (
    <ReactKeycloakProvider 
      authClient={keycloak} 
      initOptions={{
        onLoad: 'login-required',
        pkceMethod: 'S256',
        checkLoginIframe: false
      }}
      >
      <LoginGate />
    </ReactKeycloakProvider>
  )
}

export default App
