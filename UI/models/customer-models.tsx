import { BaseDto, DataCommand } from "./base-models";

export class CustomerDto extends BaseDto
{
    customer_number?: number;
    customer_name?: string;
    customer_description?: string;
    phone?: string;
    fax?: string;
    general_email?: string;
    accounting_email?: string;
    website?: string;
    category?: string;
    is_taxable?: boolean;
    tax_rate?: number;
    payment_terms?: string;
    payment_terms_name?: string;
}

export class CustomerListDto extends BaseDto
{
    customer_number?: number;
    customer_name?: string;
    customer_description?: string;
    phone?: string;
    fax?: string;
    general_email?: string;
    accounting_email?: string;
    website?: string;
    category?: string;
    is_taxable?: boolean;
    tax_rate?: number;
    payment_terms?: string;
    payment_terms_name?: string;
}

export class CustomerCreateCommand extends DataCommand {
  customer_name?: string;
  customer_description?: string;
  phone?: string;
  fax?: string;
  general_email?: string;
  accounting_email?: string;
  website?: string;
  category?: string;
  payment_terms?: string;
  is_taxable?: boolean;
  tax_rate?: number;
}

export class CustomerEditCommand extends DataCommand {
  id?: number;
  customer_name?: string;
  customer_description?: string;
  phone?: string;
  fax?: string;
  general_email?: string;
  accounting_email?: string;
  website?: string;
  category?: string;
  is_taxable?: boolean;
  tax_rate?: number;
  payment_terms?: string;
}

export class CustomerDeleteCommand extends DataCommand {
  id?: number;
}

export class CustomerFindCommand extends DataCommand {
  wildcard?: string;
}
