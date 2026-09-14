"use client"

import { AddressDto, AddressEditCommand } from "@/models/address-models";
import {
    Combobox,
    Field,
    Grid,
    GridItem,
    Input,
    Portal,
    Stack,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";
import { useKeycloak } from '@react-keycloak/web';
import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react"
import { Control, FieldError, useForm } from "react-hook-form";
import { addressService } from "@/services/address-service";
import { countryService } from "@/services/country-service";
import { CountryDto, CountryFindCommand, CountryListDto, StateDto, StateFindCommand, StateListDto } from "@/models/country-models";
import { AddressBlockResponse } from "./address-block";
import SessionStorage from "./session-storage";
import { stateService } from "@/services/state-service";



export class NewAddressBlockResponse
{
    isValid: boolean = false;
    editCommand?: AddressEditCommand = undefined;
}

export class NewAddressBlockParams
{
    address_id: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (response: NewAddressBlockResponse) => void;
}

export interface NewAddressBlockRef {
    clear: () => void;
    getCurrentValues: () => void
}

const NewAddressBlock = forwardRef<NewAddressBlockRef, NewAddressBlockParams>(
    ({address_id, onChange, control, name, error=null, onValidationChange}, ref) => {
    const { contains } = useFilter({ sensitivity: "base" })
    
    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();
    const { keycloak } = useKeycloak();
    

    const [address, setAddress] = useState<AddressDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [errorsd, setError] = useState<string | null>(null);
    const [countryId, setCountryId] = useState<number | null>(null);
    const [isComponentValid, setIsComponentValid] = useState<boolean>(false);
    const [currentResponse, setCurrentResponse] = useState<NewAddressBlockResponse>();

    const [countries, setCountries] = useState<CountryListDto[]>([]);
    const [selectedCountryValue, setSelectedCountryValue] = useState<string>();
    const [selectedCountryItem, setSelectedCountryItem] = useState<any[]>([]);

    const [states, setStates] = useState<StateListDto[]>([]);
    const [selectedStateValue, setSelectedStateValue] = useState<string>();
    const [selectedStateItem, setSelectedStateItem] = useState<any[]>([]);

    const { collection, filter, set } = useListCollection({
        initialItems: countries,
        filter: contains,
        itemToString: (item) => item.iso3 ? item.iso3 : '-01',
        itemToValue: (item) => item.country_name ? item.country_name : "-- ERROR --",
    });

    const { collection: statesCollection, filter: statesFilter, set: statesSet } = useListCollection({
        initialItems: states,
        filter: contains,
        itemToString: (item) => item.state_name ? item.state_name : '-01',
        itemToValue: (item) => item.iso2?.toString() ? item.iso2.toString() : "-1",
    });

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
    } = useForm<AddressEditCommand>();

    // Expose methods to parent component
    useImperativeHandle(ref, () => ({
        clear: () => {
            onChange(null);
        },
        getCurrentValues: () => null
    }), [onChange]);

    // Load lead data on component mount
    useEffect(() => {
        const loadAddress = async () => {
            try {
                setLoading(true);
                
                if (!address_id) {
                    setError('Invalid lead ID');
                    return;
                }

                const response = await addressService.get(address_id, keycloak?.token || "");

                //console.log(response)

                if (response.success && response.data) {
                    setAddress(response.data);
                    
                    // Set form values with loaded data
                    setValue('id', response.data.id);
                    setValue('street_address1', response.data.street_address1 || '');
                    setValue('street_address2', response.data.street_address2 || '');
                    setValue('city', response.data.city || '');
                    setValue('state', response.data.state || '');
                    setValue('postal_code', response.data.postal_code || '');
                    setValue('country', response.data.country || '');

                    if(response.data.country)
                    {
                        fetchCountry(response.data.country).then((country_response: any) => 
                        {
                            if(country_response != undefined)
                            {
                                setCountries([country_response]);
                                set([country_response]);
                                setSelectedCountryValue(country_response.country_name);
                                setSelectedCountryItem([country_response]);

                                if(response.data?.state)
                                {
                                    fetchState(country_response.id, response.data?.state).then( (states_response) => {
                                        console.log(states_response)
                                        if(states_response != undefined)
                                        {
                                            setStates([states_response]);
                                            statesSet([states_response]);
                                            setSelectedStateValue(states_response.state_name);
                                            setSelectedStateItem([states_response]);
                                        } else {
                                            setSelectedStateValue('');
                                            setSelectedStateItem([]);
                                        }
                                    });
                                }
                            }
                        });
                    }

                    if(onValidationChange)
                    {
                        //onValidationChange(buildResponse(true));
                    }
                    
                } else {
                    setError('Failed to load address');
                }
            } catch (err) {
                console.error('Error loading address:', err);
                setError('Error loading address');
            } finally {
                setLoading(false);
            }
        };
        
        loadAddress();
    }, [address_id]);


    const fetchCountry = async (iso: string): Promise<CountryDto | undefined> => {
        try {
            setLoading(true);
            const response = await countryService.getByISOAsync(iso, keycloak?.token || "");
            
            if (response.success && response.data !== undefined) 
            {
                setLoading(false);
                setCountryId(response.data.id);
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

    const fetchState = async (country_id: number, iso: string): Promise<StateDto | undefined> => {
        try {
            setLoading(true);
            const response = await stateService.getByISOAsync(country_id, iso, keycloak?.token || "");
            
            if (response.success && response.data !== undefined) 
            {
                setLoading(false);
                set([response.data]);
                setStates([response.data]);
                setSelectedStateValue(response.data.state_name);
                setSelectedStateItem([response.data]);

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
                statesSet(response.data);
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

        try {
            setLoading(true);
            let findCommand = new StateFindCommand();
            findCommand.country_id = country_id;
            
            const response = await stateService.find(findCommand, keycloak?.token || "", 1, 100, "state_name-asc");
            
            if (response.success && response.data !== undefined) {
                statesSet(response.data);
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
    
    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const CheckFormValidity = () => {
        const hasRequiredFields = Boolean(watch('street_address1') && watch('city') && watch('postal_code'));

        const isAllValid = selectedStateValue != '' && selectedStateValue != null && selectedCountryValue != '' && selectedCountryValue != null && hasRequiredFields

        setIsComponentValid(isAllValid);

        //console.log("ADDRESS BLOCK: ", hasRequiredFields);
        //console.log("ADDRESS selectedStateValue: ", selectedStateValue);
        //console.log("ADDRESS selectedCountryValue: ", selectedCountryValue);
        //console.log("ADDRESS isAllValid: ", isAllValid);


        if(onValidationChange)
        {
            onValidationChange(buildResponse(isAllValid));
        }
    }

    const buildResponse = (valid: boolean) =>
    {
        let command = new AddressEditCommand();
        command.id = watch('id');
        command.street_address1 = watch('street_address1');
        command.street_address2 = watch('street_address2');
        command.city = watch('city');
        command.state = watch('state');
        command.postal_code = watch('postal_code');
        command.country = watch('country');


        let response = new AddressBlockResponse();
        response.isValid = valid;
        response.editCommand = command;

        setCurrentResponse(response);

        return response;
    }


    const inputCountryValChange = async (inputValue: string) => {
        // Only search if input is empty or has 3+ characters
        if (inputValue.length === 1 || inputValue.length >= 3) {
            await fetchCountries(inputValue);
        }
    }

    const inputCountryChange = (inputValue: any) => {
        if(inputValue == null)
        {
            setSelectedCountryValue('');
            setSelectedCountryItem([]);
            setSelectedStateValue('');
            setSelectedStateItem([]);
            setStates([]);
            statesSet([]);
            setValue('state', '');
            setValue('country', '');

            if(onValidationChange)
            {
                onValidationChange(buildResponse(false));
            }
            return;
        }
        
        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = countries.find(item => item.country_name === inputValue.value[0]);
            //console.log("selectedItem ", selectedItem)

            if (selectedItem) {
                setSelectedCountryValue(inputValue.value[0]);
                setSelectedCountryItem([selectedItem]);
                setValue('country', selectedItem.iso3);

                fetchStatesByCountry(selectedItem.id);

                if(onValidationChange)
                {
                    onValidationChange(buildResponse(true));
                }
            } else {
                setSelectedCountryValue('');
                setSelectedCountryItem([]);
                setValue('country', '');

                if(onValidationChange)
                {
                    onValidationChange(buildResponse(false));
                }
            }
        }
        else
        {
            // Fires when cleared
            setSelectedCountryValue('');
            setSelectedCountryItem([]);
            setSelectedStateValue('');
            setSelectedStateItem([]);
            setStates([]);
            statesSet([]);
            setValue('state', '');
            setValue('country', '');

            if(onValidationChange)
            {
                onValidationChange(buildResponse(false));
            }
        }


    }

    const inputStateValChange = async (inputValue: string) => {
        // Only search if input is empty or has 3+ characters
        if (inputValue.length === 1 || inputValue.length >= 3) {
            await fetchStates(inputValue);
        }
    }

    const inputStateChange = (inputValue: any) => {
        
        if(inputValue == null)
        {
            setSelectedStateValue('');
            setSelectedStateItem([]);
            setValue('state', '');
            return;
        }

        //console.log("inputChange: ", inputValue)
        if(inputValue.value != undefined && inputValue.value.length > 0)
        {
            const selectedItem = states.find(item => item.iso2 === inputValue.value[0]);

            if (selectedItem) {
                setSelectedStateValue(selectedItem.state_name);
                setSelectedStateItem([selectedItem]);
                setValue('state', selectedItem.iso2);
                if(onValidationChange)
                {
                    onValidationChange(buildResponse(true));
                }

            } else {
                setSelectedStateValue('');
                setSelectedStateItem([]);
                setValue('state', '');
                if(onValidationChange)
                {
                    onValidationChange(buildResponse(false));
                }
            }
        }
        else
        {
            setSelectedStateValue('');
            setSelectedStateItem([]);
            setValue('state', '');
            if(onValidationChange)
            {
                onValidationChange(buildResponse(false));
            }
        }
    }

    return (
        <Grid
            templateColumns="repeat(2, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
            onChange={CheckFormValidity}>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root invalid={IsDirty('street_address1')} required={true}>
                    <Field.Label><Field.RequiredIndicator /> Street Address 1</Field.Label>
                    <Input {...register('street_address1')}/>
                    <Field.ErrorText>This field is required</Field.ErrorText>
                </Field.Root>
            </Stack>
            <GridItem colSpan={1}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root invalid={!!errors.street_address2}>
                    <Field.Label>Street Address 2</Field.Label>
                    <Input {...register('street_address2')}/>
                </Field.Root>
            </Stack>
            <GridItem colSpan={1}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root invalid={IsDirty('city')} required={true}>
                    <Field.Label><Field.RequiredIndicator /> City</Field.Label>
                    <Input {...register('city')}/>
                    <Field.ErrorText>This field is required</Field.ErrorText>
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root invalid={IsDirty('postal_code')} required={true}>
                    <Field.Label><Field.RequiredIndicator /> Postal Code</Field.Label>
                    <Input {...register('postal_code')}/>
                    <Field.ErrorText>This field is required</Field.ErrorText>
                </Field.Root>
            </Stack>
            <GridItem colSpan={2}>
                <Grid templateColumns="repeat(2, 2fr)"
                    gap={6}
                    display="grid"
                    width="100%"
                    p="auto"
                    m="auto">
                        <GridItem >
                            <Combobox.Root
                                collection={statesCollection}
                                allowCustomValue={false}
                                required={true}
                                width="100%"
                                inputValue={selectedStateValue}
                                value={selectedStateItem}
                                onValueChange={(e) => inputStateChange(e)}
                                onInputValueChange={(e) => inputStateValChange(e.inputValue)}
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
                                        {statesCollection.items.map((item) => (
                                            <Combobox.Item item={item} key={item.id}>
                                            {item.state_name}
                                            <Combobox.ItemIndicator />
                                            </Combobox.Item>
                                        ))}
                                        </Combobox.Content>
                                    </Combobox.Positioner>
                                </Portal>
                            </Combobox.Root>
                        </GridItem>
                        <GridItem >
                            <div style={{ position: 'relative', zIndex: 100000 }}>
                                <Combobox.Root
                                    collection={collection}
                                    allowCustomValue={false}
                                    required={true}
                                    width="100%"
                                    inputValue={selectedCountryValue}
                                    value={selectedCountryItem}
                                    onValueChange={(e) => inputCountryChange(e)}
                                    onInputValueChange={(e) => inputCountryValChange(e.inputValue)}
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
                            </div>
                        </GridItem>
                </Grid>
            </GridItem>
        </Grid>
    )
});

export default NewAddressBlock;