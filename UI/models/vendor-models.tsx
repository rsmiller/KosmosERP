import { AddressDto, AddressEditCommand } from "./address-models";
import { BaseDto, DataCommand } from "./base-models";

export class VendorDto extends BaseDto
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
    is_critial_vendor?: boolean;
    approved_on?: Date | null;
    approved_by?: number;
    audit_on?: Date | null;
    audit_by?: number;
    retired_on?: Date | null;
    retired_by?: number;

    address?: AddressDto | null;
}

export class VendorListDto extends BaseDto
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
    is_critial_vendor?: boolean;
    approved_on?: Date | null;
    approved_by?: number;
    audit_on?: Date | null;
    audit_by?: number;
    retired_on?: Date | null;
    retired_by?: number;
}

export class VendorCreateCommand extends DataCommand {
  vendor_name?: string;
  vendor_description?: string;
  address_id?: number;
  phone?: string;
  fax?: string;
  general_email?: string;
  website?: string;
  category?: string;
  is_critial_vendor?: boolean;
  approved_on?: Date;
  approved_by?: number;
  audit_on?: Date;
  audit_by?: number;
  retired_on?: Date;
  retired_by?: number;
  address: AddressEditCommand = new AddressEditCommand();
}

export class VendorEditCommand extends DataCommand {
  id?: number;
  vendor_name?: string;
  vendor_description?: string;
  address_id?: number;
  phone?: string;
  fax?: string;
  general_email?: string;
  website?: string;
  category?: string;
  is_critial_vendor?: boolean;
  approved_on?: Date;
  approved_by?: number;
  audit_on?: Date;
  audit_by?: number;
  retired_on?: Date;
  retired_by?: number;
  address?: AddressEditCommand = new AddressEditCommand();
}

export class VendorDeleteCommand extends DataCommand {
  id?: number;
}

export class VendorFindCommand extends DataCommand {
  wildcard?: string;
}
