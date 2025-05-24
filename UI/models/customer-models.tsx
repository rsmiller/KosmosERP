import { BaseDto } from "./base-models";

export interface CustomerListDto extends BaseDto
{
    customer_number?: number;
    customer_name?: string;
    customer_description?: string;
    phone?: string;
    fax?: string;
    general_email?: string;
    website?: string;
    category?: string;
    is_taxable?: boolean;
    tax_rate?: number;
    payment_terms?: string;
}
