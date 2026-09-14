// Import or define BaseDto and ProductAttributeDto as needed

import { BaseDto, DataCommand } from "./base-models";

export class ProductAttributeDto extends BaseDto
{
    product_id?: number;
    attribute_name?: string;
    attribute_value?: string;
    attribute_value2?: string;
    attribute_value3?: string;
}


export class ProductDto extends BaseDto  {
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
  is_stock?: boolean;
  is_material?: boolean;
  is_rental_item?: boolean;
  is_sales_item?: boolean;
  is_labor?: boolean;
  is_shippable?: boolean;
  is_retired?: boolean;
  is_manufactured?: boolean;
  manufacture_time_minutes?: number;
  created_by_name?: string | null;
  updated_by_name?: string | null;
  retired_on?: Date | null;
  retired_by_name?: string | null;

  product_attributes?: ProductAttributeDto[];

  vendor_name?: string;
  category_name?: string;
}

export class ProductListDto extends BaseDto  {
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
  is_stock?: boolean;
  is_material?: boolean;
  is_rental_item?: boolean;
  is_sales_item?: boolean;
  is_labor?: boolean;
  is_shippable?: boolean;
  is_retired?: boolean;
  is_manufactured?: boolean;
  manufacture_time_minutes?: number;
  created_by_name?: string | null;
  updated_by_name?: string | null;
  retired_on?: Date | null;
  retired_by_name?: string | null;

  vendor_name?: string;
  category_name?: string;
}

export class ProductCreateCommand extends DataCommand {
  vendor_id?: number;
  product_class?: string;
  category?: string;
  identifier1?: string;
  identifier2?: string;
  identifier3?: string;
  product_name?: string;
  internal_description?: string;
  external_description?: string;
  required_stock_level?: number;
  required_reorder_level?: number;
  required_min_order?: number;
  our_cost?: number;
  unit_cost?: number;
  sales_price?: number;
  list_price?: number;
  rfid_id?: string;
  is_taxable?: boolean;
  is_stock?: boolean;
  is_material?: boolean;
  is_rental_item?: boolean;
  is_sales_item?: boolean;
  is_labor?: boolean;
  is_shippable?: boolean;
  is_retired?: boolean;
  is_manufactured?: boolean;
  manufacture_time_minutes?: number;
}

export class ProductEditCommand extends DataCommand {
  id?: number;
  vendor_id?: number;
  product_class?: string;
  category?: string;
  identifier1?: string;
  identifier2?: string;
  identifier3?: string;
  product_name?: string;
  internal_description?: string;
  external_description?: string;
  required_stock_level?: number;
  required_reorder_level?: number;
  required_min_order?: number;
  our_cost?: number;
  unit_cost?: number;
  sales_price?: number;
  list_price?: number;
  rfid_id?: string;
  is_taxable?: boolean;
  is_stock?: boolean;
  is_material?: boolean;
  is_rental_item?: boolean;
  is_sales_item?: boolean;
  is_labor?: boolean;
  is_shippable?: boolean;
  is_retired?: boolean;
  is_manufactured?: boolean;
  manufacture_time_minutes?: number;
}

export class ProductDeleteCommand extends DataCommand {
  id?: number;
}

export class ProductFindCommand extends DataCommand {
  wildcard?: string;
}
