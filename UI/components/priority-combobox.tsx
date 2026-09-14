"use client"

import {
  Combobox,
  Portal,
  useFilter,
  useListCollection,
  HStack
} from "@chakra-ui/react"
import { forwardRef, useEffect, useImperativeHandle, useState } from "react"
import { Control, FieldError } from "react-hook-form";



export class PriorityComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    title: string = "";
    disabled: boolean = false;
    onValidationChange?: (isValid: boolean) => void;
}

export interface PriorityComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}


const PriorityCombobox = forwardRef<PriorityComboboxRef, PriorityComboboxParams>(
    ({dbKey, onChange, control, name, error, title, disabled, onValidationChange}, ref) => {
    const { contains } = useFilter({ sensitivity: "base" })

    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isValid, setIsValid] = useState<boolean>(false);

    const thevalues = [
        { label: "Low", value: "1" },
        { label: "Medium", value: "2" },
        { label: "High", value: "3" },
        { label: "Urgent", value: "4" },
    ]

    // Expose methods to parent component
    useImperativeHandle(ref, () => ({
        isValid: () => isValid,
        getValue: () => selectedItem[0] || null,
        clear: () => {
            setSelectedValue('');
            setSelectedItem([]);
            onChange(null);
            setIsValid(false);
        }
    }), [isValid, selectedItem, onChange]);

    useEffect(() => {
        const valid = selectedItem.length > 0 && selectedItem[0] !== null;
        setIsValid(valid);
        if (onValidationChange) {
            onValidationChange(valid);
        }
    }, [selectedItem, onValidationChange]);

    useEffect(() => {
        if (dbKey) {
            const selectedItem = thevalues.find(item => item.value === dbKey);

            if (selectedItem) 
            {
                setSelectedValue(selectedItem.label);
                setSelectedItem([dbKey]);
                setIsValid(true);

                if (onValidationChange) {
                    onValidationChange(true);
                }
            }
        } else {
            setSelectedValue('');
            setSelectedItem([]);
            setIsValid(false);

            if (onValidationChange) {
                onValidationChange(false);
            }
        }
    }, [dbKey]);


    const { collection, filter } = useListCollection({
        initialItems: thevalues,
        filter: contains,
    })

    const inputChange = (inputValue: any) => {

        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = thevalues.find(item => item.value === inputValue.value[0]);

            if (selectedItem) {
                setSelectedValue(selectedItem.label);
                setSelectedItem([selectedItem]);
                onChange(selectedItem);
                setIsValid(true);

                if (onValidationChange) {
                    onValidationChange(true);
                }

            } else {
                setSelectedValue('');
                setSelectedItem([]);
                onChange(null);
                setIsValid(false);

                if (onValidationChange) {
                    onValidationChange(false);
                }
            }
        }
        else
        {
            setSelectedValue('');
            setSelectedItem([]);
            onChange(null);
            setIsValid(false);

            if (onValidationChange) {
                onValidationChange(false);
            }
        }
    }


    return (
        <HStack direction="row" gap="4">
            <Combobox.Root
                collection={collection}
                inputValue={selectedValue}
                value={selectedItem}
                onValueChange={(e) => inputChange(e)}
                allowCustomValue={false}
                required={true}
                width="100%"
                disabled={disabled}
            >
                <Combobox.Label>{title}</Combobox.Label>
                <Combobox.Control>
                <Combobox.Input placeholder="Choose Type" />
                <Combobox.IndicatorGroup>
                    <Combobox.ClearTrigger />
                    <Combobox.Trigger />
                </Combobox.IndicatorGroup>
                </Combobox.Control>
                <Portal>
                <Combobox.Positioner>
                    <Combobox.Content>
                    <Combobox.Empty>No items found</Combobox.Empty>
                    {collection.items.map((item) => (
                        <Combobox.Item item={item} key={item.value}>
                        {item.label}
                        <Combobox.ItemIndicator />
                        </Combobox.Item>
                    ))}
                    </Combobox.Content>
                </Combobox.Positioner>
                </Portal>
            </Combobox.Root>
        </HStack>
    )
});

export default PriorityCombobox;