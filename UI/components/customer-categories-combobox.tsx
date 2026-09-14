"use client"


import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";

import { useEffect, useState, forwardRef, useImperativeHandle } from "react"
import { Control, FieldError } from "react-hook-form";


export class CustomerCategoriesComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (isValid: boolean) => void;
}

export interface CustomerCategoriesComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const CustomerCategoriesCombobox = forwardRef<CustomerCategoriesComboboxRef, CustomerCategoriesComboboxParams>(
    ({dbKey, onChange, control, name, error, onValidationChange}, ref) => {

    const { contains } = useFilter({ sensitivity: "base" })

    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isValid, setIsValid] = useState<boolean>(false);
    

    const categories = [
        { label: "Electronics", value: "Electronics" },
        { label: "Retail", value: "Retail" },
        { label: "Agriculture", value: "Agriculture" },
        { label: "Healthcare", value: "Healthcare" },
        { label: "Wholesale", value: "Wholesale" },
        { label: "Construction", value: "Construction" },
        { label: "Technology", value: "Technology" },
        { label: "Automotive", value: "Automotive" },
        { label: "Finance", value: "Finance" },
        { label: "Education", value: "Education" },
        { label: "Hospitality", value: "Hospitality" },
        { label: "Manufacturing", value: "Manufacturing" },
        { label: "Transportation", value: "Transportation" },
        { label: "Energy", value: "Energy" },
        { label: "Real Estate", value: "Real Estate" },
        { label: "Food & Beverage", value: "Food & Beverage" },
        { label: "Telecommunications", value: "Telecommunications" },
        { label: "Pharmaceuticals", value: "Pharmaceuticals" },
        { label: "Aerospace", value: "Aerospace" },
        { label: "Media & Entertainment", value: "Media & Entertainment" },
        { label: "Tourism", value: "Tourism" },
        { label: "Logistics", value: "Logistics" },
        { label: "Insurance", value: "Insurance" },
        { label: "Fashion & Apparel", value: "Fashion & Apparel" },
        { label: "Chemicals", value: "Chemicals" },
        { label: "Mining", value: "Mining" },
        { label: "Utilities", value: "Utilities" },
        { label: "Environmental Services", value: "Environmental Services" },
        { label: "Legal Services", value: "Legal Services" },
        { label: "Consulting", value: "Consulting" },
        { label: "Advertising & Marketing", value: "Advertising & Marketing" },
        { label: "Biotechnology", value: "Biotechnology" },
        { label: "E-commerce", value: "E-commerce" },
        { label: "Nonprofit", value: "Nonprofit" },
        { label: "Government", value: "Government" },
        { label: "Sports & Fitness", value: "Sports & Fitness" },
        { label: "Transportation & Logistics", value: "Transportation & Logistics" },
        { label: "Financial Services", value: "Financial Services" },
        { label: "Banking", value: "Banking" },
        { label: "Nonprofit & NGOs", value: "Nonprofit & NGOs" },
        { label: "Government & Public Sector", value: "Government & Public Sector" },
        { label: "Textiles & Apparel", value: "Textiles & Apparel" },
        { label: "Printing & Publishing", value: "Printing & Publishing" },
        { label: "Marine & Shipping", value: "Marine & Shipping" },
    ];


    const { collection, filter, set } = useListCollection<any>({
        initialItems: categories,
        filter: contains,
        itemToString: (item) => item.value ? item.value : '-01',
        itemToValue: (item) => item.label ? item.label : "-- ERROR --",
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
        if (dbKey && dbKey.length > 0 && categories.length > 0) {
            
            const matchingItem = categories.find(item => item.value === dbKey);

            //console.log("dbKey: ", dbKey);
            //console.log("matchingItem: ", matchingItem);

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
    }, [dbKey]);


    const inputValChange = (inputValue: any) =>
    {
        filter(inputValue);
    }

    const inputChange = (inputValue: any) =>
    {
        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = categories.find(item => item.value === inputValue.value[0]);

            //console.log("inputValue: ", inputValue);
            //console.log("selectedItem: ", selectedItem);

            if (selectedItem) {
                setSelectedValue(selectedItem.value);
                setSelectedItem([selectedItem]);
                onChange(selectedItem);
                setIsValid(true);

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
                        <Combobox.Item item={item} key={item.value}>
                        {item.label}
                        <Combobox.ItemIndicator />
                        </Combobox.Item>
                    ))}
                    </Combobox.Content>
                </Combobox.Positioner>
            </Portal>
        </Combobox.Root>
    )
});

export default CustomerCategoriesCombobox;
