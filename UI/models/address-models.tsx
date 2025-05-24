import { BaseDto } from "./base-models";

export interface AddressDto extends BaseDto
{
    street_address1?: string;
    street_address2?: string;
    city?: string;
    state?: string;
    postal_code?: string;
    country?: string;
}