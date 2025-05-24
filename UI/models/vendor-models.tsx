import { AddressDto } from "./address-models";
import { BaseDto } from "./base-models";

export interface VendorDto extends BaseDto
{
    vendor_number?: number;
    vendor_name?: string;
    vendor_description?: string;
    address_id?: number;
    phone?: string;
    fax?: string;
    general_email?: string;
    website?: string;
    category?: string;
    is_critial_vendor: boolean;
    approved_on?: Date | null;
    approved_by?: number;
    audit_on: Date | null;
    audit_by?: number;
    retired_on?: Date | null;
    retired_by?: number;

    address: AddressDto | null;
}

export interface VendorListDto extends BaseDto
{
    vendor_number?: number;
    vendor_name?: string;
    vendor_description?: string;
    address_id?: number;
    phone?: string;
    fax?: string;
    general_email?: string;
    website?: string;
    category?: string;
    is_critial_vendor: boolean;
    approved_on?: Date | null;
    approved_by?: number;
    audit_on?: Date | null;
    audit_by?: number;
    retired_on?: Date | null;
    retired_by?: number;
}