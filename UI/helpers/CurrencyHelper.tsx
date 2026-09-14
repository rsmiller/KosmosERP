
export function CurrencyHelper(value: number | undefined)
{
    if(value == undefined)
    {
        return "$0"
    }

    let val = Number(value).toFixed(2).toLocaleString();

    if(val == 'NaN')
    {
        return "$0"
    }
    return "$" + val;
}