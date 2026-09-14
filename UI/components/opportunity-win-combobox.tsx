"use client"

import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";

import { useEffect, useState, useImperativeHandle, forwardRef } from "react";
import { Controller, Control, FieldError } from "react-hook-form";


export class OpportunityWinComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    disabled?: boolean = false;
    onValidationChange?: (isValid: boolean) => void;
}

export interface OpportunityWinComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const OpportunityWinCombobox = forwardRef<OpportunityWinComboboxRef, OpportunityWinComboboxParams>(
    ({dbKey, onChange, control, name, error, disabled, onValidationChange}, ref) => {
        const { contains } = useFilter({ sensitivity: "base" })

        const [selectedValue, setSelectedValue] = useState<string>();
        const [selectedItem, setSelectedItem] = useState<any[]>([]);
        const [isValid, setIsValid] = useState<boolean>(false);
        
        const win_chance = [
            { label: "0%", value: "0" },
            { label: "10%", value: "10" },
            { label: "20%", value: "20" },
            { label: "30%", value: "30" },
            { label: "40%", value: "40" },
            { label: "50%", value: "50" },
            { label: "60%", value: "60" },
            { label: "70%", value: "70" },
            { label: "80%", value: "80" },
            { label: "90%", value: "90" },
            { label: "100%", value: "100" },
        ]

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
            if (dbKey && win_chance.length > 0) {
                const matchingItem = win_chance.find(item => item.value === String(dbKey));

                if (matchingItem) {
                    setSelectedValue(matchingItem.label);
                    setSelectedItem([matchingItem]);
                    setIsValid(true);
                } else {
                    setSelectedValue('');
                    setSelectedItem([]);
                    setIsValid(false);
                }
            } else {
                setSelectedValue('');
                setSelectedItem([]);
                setIsValid(false);
            }
        }, [dbKey]);

        const { collection, filter, set } = useListCollection({
            initialItems: win_chance,
            filter: contains,
            itemToString: (item) => item.label,
            itemToValue: (item) => item.value,
        })

        const inputValChange = (inputValue: any) =>
        {
            filter(inputValue);
        }

        const inputChange = (inputValue: any) =>
        {
            if(inputValue.value != undefined && inputValue.value.length > 0)
            {
                const selectedItem = win_chance.find(item => item.value === String(inputValue.value));
                
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

        const comboboxComponent = (
            <Combobox.Root
                collection={collection}
                inputValue={selectedValue}
                value={selectedItem}
                onValueChange={(e) => inputChange(e)}
                onInputValueChange={(e) => inputValChange(e.inputValue)}
                allowCustomValue={false}
                required={true}
                width="100%"
                disabled={disabled}
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
                            <Combobox.Item item={item} key={item.value}>
                            {item.label}
                            <Combobox.ItemIndicator />
                            </Combobox.Item>
                        ))}
                        </Combobox.Content>
                    </Combobox.Positioner>
                </Portal>
            </Combobox.Root>
        );

        // If control and name are provided, wrap with Controller for React Hook Form integration
        if (control && name) {
            return (
                <Controller
                    name={name}
                    control={control}
                    rules={{ required: "Win chance is required" }}
                    render={({ field }) => (
                        <div className={"full-width"}>
                            {comboboxComponent}
                            {error && (
                                <div style={{ color: 'red', fontSize: '0.875rem', marginTop: '0.25rem' }}>
                                    {error.message}
                                </div>
                            )}
                        </div>
                    )}
                />
            );
        }

        // Return the component without Controller wrapper
        return comboboxComponent;
    }
);

OpportunityWinCombobox.displayName = 'OpportunityWinCombobox';

export default OpportunityWinCombobox;