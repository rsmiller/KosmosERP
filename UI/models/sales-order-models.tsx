import { BaseDto } from "./base-models";

export interface OrderHeaderListDto extends BaseDto {
  order_number?: number;
  customer_id?: number;
  ship_to_address_id?: number;
  shipping_method_id?: number;
  pay_method_id?: number | null;
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

  product_name?: string;
  opportunity_name?: string;
}