import axiosInstance from "./axios-instance";
import { ApiResponse } from "@/models/base-models";
import { SamlRequestDto } from "@/models/saml-models";

export const samlService = {
  /**
   * Begins SP-initiated SSO. The API builds the AuthnRequest and returns the
   * IdP redirect URL. `returnUrl` is passed as RelayState so the IdP echoes it
   * back on the response, letting the app return the user where they started.
   */
  async begin(returnUrl: string, token: string = ""): Promise<ApiResponse<SamlRequestDto>> {
    try {
      const response = await axiosInstance(token).post<ApiResponse<SamlRequestDto>>(
        `/api/v1/SAML/init`,
        { relayState: returnUrl }
      );

      return response.data;
    } catch (ex: any) {
      if (ex.response != undefined && ex.response.data != undefined) {
        return ex.response.data as ApiResponse<SamlRequestDto>;
      } else {
        return { success: false, exception: ex, resultCode: -5, data: undefined };
      }
    }
  },
};
