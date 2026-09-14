import { BaseDto, DataCommand } from "./base-models";

export class ContactDto extends BaseDto {
    customer_id?: number;
    first_name?: string;
    last_name?: string;
    title?: string;
    email?: string;
    phone?: string;
    cell_phone?: string;

    customer_name?: string;
}


export class ContactListDto extends BaseDto {
    customer_id?: number;
    first_name?: string;
    last_name?: string;
    title?: string;
    email?: string;
    phone?: string;
    cell_phone?: string;

    customer_name?: string;
}

export class ContactCreateCommand extends DataCommand {
  customer_id?: number;
  first_name?: string;
  last_name?: string;
  title?: string;
  email?: string;
  phone?: string;
  cell_phone?: string;
}

export class ContactEditCommand extends DataCommand {
  id?: number;
  customer_id?: number;
  first_name?: string;
  last_name?: string;
  title?: string;
  email?: string;
  phone?: string;
  cell_phone?: string;
}

export class ContactDeleteCommand extends DataCommand {
  id?: number;
}

export class ContactFindCommand extends DataCommand {
  wildcard?: string;
  customer_id?: number;
}
  
