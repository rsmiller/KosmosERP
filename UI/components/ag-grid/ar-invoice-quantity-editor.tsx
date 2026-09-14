import { Field, NumberInput } from "@chakra-ui/react";
import { CustomCellEditorProps } from "ag-grid-react";
import { useEffect, useState } from "react";

const ARInvoiceQuantityEditor = (
    ({ data, value, onValueChange, eventKey, stopEditing }: CustomCellEditorProps) => {
    
    const [maxVal, setMaxVal] = useState<number>(0);

    useEffect(() => {
        //console.log(data);
        setMaxVal(Number(data.max_invoice_qty));
    }, [data]);
    
    const updateValue = (val: number) => {
        //console.log(val)
        
        if(Number.isNaN(val) || val < 0)
        {
            onValueChange(0);
        }

        if(val > maxVal)
        {
            onValueChange(maxVal);
        }
        else
        {
            onValueChange(val);
        }
    };

    return (
        <Field.Root>
            <NumberInput.Root
                onValueChange={(event: any) => updateValue(event.valueAsNumber)}
                max={maxVal}
                min={0}
            >
                <NumberInput.Control/>
                <NumberInput.Input defaultValue={value}/>
            </NumberInput.Root>
        </Field.Root>
    );
});

export default ARInvoiceQuantityEditor;