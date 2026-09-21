"use client"

import { KeyValueDto } from "@/models/key-value-models";
import { keyValueService } from "@/services/keyvalue-service";
import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";
import { useAuth } from '@/lib/auth/auth-context';

import { useEffect, useState, forwardRef, useImperativeHandle } from "react"
import { Control, FieldError } from "react-hook-form";
import { useAsync } from "react-use";


export class TransactionTypeComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (isValid: boolean) => void;
}

export interface TransactionTypeComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const TransactionTypeCombobox = forwardRef<TransactionTypeComboboxRef, TransactionTypeComboboxParams>(
    ({dbKey, onChange, control, name, error, onValidationChange}, ref) => {

    const { contains } = useFilter({ sensitivity: "base" })
    const auth = useAuth();

    const [stages, setStages] = useState<KeyValueDto[]>([]);
    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isValid, setIsValid] = useState<boolean>(false);
    
    const { collection, filter, set } = useListCollection<KeyValueDto>({
        initialItems: stages,
        filter: contains,
        itemToString: (item) => item.key,
        itemToValue: (item) => item.value,
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
            const matchingItem = stages.find(item => item.key === dbKey[0]);

            if (matchingItem) {
                setSelectedValue(matchingItem.value);
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


    const fetchData = useAsync(async () => {
        await keyValueService.GetDtoByModule("416786e0-47b3-440a-90da-b7036d72b1f7", auth.token || "").then((response) =>
        {
            if (response.success && response.data !== undefined) {
                set(response.data);
                setStages(response.data);
            }
        });
    }, [set, auth.token]);

    const inputValChange = (inputValue: any) =>
    {
        filter(inputValue);
    }

    const inputChange = (inputValue: any) =>
    {
        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = stages.find(item => item.value === inputValue.value[0]);

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
                        <Combobox.Item item={item} key={item.key}>
                        {item.value}
                        <Combobox.ItemIndicator />
                        </Combobox.Item>
                    ))}
                    </Combobox.Content>
                </Combobox.Positioner>
            </Portal>
        </Combobox.Root>
    )
});

export default TransactionTypeCombobox;
