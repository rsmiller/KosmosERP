"use client"

import { KeyValueDto } from "@/models/key-value-models";
import { keyValueService } from "@/services/keyvalue-service";
import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";

import { useEffect, useState, forwardRef, useImperativeHandle } from "react"
import { Control, FieldError } from "react-hook-form";
import { useAsync } from "react-use";
import { useKeycloak } from '@react-keycloak/web';

export class FreightComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    disabled: boolean = false;
    onValidationChange?: (isValid: boolean) => void;
}

export interface FreightComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const FreightCombobox = forwardRef<FreightComboboxRef, FreightComboboxParams>(
    ({dbKey, onChange, control, name, error, disabled, onValidationChange}, ref) => {
    
    const { keycloak } = useKeycloak();

    const { contains } = useFilter({ sensitivity: "base" })

    const [freightOptions, setFreightOptions] = useState<KeyValueDto[]>([]);
    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isValid, setIsValid] = useState<boolean>(false);
    
    const { collection, filter, set } = useListCollection<KeyValueDto>({
        initialItems: freightOptions,
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
        if (dbKey && dbKey.length > 0 && freightOptions.length > 0) {
            const matchingItem = freightOptions.find(item => item.key === dbKey);

            //console.log('FRIEGHT: ', dbKey)

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
    }, [dbKey, freightOptions]);


    const fetchData = useAsync(async () => {
        await keyValueService.GetDtoByModule("2a2d1004-5283-40ef-96fd-8cc30c65cefa", keycloak?.token || "").then((response) =>
        {
            if (response.success && response.data !== undefined) {
                set(response.data);
                setFreightOptions(response.data);
            }
        });
    }, [set]);

    const inputValChange = (inputValue: any) =>
    {
        filter(inputValue);
    }

    const inputChange = (inputValue: any) =>
    {
        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = freightOptions.find(item => item.value === inputValue.value[0]);

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
            disabled={disabled}
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

export default FreightCombobox;