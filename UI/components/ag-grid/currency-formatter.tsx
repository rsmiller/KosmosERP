import {   ValueFormatterParams } from "ag-grid-community";

export function CurrencyFormatter(params: ValueFormatterParams)
{
    return "$" + Math.floor(params.value).toFixed(2).toLocaleString();
}
