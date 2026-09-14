import { BaseDto, DataCommand } from "./base-models";

export class OpportunityDto extends BaseDto
{
    opportunity_name?: string;
    customer_id?: number;
    contact_id?: number;
    amount?: number;
    stage?: string;
    win_chance?: number;
    expected_close?: Date;
    owner_id?: number;

    customer_name?: string;
    contact_name?: string;
    owner_name?: string;
    stage_name?: string;

    opportunity_lines?: Array<OpportunityLineDto>;
}

export class OpportunityListDto extends BaseDto
{
    opportunity_name?: string;
    customer_id?: number;
    contact_id?: number;
    amount?: number;
    stage?: string;
    win_chance?: number;
    expected_close?: Date;
    owner_id?: number;

    customer_name?: string;
    contact_name?: string;
    owner_name?: string;
    stage_name?: string;
}

export class OpportunityCreateCommand extends DataCommand {
    opportunity_name?: string;
    customer_id?: number;
    contact_id?: number;
    amount?: number;
    stage?: string;
    win_chance?: number;
    expected_close?: string;
    owner_id?: number;

    opportunity_lines?: Array<OpportunityLineCreateCommand>;
}

export class OpportunityEditCommand extends DataCommand {
    id?: number;
    opportunity_name?: string;
    customer_id?: number;
    contact_id?: number;
    amount?: number;
    stage?: string;
    win_chance?: number;
    expected_close?: string;
    owner_id?: number;

    opportunity_lines?: Array<OpportunityLineEditCommand>;
}

export class OpportunityDeleteCommand extends DataCommand {
    id?: number;
}

export class OpportunityFindCommand extends DataCommand {
    opportunity_name?: string;
    customer_id?: number;
    contact_id?: number;
    amount?: number;
    stage?: string;
    win_chance?: number;
    expected_close?: Date;
    owner_id?: number;
    customer_name?: string;
    contact_name?: string;
    owner_name?: string;
}



export class OpportunityLineDto extends BaseDto
{
    opportunity_id?: number;
    product_id?: number;
    description?: string;
    line_number?: number;
    quantity?: number;
    unit_price?: number;
    guid?: string;

    product_name?: string;
    identifier1?: string;
    opportunity_name?: string;
}

export class OpportunityLineListDto extends BaseDto
{
    opportunity_id?: number;
    product_id?: number;
    description?: string;
    line_number?: number;
    quantity?: number;
    unit_price?: number;
    guid?: string;

    product_name?: string;
    opportunity_name?: string;
}

export class OpportunityLineCreateCommand extends DataCommand {
    opportunity_id?: number;
    product_id?: number;
    description?: string;
    line_number?: number;
    quantity?: number;
    unit_price?: number;

    // For ui
    product_name?: string;
    guid?: string;
}

export class OpportunityLineEditCommand extends DataCommand {
    id?: number;
    opportunity_id?: number;
    product_id?: number;
    description?: string;
    line_number?: number;
    quantity?: number;
    unit_price?: number;
}

export class OpportunityLineDeleteCommand extends DataCommand {
    id?: number;
}

export class OpportunityLineFindCommand extends DataCommand {
    opportunity_id?: number;
    product_id?: number;
    description?: string;
    line_number?: number;
    quantity?: number;
    unit_price?: number;
    product_name?: string;
    opportunity_name?: string;
} 