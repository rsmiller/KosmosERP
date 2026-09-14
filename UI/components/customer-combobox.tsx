"use client"

import { CustomerDto, CustomerFindCommand, CustomerListDto } from "@/models/customer-models";
import { customerService } from "@/services/customer-service";
import {
    Combobox,
    Portal,
    useListCollection,
} from "@chakra-ui/react"
import { useEffect, useState, useImperativeHandle, forwardRef } from "react"
import { useKeycloak } from '@react-keycloak/web';
import { Controller, Control, FieldError } from "react-hook-form";
import SessionStorage from "./session-storage";

export class CustomerComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    disabled: boolean = false;
    onValidationChange?: (isValid: boolean) => void;
}

export interface CustomerComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const CustomerCombobox = forwardRef<CustomerComboboxRef, CustomerComboboxParams>(
    ({dbKey, onChange, control, name, error, disabled, onValidationChange}, ref) => {
        const userId = SessionStorage.getUserId();
        const sessionId = SessionStorage.getSession();
        const { keycloak } = useKeycloak();

        const [customers, setCustomers] = useState<CustomerListDto[]>([]);
        const [loading, setLoading] = useState(false);
        const [selectedValue, setSelectedValue] = useState<string>();
        const [selectedItem, setSelectedItem] = useState<any[]>([]);
        const [isValid, setIsValid] = useState<boolean>(false);

        const { collection, set } = useListCollection<CustomerListDto>({
            initialItems: customers,
            itemToString: (item) => item.id.toString(),
            itemToValue: (item) => item.customer_name ? item.customer_name : "-- ERROR --",
        })

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
            if (dbKey) {
                fetchCustomer(dbKey).then( (response) => {
                    if(response != undefined)
                    {
                        setCustomers([response]);
                        set([response]);
                        setSelectedValue(response.customer_name);
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

        const fetchCustomer = async (id: number): Promise<CustomerDto | undefined> => {
            try {
                setLoading(true);
                const response = await customerService.get(id, keycloak?.token || "");
                
                if (response.success && response.data !== undefined) 
                {
                    setLoading(false);
                    return response.data;
                }

                return undefined;
            } catch (error) {
                console.error("Error fetching customers:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const fetchCustomers = async (searchTerm: string): Promise<CustomerListDto[] | undefined> => {
            try {
                setLoading(true);
                let findCommand = new CustomerFindCommand();
                findCommand.wildcard = searchTerm;
                
                const response = await customerService.find(findCommand, keycloak?.token || "", 1, 20, "customer_name-asc");
                
                if (response.success && response.data !== undefined) {
                    set(response.data);
                    setCustomers(response.data);
                    return response.data;
                }
                return undefined;
            } catch (error) {
                console.error("Error fetching customers:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const inputValChange = async (inputValue: string) => {
            // Only search if input is empty or has 3+ characters
            if (inputValue.length === 0 || inputValue.length >= 3) {
                await fetchCustomers(inputValue);
            }
        }

        const inputChange = (inputValue: any) => {

            if(inputValue.value != undefined && inputValue.value.length > 0)
            {
                const selectedItem = customers.find(item => item.customer_name === inputValue.value[0]);

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
                    <Combobox.Empty>
                        {loading ? "Loading..." : "No items found"}
                    </Combobox.Empty>
                    {collection.items.map((item) => (
                        <Combobox.Item item={item} key={item.id}>
                        {item.customer_name}
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
                    rules={{ required: "Customer is required" }}
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

CustomerCombobox.displayName = 'CustomerCombobox';

export default CustomerCombobox;