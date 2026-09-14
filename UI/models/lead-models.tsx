import { BaseDto, DataCommand } from "./base-models";

export class LeadDto extends BaseDto 
{
    first_name?: string;
    last_name?: string;
    title?: string;
    email?: string;
    phone?: string;
    cell_phone?: string;
    company_name?: string;
    lead_stage?: string;
    time_zone?: string;
    address_line1?: string;
    address_line2?: string;
    city?: string;
    state?: string;
    zip?: string;
    country?: string;
    is_converted?: boolean;
    converted_customer_id?: number;
    converted_contact_id?: number;
    owner_id?: number;

    owner_name?: string;
    stage_name?: string;
}

export class LeadListDto extends BaseDto 
{
    first_name?: string;
    last_name?: string;
    title?: string;
    email?: string;
    phone?: string;
    cell_phone?: string;
    company_name?: string;
    lead_stage?: string;
    time_zone?: string;
    address_line1?: string;
    address_line2?: string;
    city?: string;
    state?: string;
    zip?: string;
    country?: string;
    is_converted?: boolean;
    converted_customer_id?: number;
    converted_contact_id?: number;
    owner_id?: number;

    owner_name?: string;
    stage_name?: string;
}

export class LeadCreateCommand extends DataCommand {
  first_name?: string;
  last_name?: string;
  title?: string;
  email?: string;
  phone?: string;
  cell_phone?: string;
  company_name?: string;
  lead_stage?: string;
  time_zone?: string;
  address_line1?: string;
  address_line2?: string;
  city?: string;
  state?: string;
  zip?: string;
  country?: string;
  owner_id?: number;
}

export class LeadEditCommand extends DataCommand {
  id?: number;
  first_name?: string;
  last_name?: string;
  title?: string;
  email?: string;
  phone?: string;
  cell_phone?: string;
  company_name?: string;
  lead_stage?: string;
  time_zone?: string;
  address_line1?: string;
  address_line2?: string;
  city?: string;
  state?: string;
  zip?: string;
  country?: string;
  owner_id?: number;
}

export class LeadDeleteCommand extends DataCommand {
  id?: number;
}

export class LeadFindCommand extends DataCommand {
  first_name?: string;
  last_name?: string;
  title?: string;
  email?: string;
  phone?: string;
  cell_phone?: string;
  company_name?: string;
  lead_stage?: string;
  time_zone?: string;
  address_line1?: string;
  address_line2?: string;
  city?: string;
  state?: string;
  zip?: string;
  country?: string;
  is_converted?: boolean;
  converted_customer_id?: number;
  converted_contact_id?: number;
  owner_id?: number;
  owner_name?: string;
}
