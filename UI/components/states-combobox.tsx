"use client"

import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";

import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import { useKeycloak } from '@react-keycloak/web';
import { Control, FieldError } from "react-hook-form";
import SessionStorage from "./session-storage";
import { StateDto, StateFindCommand, StateListDto } from "@/models/country-models";
import { stateService } from "@/services/state-service";


export class StatesComboboxParams
{
    dbKey: any;
    country_id: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (isValid: boolean) => void;
}

export interface StatesComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
    rebuild: (countryId: number) => void;
}


const StatesCombobox = forwardRef<StatesComboboxRef, StatesComboboxParams>(
    ({dbKey, country_id, onChange, control, name, error, onValidationChange}, ref) => {
    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();
    const { keycloak } = useKeycloak();

    const { contains } = useFilter({ sensitivity: "base" })
    const [states, setStates] = useState<StateListDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isValid, setIsValid] = useState<boolean>(false);

    const { collection, filter, set } = useListCollection({
        initialItems: states,
        filter: contains,
        itemToString: (item) => item.state_name ? item.state_name : '-01',
        itemToValue: (item) => item.iso2?.toString() ? item.iso2.toString() : "-1",
    })


    // Expose methods to parent component
    useImperativeHandle(ref, () => ({
        isValid: () => isValid,
        getValue: () => selectedItem[0] || null,
        clear: () => {
            setStates([]);
            set([]);
            setSelectedValue('');
            setSelectedItem([]);
            onChange(null);
            setIsValid(false);
        },
        rebuild: (country_id: number) => fetchStatesByCountry(country_id)
    }), [isValid, selectedItem, onChange]);

    // Update validation state when selectedItem changes
    useEffect(() => {
        const valid = selectedItem.length > 0 && selectedItem[0] !== null;
        //setIsValid(valid);
        if (onValidationChange) {
            onValidationChange(valid);
        }
    }, [selectedItem]);

    useEffect(() => {
        if (dbKey) {
            console.log('StatesCombobox dbKey: ', dbKey);
            console.log('StatesCombobox country_id: ', country_id)
            if(country_id && country_id != undefined)
            {
                fetchState(country_id, dbKey).then( (response) => {
                    if(response != undefined)
                    {
                        setStates([response]);
                        set([response]);
                        setSelectedValue(response.state_name);
                        setSelectedItem([response]);
                    } else {
                        setSelectedValue('');
                        setSelectedItem([]);
                    }
                });
            }
        } else {
            setSelectedValue('');
            setSelectedItem([]);
        }
    }, [dbKey]);

    useEffect(() => {
        if (country_id && country_id != undefined) {
            //console.log('StatesCombobox country_id: ', country_id)
            fetchStatesByCountry(country_id).then( (response) =>
            {
                if(response != undefined)
                {
                    setStates(response);
                    set(response);

                    if(country_id && country_id != undefined && dbKey)
                    {
                        fetchState(country_id, dbKey);
                    }
                    else
                    {
                        fetchStatesByCountry(country_id);
                    }
                } else {
                    setStates([]);
                    set([]);
                }
            });
        }
    }, [country_id]);


    const fetchState = async (country_id: number, iso: string): Promise<StateDto | undefined> => {
        try {
            setLoading(true);
            const response = await stateService.getByISOAsync(country_id, iso, keycloak?.token || "");
            
            if (response.success && response.data !== undefined) 
            {
                setLoading(false);
                set([response.data]);
                setStates([response.data]);
                setSelectedValue(response.data.state_name);
                setSelectedItem([response.data]);
                setIsValid(true);
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
    
    const fetchStates = async (searchTerm: string): Promise<StateListDto[] | undefined> => {
        if(searchTerm == "")
        {
            return;
        }

        try {
            setLoading(true);
            let findCommand = new StateFindCommand();
            findCommand.wildcard = searchTerm;
            
            const response = await stateService.find(findCommand, keycloak?.token || "", 1, 200, "state_name-asc");
            
            if (response.success && response.data !== undefined) {
                set(response.data);
                setStates(response.data);
                return response.data;
            }
            return undefined;
        } catch (error) {
            console.error("Error fetching countries:", error);
            return undefined;
        } finally {
            setLoading(false);
        }
    }

    const fetchStatesByCountry = async (country_id: number): Promise<StateListDto[] | undefined> => {
        
        if(country_id == null)
        {
            return;
        }

        //console.log("------------------------- FIRED")

        try {
            setLoading(true);
            let findCommand = new StateFindCommand();
            findCommand.country_id = country_id;
            
            const response = await stateService.find(findCommand, keycloak?.token || "", 1, 100, "state_name-asc");
            
            if (response.success && response.data !== undefined) {
                set(response.data);
                setStates(response.data);
                return response.data;
            }
            return undefined;
        } catch (error) {
            console.error("Error fetching countries:", error);
            return undefined;
        } finally {
            setLoading(false);
        }
    }
    
    const inputValChange = async (inputValue: string) => {
        // Only search if input is empty or has 3+ characters
        if (inputValue.length === 1 || inputValue.length >= 3) {
            await fetchStates(inputValue);
        }
    }

    const inputChange = (inputValue: any) => {
        
        if(inputValue == null)
        {
            setSelectedValue('');
            setSelectedItem([]);
            onChange(null);
            setIsValid(false);
            return;
        }

        //console.log("inputChange: ", inputValue)
        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = states.find(item => item.iso2 === inputValue.value[0]);

            if (selectedItem) {
                setSelectedValue(selectedItem.state_name);
                setSelectedItem([selectedItem]);
                onChange(selectedItem);
                setIsValid(true);
            } else {
                setSelectedValue('');
                setSelectedItem([]);
                onChange(null);
                setIsValid(false);
            }
        }
        else
        {
            setSelectedValue('');
            setSelectedItem([]);
            onChange(null);
            setIsValid(false);
        }
    }

    const itemSelect = (item: any) =>
    {
        if(item && item.value.length > 0)
        {

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
            onSelect={itemSelect}
        >
            <Combobox.Label>State</Combobox.Label>
            <Combobox.Control >
            <Combobox.Input placeholder="Type to search" />
            <Combobox.IndicatorGroup>
                <Combobox.ClearTrigger />
                <Combobox.Trigger />
            </Combobox.IndicatorGroup>
            </Combobox.Control>
            <Portal>
                <Combobox.Positioner>
                    <Combobox.Content >
                    <Combobox.Empty>No items found</Combobox.Empty>
                    {collection.items.map((item) => (
                        <Combobox.Item item={item} key={item.id}>
                        {item.state_name}
                        <Combobox.ItemIndicator />
                        </Combobox.Item>
                    ))}
                    </Combobox.Content>
                </Combobox.Positioner>
            </Portal>
        </Combobox.Root>
    )
});

export default StatesCombobox;