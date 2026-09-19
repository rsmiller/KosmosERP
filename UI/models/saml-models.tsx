// Mirrors KosmosERP.BusinessLayer.AuthenticiationProviders.Models.SAMLRequest
export interface SamlRequestDto {
  id?: string;
  request?: string;
  redirectUrl?: string;
  relayState?: string;
}
