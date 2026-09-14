import Keycloak from 'keycloak-js';

export const keycloakAuth = new Keycloak({
    url: process.env.NEXT_PUBLIC_AUTHORIZATION_URL || "",
    realm: process.env.NEXT_PUBLIC_AUTHORIZATION_REALM || "",
    clientId: process.env.NEXT_PUBLIC_AUTHORIZATION_CLIENT_ID || "",
});