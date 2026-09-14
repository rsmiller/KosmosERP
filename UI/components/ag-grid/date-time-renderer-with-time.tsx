import { ValueFormatterParams } from "ag-grid-community";

export function DateTimeRenderWithTime (params: ValueFormatterParams)
{
    if(params.value == undefined || params.value == null || params.value == "")
    {
        return "";
    }
    
    const dateParts = params.value.split("-");
    const year = dateParts[0];
    const month = dateParts[1];
    const day = dateParts[2].split("T")[0];
    const time = dateParts[2].split("T")[1];

    const dateString = `${month}/${day}/${year} ${time}`;

    return new Date(dateString).toLocaleString();
}