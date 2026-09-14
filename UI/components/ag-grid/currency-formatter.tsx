import {   ValueFormatterParams } from "ag-grid-community";

export function CurrencyFormatter(params: ValueFormatterParams)
{
    let val = Number(params.value).toFixed(2).toLocaleString();

    if(val == 'NaN')
    {
        return "$0"
    }
    return "$" + val;
}
