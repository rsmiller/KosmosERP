import { BaseDto, DataCommand } from "./base-models";

export class AddressDto extends BaseDto
{
    street_address1?: string;
    street_address2?: string;
    city?: string;
    state?: string;
    postal_code?: string;
    country?: string;
    guid?: string;
}

export class AddressListDto extends BaseDto
{
    street_address1?: string;
    street_address2?: string;
    city?: string;
    state?: string;
    postal_code?: string;
    country?: string;
    guid?: string;
}

export class AddressCreateCommand extends DataCommand
{
    street_address1?: string;
    street_address2?: string;
    city?: string;
    state?: string;
    postal_code?: string;
    country?: string;
    customer_id?: number;
    address_type_id?: number;
}

export class AddressEditCommand extends DataCommand
{
    id?: number;
    street_address1?: string;
    street_address2?: string;
    city?: string;
    state?: string;
    postal_code?: string;
    country?: string;
}

export class AddressDeleteCommand extends DataCommand
{
    id?: number;
}

export class AddressFindCommand extends DataCommand
{
    customer_id?: number;
    wildcard?: string;
    address_type_id?: number;
}