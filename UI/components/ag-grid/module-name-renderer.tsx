import { ModuleObjectDto } from "@/models/key-value-models";

export function ModuleNameRenderer(params: any)
{
    //console.log("ModuleNameRenderer:", params);

    if(params.moduleData == undefined)
    {
        return "";
    }

    const module = params.moduleData.find((e: ModuleObjectDto) => e.module_id == params.value);

    if(module == undefined || module.length == 0)
    {
        return "";
    }

    return module.module_name != undefined ? module.module_name : module.name;
}
