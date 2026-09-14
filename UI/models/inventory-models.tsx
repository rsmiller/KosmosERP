export class InventoryDto {
  id?: number;
  product_id?: number;
  product_name?: string;
  current_stock?: number;
  required_stock?: number;
  reorder_level?: number;
  on_hand?: number;
  reserved?: number;
  on_order?: number;
  to_order?: number;
  total_units_sold?: number;
  total_units_received?: number;
  total_units_shipped?: number;
  total_on_purchased?: number;
  guid?: string;
  created_on?: string;
  updated_on?: string;
}