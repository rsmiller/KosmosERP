"use client"

import "../globals.css";
import "../styles/sidebar.component.css";
import "react-datepicker/dist/react-datepicker.css";

import { ReactKeycloakProvider } from '@react-keycloak/web';

import { keycloakAuth } from '@/lib/keycloak';
import ContentComponent from './content';
import { useEffect } from "react";
import SessionStorage from "@/components/session-storage";
import { useRouter } from "next/navigation";

export default function ErpLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const router = useRouter();
  
  const handleKeycloakEvent = (event: any, error: any) => {
    if (event === 'onReady' || event === 'onAuthSuccess') {
      // Clean up URL after successful authentication
      if (typeof window !== 'undefined' && window.location.hash) {
        window.history.replaceState(null, '', window.location.pathname);
      }
    }
    if (event === 'onAuthError') {
      console.error('Keycloak auth error:', error);
    }
  };

  useEffect(() => {
    //console.log(keycloakAuth);

    // Fire a timer that will check if the keycloakAuth token is null, if it is redirect to login
    setTimeout(() => {
      if(keycloakAuth != undefined && keycloakAuth.token == undefined)
      {
        SessionStorage.removeName();
        SessionStorage.removeToken();

        keycloakAuth.logout({ redirectUri: window.location.origin });
      }
      else if(keycloakAuth == undefined)
      {
        SessionStorage.removeName();
        SessionStorage.removeToken();
        router.push("/");
      }

    }, 10000);
  }, [keycloakAuth]);

  return (
    <ReactKeycloakProvider 
      authClient={keycloakAuth}
      initOptions={{
        onLoad: 'check-sso',
        silentCheckSsoRedirectUri: typeof window !== 'undefined' ? `${window.location.origin}/silent-check-sso.html` : undefined,
        pkceMethod: 'S256',
        checkLoginIframe: true,
        redirectUri: typeof window !== 'undefined' ? `${window.location.origin}/erp/` : undefined
      }}
      onEvent={handleKeycloakEvent}
    >
      <ContentComponent>
        {children}
      </ContentComponent>
    </ReactKeycloakProvider>
  );
}
