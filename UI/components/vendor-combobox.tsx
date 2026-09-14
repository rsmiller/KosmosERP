"use client"

import { VendorDto, VendorFindCommand, VendorListDto } from "@/models/vendor-models";
import { vendorService } from "@/services/vendor-service";
import {
    Combobox,
    Portal,
    useListCollection,
} from "@chakra-ui/react"
import { useEffect, useState, useImperativeHandle, forwardRef } from "react"
import { useKeycloak } from '@react-keycloak/web';
import { Controller, Control, FieldError } from "react-hook-form";


export class VendorComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    disabled: boolean = false;
    onValidationChange?: (isValid: boolean) => void;
    required?: boolean = true;
}

export interface VendorComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const VendorCombobox = forwardRef<VendorComboboxRef, VendorComboboxParams>(
    ({dbKey, onChange, control, name, error, disabled, onValidationChange, required}, ref) => {
        const { keycloak } = useKeycloak();

        const [vendors, setVendors] = useState<VendorListDto[]>([]);
        const [loading, setLoading] = useState(false);
        const [selectedValue, setSelectedValue] = useState<string>();
        const [selectedItem, setSelectedItem] = useState<any[]>([]);
        const [isValid, setIsValid] = useState<boolean>(false);

        const { collection, set } = useListCollection<VendorListDto>({
            initialItems: vendors,
            itemToString: (item) => item.id.toString(),
            itemToValue: (item) => item.vendor_name ? item.vendor_name : "-- ERROR --",
        })

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

        // Update validation state when selectedItem changes
        useEffect(() => {
            const valid = selectedItem.length > 0 && selectedItem[0] !== null;
            setIsValid(valid);
            if (onValidationChange) {
                onValidationChange(valid);
            }
        }, [selectedItem, onValidationChange]);

        useEffect(() => {
            if (dbKey) {
                fetchVendor(dbKey).then( (response) => {
                    if(response != undefined)
                    {
                        setVendors([response]);
                        set([response]);
                        setSelectedValue(response.vendor_name);
                        setSelectedItem([response]);
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
                });
            } else {
                setSelectedValue('');
                setSelectedItem([]);
                setIsValid(false);

                if (onValidationChange) {
                    onValidationChange(false);
                }
            }
        }, [dbKey]);

        const fetchVendor = async (id: number): Promise<VendorDto | undefined> => {
            try {
                setLoading(true);
                const response = await vendorService.get(id, keycloak?.token || "");
                
                if (response.success && response.data !== undefined) 
                {
                    setLoading(false);
                    return response.data;
                }

                return undefined;
            } catch (error) {
                console.error("Error fetching vendors:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const fetchVendors = async (searchTerm: string): Promise<VendorListDto[] | undefined> => {
            try {
                setLoading(true);
                let findCommand = new VendorFindCommand();
                findCommand.wildcard = searchTerm;
                
                const response = await vendorService.find(findCommand, keycloak?.token || "", 1, 20, "vendor_name-asc");
                
                if (response.success && response.data !== undefined) {
                    set(response.data);
                    setVendors(response.data);
                    return response.data;
                }
                return undefined;
            } catch (error) {
                console.error("Error fetching vendors:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const inputValChange = async (inputValue: string) => {
            // Only search if input is empty or has 3+ characters
            if (inputValue.length === 0 || inputValue.length >= 3) {
                await fetchVendors(inputValue);
            }
        }

        const inputChange = (inputValue: any) => {

            if(inputValue.value != undefined && inputValue.value.length > 0)
            {
                const selectedItem = vendors.find(item => item.vendor_name === inputValue.value[0]);

                if (selectedItem) {
                    setSelectedValue(inputValue.value[0]);
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
                required={required}
                width="100%"
                disabled={disabled}
                invalid={!isValid}
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
                    <Combobox.Empty>
                        {loading ? "Loading..." : "No items found"}
                    </Combobox.Empty>
                    {collection.items.map((item) => (
                        <Combobox.Item item={item} key={item.id}>
                        {item.vendor_name}
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
                    rules={{ required: "Vendor is required" }}
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

VendorCombobox.displayName = 'VendorCombobox';

export default VendorCombobox;