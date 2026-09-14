"use client"

import { AddressCreateCommand, AddressDto, AddressFindCommand, AddressListDto } from "@/models/address-models";
import { addressService } from "@/services/address-service";
import { useKeycloak } from '@react-keycloak/web';
import {
  Combobox,
  Portal,
  useListCollection,
  HStack,
  Button
} from "@chakra-ui/react"
import { useEffect, useState, useImperativeHandle, forwardRef, useRef } from "react"
import { Controller, Control, FieldError } from "react-hook-form";
import SessionStorage from "./session-storage";
import AddAddressDialog, { AddAddressDialogRef } from "./dialogs/new-address-dialog";

export class AddressSelectorComboboxParams
{
    dbKey: any
    customer_id: number = 0;
    hideAddBtn: boolean = false;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (isValid: boolean) => void;
    title?: string;
    disabled: boolean = false;
}

export interface AddressSelectorComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const AddressSelectorCombobox = forwardRef<AddressSelectorComboboxRef, AddressSelectorComboboxParams>(
    ({dbKey, customer_id, onChange, control, name, error, disabled, hideAddBtn, onValidationChange, title}, ref) => {
        const userId = SessionStorage.getUserId();
        const sessionId = SessionStorage.getSession();
        const { keycloak } = useKeycloak();

        const [addresses, setAddresses] = useState<AddressListDto[]>([]);
        const [loading, setLoading] = useState(false);
        const [selectedValue, setSelectedValue] = useState<string>();
        const [selectedItem, setSelectedItem] = useState<any[]>([]);
        const [isValid, setIsValid] = useState<boolean>(false);
        const [openAddressDialog, setOpenAddressDialog] = useState(false);
        
        const addAddressDialogRef = useRef<AddAddressDialogRef>(null);
        
        const { collection, set } = useListCollection<AddressListDto>({
            initialItems: addresses,
            itemToString: (item) => item.id.toString(),
            itemToValue: (item) => formatAddress(item),
        })

        // Helper function to format address for display
        const formatAddress = (address: AddressListDto): string => {
            if (!address) return "-- ERROR --";
            
            const parts = [
                address.street_address1,
                address.street_address2,
                address.city,
                address.state,
                address.postal_code
            ].filter(part => part && part.trim() !== '');
            
            return parts.join(", ") || "-- ERROR --";
        }

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
                fetchAddress(dbKey).then( (response) => {
                    if(response != undefined)
                    {
                        setAddresses([response]);
                        set([response]);
                        setSelectedValue(formatAddress(response));
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
                // If no value get some values to select
                fetchAddresses('');
            }
        }, [dbKey]);

        const fetchAddress = async (id: number): Promise<AddressDto | undefined> => {
            try {
                setLoading(true);
                const response = await addressService.get(id, keycloak?.token || "");
                
                if (response.success && response.data !== undefined) 
                {
                    setLoading(false);
                    return response.data;
                }

                return undefined;
            } catch (error) {
                console.error("Error fetching addresses:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const fetchAddresses = async (searchTerm: string): Promise<AddressListDto[] | undefined> => {
            try {
                setLoading(true);
                let findCommand = new AddressFindCommand();
                findCommand.wildcard = searchTerm;
                findCommand.customer_id = customer_id;
                //console.log(findCommand)
                const response = await addressService.find(findCommand, keycloak?.token || "", 1, 20, "street_address1-asc");
                
                if (response.success && response.data !== undefined) {
                    set(response.data);
                    setAddresses(response.data);
                    return response.data;
                }
                return undefined;
            } catch (error) {
                console.error("Error fetching addresses:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const inputValChange = async (inputValue: string) => {
            // Only search if input is empty or has 3+ characters
            if (inputValue.length === 1 || inputValue.length >= 3) {
                await fetchAddresses(inputValue);
            }
        }

        const inputChange = (inputValue: any) => {disabled
            if(inputValue.value != undefined && inputValue.value.length > 0)
            {
                const selectedItem = addresses.find(item => formatAddress(item) === inputValue.value[0]);

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

        const handleNewAddressClick = () => {
            // TODO: Implement new address creation
            //console.log("New address button clicked");
            setOpenAddressDialog(true);
        };

        const handleAddAddressSelect = async (command: AddressCreateCommand) =>
        {
            command.customer_id = customer_id;
            command.address_type_id = 2; // TODO: This needs to have a type selector in the form
            console.log(command);


            await addressService.create(command, keycloak?.token || "").then( async (response) => 
            {
                setOpenAddressDialog(false);

                if(response.success && response.data)
                {
                    await fetchAddress(response.data?.id);
                }
            });
        }

        const comboboxComponent = (
            <HStack direction="row" gap="4">
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
                    <Combobox.Label>{title}</Combobox.Label>
                    <Combobox.Control>
                    <Combobox.Input placeholder="Type to search" />
                    <Combobox.IndicatorGroup>
                        <Combobox.ClearTrigger />
                        <Combobox.Trigger />
                    </Combobox.IndicatorGroup>
                    </Combobox.Control>
                    <Portal>
                    <Combobox.Positioner style={{ zIndex: 9999 }}>
                        <Combobox.Content style={{ zIndex: 9999 }}>
                        <Combobox.Empty>
                            {loading ? "Loading..." : "No items found"}
                        </Combobox.Empty>
                        {collection.items.map((item) => (
                            <Combobox.Item item={item} key={item.id}>
                            {formatAddress(item)}
                            <Combobox.ItemIndicator />
                            </Combobox.Item>
                        ))}
                        </Combobox.Content>
                    </Combobox.Positioner>
                    </Portal>
                </Combobox.Root>
                <div style={{ marginTop: "22px"}} hidden={hideAddBtn}>
                    <Button type="button" colorPalette="blue" onClick={handleNewAddressClick} disabled={disabled}>New</Button>
                </div>
                <AddAddressDialog 
                    openDialog={openAddressDialog}
                    ref={addAddressDialogRef}
                    control={control}
                    name="add_dialog_address"
                    onChange={handleAddAddressSelect}
                />
            </HStack>
        );

        // If control and name are provided, wrap with Controller for React Hook Form integration
        if (control && name) {
            return (
                <Controller
                    name={name}
                    control={control}
                    rules={{ required: "Address is required" }}
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

AddressSelectorCombobox.displayName = 'AddressSelectorCombobox';

export default AddressSelectorCombobox;