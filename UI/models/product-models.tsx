// Import or define BaseDto and ProductAttributeDto as needed

import { BaseDto } from "./base-models";

export interface ProductAttributeDto extends BaseDto
{
    product_id?: number;
    attribute_name?: string;
    attribute_value?: string;
    attribute_value2?: string;
    attribute_value3?: string;
}


export interface ProductDto extends BaseDto  {
  vendor_id?: number;
  product_class?: string;
  category?: string;
  identifier1?: string;
  identifier2?: string | null;
  identifier3?: string | null;
  product_name?: string;
  internal_description?: string;
  external_description?: string | null;
  required_stock_level?: number;
  required_reorder_level?: number;
  required_min_order?: number;
  our_cost?: number;
  unit_cost?: number;
  sales_price?: number;
  list_price?: number;
  rfid_id?: string | null;
  is_taxable?: boolean;
  is_stock: boolean;
  is_material: boolean;
  is_rental_item: boolean;
  is_sales_item: boolean;
  is_labor: boolean;
  is_shippable: boolean;
  is_retired: boolean;
  created_by_name?: string | null;
  updated_by_name?: string | null;
  retired_on?: Date | null;
  retired_by_name?: string | null;

  product_attributes?: ProductAttributeDto[];

  vendor_name?: string;
}

export interface ProductListDto extends BaseDto  {
  vendor_id?: number;
  product_class?: string;
  category?: string;
  identifier1?: string;
  identifier2?: string | null;
  identifier3?: string | null;
  product_name?: string;
  internal_description?: string;
  external_description?: string | null;
  required_stock_level?: number;
  required_reorder_level?: number;
  required_min_order?: number;
  our_cost?: number;
  unit_cost?: number;
  sales_price?: number;
  list_price?: number;
  rfid_id?: string | null;
  is_taxable?: boolean;
  is_stock: boolean;
  is_material: boolean;
  is_rental_item: boolean;
  is_sales_item: boolean;
  is_labor: boolean;
  is_shippable: boolean;
  is_retired: boolean;
  created_by_name?: string | null;
  updated_by_name?: string | null;
  retired_on?: Date | null;
  retired_by_name?: string | null;

  vendor_name?: string;
}
