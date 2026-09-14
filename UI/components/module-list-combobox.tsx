"use client"

import { KeyValueDto, ModuleObjectDto } from "@/models/key-value-models";
import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";

import { useEffect, useState, forwardRef, useImperativeHandle } from "react"
import { Control, FieldError } from "react-hook-form";


export class ModuleListComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    dataSet?: ModuleObjectDto[];
    onValidationChange?: (isValid: boolean) => void;
}

export interface ModuleListComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const ModuleListCombobox = forwardRef<ModuleListComboboxRef, ModuleListComboboxParams>(
    ({dbKey, onChange, control, name, error, dataSet, onValidationChange}, ref) => {

    const { contains } = useFilter({ sensitivity: "base" })

    const [stages, setStages] = useState<ModuleObjectDto[]>([]);
    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isValid, setIsValid] = useState<boolean>(false);
    
    const { collection, filter, set } = useListCollection<ModuleObjectDto>({
        initialItems: stages,
        filter: contains,
        itemToString: (item) => item.module_id,
        itemToValue: (item) => item.module_name,
    });

    // Expose methods to parent component
    useImperativeHandle(ref, () => ({
        isValid: () => isValid,
        getValue: () => selectedItem[0] || null,
        clear: () => {
            setSelectedValue('');
            setSelectedItem([]);
            onChange(null);
        }
    }), [isValid, selectedItem, onChange]);

    // Update validation state when selectedItem changes
    useEffect(() => {
        const valid = selectedItem.length > 0 && selectedItem[0] !== null;
        setIsValid(valid);
        if (onValidationChange) {
            onValidationChange(valid);
        }
    }, [selectedItem, onValidationChange]);

    useEffect(() => {
        if (dbKey && dbKey.length > 0 && stages.length > 0) {
            const matchingItem = stages.find(item => item.module_id === dbKey[0]);

            if (matchingItem) {
                setSelectedValue(matchingItem.module_name);
                setSelectedItem([matchingItem]);
                setIsValid(true);

                if (onValidationChange) {
                    onValidationChange(true);
                }

            } else {
                setSelectedValue('');
                setSelectedItem([]);
                setIsValid(false);

                if (onValidationChange) {
                    onValidationChange(false);
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
    }, [dbKey, stages]);


    useEffect(() => {
        //console.log(dataSet);
        if(dataSet != undefined && set.length === 0)
        {
            setStages(dataSet);
            set(dataSet);
        }

    }, [dataSet]);

    const inputValChange = (inputValue: any) =>
    {
        filter(inputValue);
    }

    const inputChange = (inputValue: any) =>
    {
        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = stages.find(item => item.module_name === inputValue.value[0]);

            if (selectedItem) {
                setSelectedValue(inputValue.value[0]);
                setSelectedItem([selectedItem]);
                onChange(selectedItem);
                setIsValid(false);

                if (onValidationChange) {
                    onValidationChange(false);
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
        <Combobox.Root
            collection={collection}
            inputValue={selectedValue}
            value={selectedItem}
            onValueChange={(e) => inputChange(e)}
            onInputValueChange={(e) => inputValChange(e.inputValue)}
            allowCustomValue={false}
            required={true}
            width="100%"
        >
            <Combobox.Control>
            <Combobox.Input placeholder="Type to search" />
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
                        <Combobox.Item item={item} key={item.module_id}>
                        {item.module_name}
                        <Combobox.ItemIndicator />
                        </Combobox.Item>
                    ))}
                    </Combobox.Content>
                </Combobox.Positioner>
            </Portal>
        </Combobox.Root>
    )
});

export default ModuleListCombobox;
