// Provider-agnostic authentication contract shared by every auth method.

export type AuthMethod = 'database' | 'saml' | 'keycloak' | 'oidc';

/**
 * The auth state carried through React context and consumed by every page via
 * useAuth(). `token` is a synchronous string (kept fresh in the background by
 * the active provider) so pages can read it the way they used to read
 * keycloak.token. `roles` is normalized to the permissionsService string scheme
 * (e.g. "customers_read", "admin") regardless of the underlying method.
 */
export interface AuthState {
  /** True once the provider has finished initializing (auth status is known). */
  ready: boolean;
  authenticated: boolean;
  token: string;
  roles: string[];
  name: string;
  userId: string | null;
  /** Start a login. For SAML/OIDC this redirects; returnUrl is where to land after. */
  login: (returnUrl?: string) => void | Promise<void>;
  logout: () => void | Promise<void>;
}
