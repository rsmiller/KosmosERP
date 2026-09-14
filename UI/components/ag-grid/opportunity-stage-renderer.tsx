import { KeyValueDto } from "@/models/key-value-models";

export function OpportunityStageRenderer(params: any)
{
    //console.log(params);
    const value  = params.stages.find((e: KeyValueDto) => e.key == params.value);
    return value;
}
