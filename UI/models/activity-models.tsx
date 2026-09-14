import { BaseDto, DataCommand } from "./base-models";

export class ActivityDto extends BaseDto
{
    subject?: string;
    description?: string;
    activity_type?: string;
    status?: string;
    owner_id?: number;
    owner_name?: string;
    start_date?: string;
    end_date?: string;
    priority?: string;
    customer_id?: number;
    customer_name?: string;
    contact_id?: number;
    contact_name?: string;
    opportunity_id?: number;
    opportunity_name?: string;
    lead_id?: number;
    lead_name?: string;
    related_entity_id?: number;
    related_entity_type?: string;
    is_all_day?: boolean;
    location?: string;
    reminder_type?: string;
    reminder_time?: string;
}

export class ActivityListDto extends BaseDto
{
    subject?: string;
    description?: string;
    activity_type?: string;
    status?: string;
    owner_id?: number;
    owner_name?: string;
    start_date?: string;
    end_date?: string;
    priority?: string;
    customer_id?: number;
    customer_name?: string;
    contact_id?: number;
    contact_name?: string;
    opportunity_id?: number;
    opportunity_name?: string;
    lead_id?: number;
    lead_name?: string;
    related_entity_id?: number;
    related_entity_type?: string;
    is_all_day?: boolean;
    location?: string;
    reminder_type?: string;
    reminder_time?: string;
}

export class ActivityCreateCommand extends DataCommand {
    subject?: string;
    description?: string;
    activity_type?: string;
    status?: string;
    owner_id?: number;
    start_date?: string;
    end_date?: string;
    priority?: string;
    customer_id?: number;
    contact_id?: number;
    opportunity_id?: number;
    lead_id?: number;
    related_entity_id?: number;
    related_entity_type?: string;
    is_all_day?: boolean;
    location?: string;
    reminder_type?: string;
    reminder_time?: string;
}

export class ActivityEditCommand extends DataCommand {
    id?: number;
    subject?: string;
    description?: string;
    activity_type?: string;
    status?: string;
    owner_id?: number;
    start_date?: string;
    end_date?: string;
    priority?: string;
    customer_id?: number;
    contact_id?: number;
    opportunity_id?: number;
    lead_id?: number;
    related_entity_id?: number;
    related_entity_type?: string;
    is_all_day?: boolean;
    location?: string;
    reminder_type?: string;
    reminder_time?: string;
}

export class ActivityDeleteCommand extends DataCommand {
    id?: number;
}

export class ActivityFindCommand extends DataCommand {
    wildcard?: string;
    owner_id?: number;
    customer_id?: number;
    contact_id?: number;
    opportunity_id?: number;
    lead_id?: number;
    activity_type?: string;
    status?: string;
    start_date_from?: string;
    start_date_to?: string;
} 