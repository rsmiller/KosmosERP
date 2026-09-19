

const appConfigurationInstance = {

  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
  authMethod: process.env.NEXT_PUBLIC_AUTH_METHOD,
  samlLoginUrl: process.env.NEXT_PUBLIC_SAML_LOGIN_URL,
};

export default appConfigurationInstance;