import { BaseDto, DataCommand } from "./base-models";

export class KeyValueDto  extends BaseDto 
{
    key: string = "";
    value: string = "";
    int_value?: number;
    module_id?: string;
}

export class KeyValueFindCommand extends DataCommand 
{
    wildcard?: string;
}

export class KeyValueDeleteCommand extends DataCommand 
{
    id?: number;
}


export class KeyValueEditCommand extends DataCommand
{
    id: number = 0;
    value: string = "";
    int_value?: number;
    module_id?: string;
}

export class KeyValueCreateCommand extends DataCommand
{
    key: string = "";
    value: string = "";
    int_value?: number;
    module_id?: string;
}

export class ModuleObjectDto
{
    module_id: string = "";
    module_name: string = "";
    name?: string;
}