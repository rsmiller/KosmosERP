"use client"


import {
  Combobox,
  Portal,
  useFilter,
  useListCollection,
} from "@chakra-ui/react"
import { forwardRef, useEffect, useImperativeHandle, useState } from "react"
import { Control, Controller, FieldError } from "react-hook-form";
import { useAsync } from "react-use";
import { KeyValueDto } from "@/models/key-value-models";
import { keyValueService } from "@/services/keyvalue-service";
import { useKeycloak } from '@react-keycloak/web';

export class CustomerPaymentTermsComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (isValid: boolean) => void;
}

export interface CustomerPaymentTermsComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const CustomerPaymentTermsCombobox = forwardRef<CustomerPaymentTermsComboboxRef, CustomerPaymentTermsComboboxParams>(
    ({dbKey, onChange, control, name, error, onValidationChange}, ref) => {

    const { keycloak } = useKeycloak();

    const { contains } = useFilter({ sensitivity: "base" })

    const [paymentTerms, setPaymentTerms] = useState<KeyValueDto[]>([]);
    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isValid, setIsValid] = useState<boolean>(false);
    
    const { collection, filter, set } = useListCollection<KeyValueDto>({
        initialItems: paymentTerms,
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
        if (dbKey && dbKey.length > 0 && paymentTerms.length > 0) {
            const matchingItem = paymentTerms.find(item => item.key === dbKey[0]);

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
    }, [dbKey, paymentTerms]);



    const fetchData = useAsync(async () => {
        await keyValueService.GetDtoByModule("93bf02ec-5578-4aa4-a45b-f82962adf4bd", keycloak?.token || "").then((response) =>
        {
            if (response.success && response.data !== undefined) {
                set(response.data);
                setPaymentTerms(response.data);
            }
        });
    }, [set]);


    //useEffect(() => {
    //    keyValueService.find("93bf02ec-5578-4aa4-a45b-f82962adf4bd").then((response) =>
    //    {
     //       if (response.success && response.data !== undefined) {
    //            set(response.data);
     //           setPaymentTerms(response.data);
    //        }
    //    });
    //});

    const inputValChange = (inputValue: any) =>
    {
        filter(inputValue);
    }

    const inputChange = (inputValue: any) =>
    {
        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = paymentTerms.find(item => item.value === inputValue.value[0]);

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
    );

    // If control and name are provided, wrap with Controller for React Hook Form integration
    if (control && name) {
        return (
            <Controller
                name={name}
                control={control}
                rules={{ required: "Opportunity stage is required" }}
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
});

export default CustomerPaymentTermsCombobox;