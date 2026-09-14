import { BaseDto, DataCommand } from "./base-models";

export class TransactionDto extends BaseDto {
  product_id?: number;
  transaction_type?: number;
  transaction_date?: Date;
  object_reference_id?: number;
  object_sub_reference_id?: number;
  units_sold?: number;
  units_shipped?: number;
  units_purchased?: number;
  units_received?: number;
  purchased_unit_cost?: number;
  sold_unit_price?: number;
}

export class TransactionListDto extends BaseDto {
  product_id?: number;
  transaction_type?: number;
  transaction_date?: Date;
  object_reference_id?: number;
  object_sub_reference_id?: number;
  units_sold?: number;
  units_shipped?: number;
  units_purchased?: number;
  units_received?: number;
  purchased_unit_cost?: number;
  sold_unit_price?: number;
  product_name?: string;
  transaction_type_name?: string;
}

export class TransactionCreateCommand extends DataCommand {
  product_id?: number;
  transaction_type?: number;
  transaction_date?: string;
  object_reference_id?: number;
  object_sub_reference_id?: number;
  units_sold?: number;
  units_shipped?: number;
  units_purchased?: number;
  units_received?: number;
  purchased_unit_cost?: number;
  sold_unit_price?: number;
}

export class TransactionEditCommand extends DataCommand {
  id?: number;
  product_id?: number;
  transaction_type?: number;
  transaction_date?: string;
  object_reference_id?: number;
  object_sub_reference_id?: number;
  units_sold?: number;
  units_shipped?: number;
  units_purchased?: number;
  units_received?: number;
  purchased_unit_cost?: number;
  sold_unit_price?: number;
}

export class TransactionDeleteCommand extends DataCommand {
  id?: number;
}

export class TransactionFindCommand extends DataCommand {
  product_id?: number;
  object_reference_id?: number;
  wildcard?: string;
  sales_order_number?: number;
  purchase_order_number?: number;
} 