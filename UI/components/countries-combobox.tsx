"use client"

import { CountryDto, CountryFindCommand, CountryListDto } from "@/models/country-models";
import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";

import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import { useKeycloak } from '@react-keycloak/web';
import SessionStorage from "./session-storage";
import { countryService } from "@/services/country-service";
import { Control, FieldError } from "react-hook-form";


export class CountriesComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (isValid: boolean) => void;
}

export interface CountriesComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const CountriesCombobox = forwardRef<CountriesComboboxRef, CountriesComboboxParams>(
    ({dbKey, onChange, control, name, error, onValidationChange}, ref) => {
    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();

    const { contains } = useFilter({ sensitivity: "base" })
    const [countries, setCountries] = useState<CountryListDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isValid, setIsValid] = useState<boolean>(false);

    const { collection, filter, set } = useListCollection({
        initialItems: countries,
        filter: contains,
        itemToString: (item) => item.iso3 ? item.iso3 : '-01',
        itemToValue: (item) => item.country_name ? item.country_name : "-- ERROR --",
    })

    const { keycloak } = useKeycloak();

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
        //setIsValid(valid);
        if (onValidationChange) {
            onValidationChange(valid);
        }
    }, [selectedItem]);

    useEffect(() => {
        if (dbKey) {
            fetchCountry(dbKey).then( (response) => {
                if(response != undefined)
                {
                    setCountries([response]);
                    set([response]);
                    setSelectedValue(response.country_name);
                    setSelectedItem([response]);
                    setIsValid(true);
                } else {
                    setSelectedValue('');
                    setSelectedItem([]);
                    setIsValid(false);
                }
            });
        } else {
            setSelectedValue('');
            setSelectedItem([]);
            setIsValid(false);
        }
    }, [dbKey]);

    const fetchCountry = async (iso: string): Promise<CountryDto | undefined> => {
        try {
            setLoading(true);
            const response = await countryService.getByISOAsync(iso,keycloak?.token || "");
            
            if (response.success && response.data !== undefined) 
            {
                setLoading(false);
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

    const fetchCountries = async (searchTerm: string): Promise<CountryListDto[] | undefined> => {
        try {
            setLoading(true);
            let findCommand = new CountryFindCommand();
            findCommand.wildcard = searchTerm;
            const response = await countryService.find(findCommand, keycloak?.token || "", 1, 20, "country_name-asc");

            if (response.success && response.data !== undefined) {
                set(response.data);
                setCountries(response.data);
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
        if (inputValue.length === 0 || inputValue.length >= 3) {
            await fetchCountries(inputValue);
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

        //console.log('Countries inputValue: ', inputValue);

        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = countries.find(item => item.country_name === inputValue.value[0]);

            if (selectedItem) {
                setSelectedValue(inputValue.value[0]);
                setSelectedItem([selectedItem]);
                setIsValid(true);
                onChange(selectedItem);
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
            <Combobox.Label>Country</Combobox.Label>
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
                        <Combobox.Item item={item} key={item.iso3}>
                        {item.country_name}
                        <Combobox.ItemIndicator />
                        </Combobox.Item>
                    ))}
                    </Combobox.Content>
                </Combobox.Positioner>
            </Portal>
        </Combobox.Root>
    )
});

export default CountriesCombobox;