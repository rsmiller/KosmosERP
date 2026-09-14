import { AddressDto } from "./address-models";
import { ARInvoiceLineDto } from "./ar-models";
import { BaseDto, DataCommand } from "./base-models";

export class OrderHeaderDto implements BaseDto {
  id: number = 0;
  is_deleted?: boolean | undefined;
  created_on?: string | undefined;
  created_by?: string | undefined;
  created_on_timezone?: string | undefined;
  created_on_string?: string | undefined;
  updated_on?: string | null | undefined;
  updated_by?: string | null | undefined;
  updated_on_timezone?: string | null | undefined;
  updated_on_string?: string | null | undefined;
  deleted_on?: string | null | undefined;
  deleted_by?: string | null | undefined;
  deleted_on_timezone?: string | null | undefined;
  deleted_on_string?: string | null | undefined;
  order_number?: number;
  customer_id?: number;
  ship_to_address_id?: number;
  billing_address_id?: number;
  shipping_method?: string;
  pay_method?: string;
  opportunity_id?: number | null;
  order_type?: string;
  revision_number?: number;
  order_date?: string;
  required_date?: string;
  po_number?: string | null;
  price?: number;
  tax?: number;
  shipping_cost?: number;
  guid?: string;
  deleted_reason?: string | null;
  canceled_reason?: string | null;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_on?: string | null;
  canceled_by?: number | null;

  customer_name?: string;
  pay_method_name?: string;
  shipping_method_name?: string;
  created_by_name?: string;

  ship_to_address?: AddressDto;
  order_lines?: Array<OrderLineDto>;
  ar_lines?: Array<ARInvoiceLineDto>;
}

export interface OrderHeaderListDto extends BaseDto {
  order_number?: number;
  customer_id?: number;
  ship_to_address_id?: number;
  billing_address_id?: number;
  shipping_method?: string;
  pay_method?: string;
  opportunity_id?: number | null;
  order_type?: string;
  revision_number?: number;
  order_date?: string;
  required_date?: string;
  po_number?: string | null;
  price?: number;
  tax?: number;
  shipping_cost?: number;
  guid?: string;
  deleted_reason?: string | null;
  canceled_reason?: string | null;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_on?: string | null;
  canceled_by?: number | null;

  customer_name?: string;
  pay_method_name?: string;
  shipping_method_name?: string;
}

export interface OrderLineAttributeDto
{
    order_line_id: string;
    attribute_name: string;
    attribute_value: string;
    attribute_value2: string;
    attribute_value3: string;
    guid: string;
}

export interface OrderLineDto extends BaseDto 
{
  order_header_id?: number;
  product_id: number;
  line_number: number;
  line_description: string;
  opportunity_line_id?: number | null;
  quantity: number;
  unit_price: number;
  guid: string;
  attributes?: OrderLineAttributeDto[];
  shipped_qty: number;

  product_name?: string;
  identifier1?: string;
  opportunity_name?: string;
}

export class OrderHeaderCreateCommand extends DataCommand {
  order_number?: number;
  customer_id?: number;
  ship_to_address_id?: number;
  shipping_method?: string;
  pay_method?: string;
  opportunity_id?: number;
  order_type?: string;
  revision_number?: number;
  order_date?: string;
  required_date?: string;
  po_number?: string;
  price?: number;
  tax?: number;
  shipping_cost?: number;
  deleted_reason?: string;
  canceled_reason?: string;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_on?: string;
  canceled_by?: number;
  order_lines?: OrderLineCreateCommand[];
}

export class OrderHeaderEditCommand extends DataCommand {
  id?: number;
  order_number?: number;
  customer_id?: number;
  ship_to_address_id?: number;
  billing_address_id?: number;
  shipping_method?: string;
  pay_method?: string;
  opportunity_id?: number;
  order_type?: string;
  revision_number?: number;
  order_date?: string;
  required_date?: string;
  po_number?: string;
  price?: number;
  tax?: number;
  shipping_cost?: number;
  deleted_reason?: string;
  canceled_reason?: string;
  is_complete?: boolean;
  is_canceled?: boolean;
  canceled_on?: string;
  canceled_by?: number;
  order_lines?: OrderLineEditCommand[];
}

export class OrderHeaderDeleteCommand extends DataCommand {
  id?: number;
}

export class OrderHeaderFindCommand extends DataCommand {
  customer_id?: number;
  wildcard?: string;
}

export class OrderLineCreateCommand extends DataCommand {
  order_header_id?: number;
  product_id?: number;
  line_number?: number;
  line_description?: string;
  opportunity_line_id?: number;
  quantity?: number;
  unit_price?: number;
  attributes?: OrderLineAttributeDto[];
}

export class OrderLineEditCommand extends DataCommand {
  id?: number;
  order_header_id?: number;
  product_id?: number;
  line_number?: number;
  line_description?: string;
  opportunity_line_id?: number;
  quantity?: number;
  unit_price?: number;
  attributes?: OrderLineAttributeDto[];
}

export class OrderLineDeleteCommand extends DataCommand {
  id?: number;
}