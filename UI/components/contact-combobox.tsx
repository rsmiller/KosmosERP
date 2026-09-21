"use client"

import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";
import { useAuth } from '@/lib/auth/auth-context';

import { useEffect, useState, useImperativeHandle, forwardRef } from "react";
import { Controller, Control, FieldError } from "react-hook-form";
import SessionStorage from "./session-storage";
import { ContactDto, ContactFindCommand, ContactListDto } from "@/models/contact-models";
import { contactService } from "@/services/contact-service";


export class ContactComboboxParams
{
    dbKey: any;
    customerId: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    disabled?: boolean = false;
    onValidationChange?: (isValid: boolean) => void;
}

export interface ContactComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const ContactCombobox = forwardRef<ContactComboboxRef, ContactComboboxParams>(
    ({dbKey, customerId, onChange, control, name, error, disabled, onValidationChange}, ref) => {
        const { contains } = useFilter({ sensitivity: "base" })

        const userId = SessionStorage.getUserId();
        const sessionId = SessionStorage.getSession();
        const auth = useAuth();

        const [contacts, setContacts] = useState<ContactListDto[]>([]);
        const [loading, setLoading] = useState(false);
        const [selectedValue, setSelectedValue] = useState<string>();
        const [selectedItem, setSelectedItem] = useState<any[]>([]);
        const [isValid, setIsValid] = useState<boolean>(false);

        const { collection, filter, set } = useListCollection<ContactListDto>({
            initialItems: contacts,
            filter: contains,
            itemToString: (item) => item.id.toString(),
            itemToValue: (item) => item.first_name ? item.first_name + " " + item.last_name  : "-- ERROR --",
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
                fetchContact(dbKey).then( (response) => {
                    if(response != undefined)
                    {
                        setContacts([response]);
                        set([response]);
                        setSelectedValue(response.first_name + " " + response.last_name);
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

        const fetchContact = async (id: number): Promise<ContactDto | undefined> => {
            try {
                setLoading(true);
                const response = await contactService.get(id, auth.token || "");
                
                if (response.success && response.data !== undefined) 
                {
                    setLoading(false);
                    
                    return response.data;
                }

                return undefined;
            } catch (error) {
                console.error("Error fetching contacts:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const fetchContacts = async (searchTerm: string): Promise<ContactListDto[] | undefined> => {
            try {
                setLoading(true);
                let findCommand = new ContactFindCommand();
                findCommand.wildcard = searchTerm;
                findCommand.customer_id = Number(customerId);
                //console.log(findCommand);

                const response = await contactService.find(findCommand, auth.token || "", 1, 20, "first_name-asc");

                if (response.success && response.data !== undefined) {
                    set(response.data);
                    setContacts(response.data);
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
                await fetchContacts(inputValue);
            }
        }

        const inputChange = (inputValue: any) => {
            if(inputValue.value != undefined && inputValue.value.length > 0)
            {
                const selectedItem = contacts.find(item => (item.first_name + " " + item.last_name) === inputValue.value[0]);
                
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
                            {item.first_name} {item.last_name}
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
                    rules={{ required: "Contact is required" }}
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

ContactCombobox.displayName = 'ContactCombobox';

export default ContactCombobox;