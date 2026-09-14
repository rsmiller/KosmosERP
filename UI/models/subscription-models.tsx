import { BaseDto, DataCommand } from "./base-models";
import { CustomerDto } from "./customer-models";
import { OrderHeaderDto } from "./sales-order-models";

export class SubscriptionDto extends BaseDto 
{
    customer_id?: number;
    order_header_id?: number;
    subscription_number?: number;
    quantity?: number;
    price?: number;
    tax?: number;
    cycle_days?: number;
    start_date?: string;
    next_date?: string;
    end_date?: string;

    customer?: CustomerDto;
    order?: OrderHeaderDto;
}

export class SubscriptionListDto extends BaseDto 
{
    customer_id?: number;
    order_header_id?: number;
    subscription_number?: number;
    quantity?: number;
    price?: number;
    tax?: number;
    cycle_days?: number;
    start_date?: string;
    next_date?: string;
    end_date?: string;

    customer_name?: string;
}

export class SubscriptionCreateCommand extends DataCommand 
{
    customer_id?: number;
    order_header_id?: number;
    quantity?: number;
    price?: number;
    tax?: number;
    cycle_days?: number;
    start_date?: string;
    end_date?: Date;
}

export class SubscriptionEditCommand extends DataCommand 
{
    id?: number;
    quantity?: number;
    price?: number;
    tax?: number;
    cycle_days?: number;
    start_date?: string;
    end_date?: string;
}

export class SubscriptionDeleteCommand extends DataCommand {
  id?: number;
}

export class SubscriptionFindCommand extends DataCommand {
    wildcard?: string;
    customer_id?: number;
}
