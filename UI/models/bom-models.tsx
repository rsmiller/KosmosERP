import { BaseDto, DataCommand } from "./base-models";

export class BaseBOMData extends BaseDto
{
  parent_product_id?: number;
  product_id?: number;
  parent_bom_id?: number;
  quantity?: number;
  instructions?: string;
  order_number?: number;
  product_name?: string;
  isDirty: boolean = false; // Used on the ui
}

export class BOMDto extends BaseBOMData {


}

export class BOMListDto extends BaseBOMData {

}

export class BOMCreateCommand extends DataCommand {
  parent_product_id?: number;
  product_id?: number;
  parent_bom_id?: number;
  quantity?: number;
  order_number?: number;
  instructions?: string;
}

export class BOMEditCommand extends DataCommand {
  id?: number;
  parent_product_id?: number;
  product_id?: number;
  parent_bom_id?: number;
  quantity?: number;
  order_number?: number;
  instructions?: string;
}

export class BOMDeleteCommand extends DataCommand {
  id?: number;
}

export class BOMFindCommand extends DataCommand {
  parent_product_id?: number;
  parent_bom_id?: number;
  product_id?: number;
} 