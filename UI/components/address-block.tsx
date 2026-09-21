"use client"

import { AddressDto, AddressEditCommand } from "@/models/address-models";
import {
    Field,
    Grid,
    GridItem,
    Input,
    Stack,
} from "@chakra-ui/react";
import { useAuth } from '@/lib/auth/auth-context';
import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react"
import { Control, FieldError, useForm } from "react-hook-form";
import StatesCombobox, { StatesComboboxRef } from "./states-combobox";
import CountriesCombobox, { CountriesComboboxRef } from "./countries-combobox";
import { addressService } from "@/services/address-service";
import { countryService } from "@/services/country-service";
import { CountryDto } from "@/models/country-models";



export class AddressBlockResponse
{
    isValid: boolean = false;
    editCommand?: AddressEditCommand = undefined;
}

export class AddressBlockParams
{
    address_id: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (isValid: boolean) => void;
}

export interface AddressBlockRef {
    clear: () => void;
    getCurrentValues: () => void
}

const AddressBlock = forwardRef<AddressBlockRef, AddressBlockParams>(
    ({address_id, onChange, control, name, error, onValidationChange}, ref) => {

    const [address, setAddress] = useState<AddressDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [errorsd, setError] = useState<string | null>(null);
    const [countryId, setCountryId] = useState<number | null>(null);
    const [isComponentValid, setIsComponentValid] = useState<boolean>(false);

    const countriesComboboxRef = useRef<CountriesComboboxRef>(null);
    const statesComboboxRef = useRef<StatesComboboxRef>(null);
    
    
    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
    } = useForm<AddressEditCommand>();

    const auth = useAuth();

    // Expose methods to parent component
    useImperativeHandle(ref, () => ({
        clear: () => {
            onChange(null);
        },
        getCurrentValues: () => buildResponse(isComponentValid)
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

                const response = await addressService.get(address_id, auth.token || "");

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
                        fetchCountry(response.data.country);
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
    }, [address_id, setValue]);

    const fetchCountry = async (iso: string): Promise<CountryDto | undefined> => {
        try {
            setLoading(true);
            const response = await countryService.getByISOAsync(iso, auth.token || "");
            
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

    const statesChange = (inputValue: any) =>
    {
        if(inputValue == null)
        {
            setIsComponentValid(false);
        }

        if(inputValue)
        {
            CheckFormValidity();
        }
    }

    const countriesChange = (inputValue: any) =>
    {
        //console.log(inputValue)
        // Cleared
        if(inputValue == null)
        {
            setValue('state', '');
            statesComboboxRef.current?.clear()
            setIsComponentValid(false);

            if(onChange)
            {
                onChange(buildResponse(false))
            }
        }

        if(inputValue)
        {
            setCountryId(inputValue.id);

            if(watch('state') == '')
            {
                if(countryId)
                {
                    statesComboboxRef.current?.rebuild(countryId);
                }
            }

            CheckFormValidity();
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

        return response;
    }

    const CheckFormValidity = () => {

        if (countriesComboboxRef.current && statesComboboxRef.current) {
            const hasRequiredFields = Boolean(watch('street_address1') && watch('city') && watch('postal_code'));

            const isCountryValid = countriesComboboxRef.current.isValid();
            const isStateValid = statesComboboxRef.current.isValid();

            const allValid = isCountryValid && isStateValid && hasRequiredFields;
            
            console.log("ADDRESS hasRequiredFields: ", hasRequiredFields);
            //console.log("isCountryValid: ", isCountryValid);
            //console.log("isStateValid: ", isStateValid);

            console.log("ADDRESS allValid: ", allValid);

            setIsComponentValid(allValid);

            if(onValidationChange)
            {
                onValidationChange(allValid);
            }

            if(onChange)
            {
                onChange(buildResponse(allValid))
            }
        }
        else
        {
            setIsComponentValid(false);

            if(onValidationChange)
            {
                onValidationChange(false);
            }

            if(onChange)
            {
                onChange(buildResponse(false))
            }
        }
    };

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const checkFormValue = (formName: any) => {
        const hasRequiredFields = Boolean(watch('street_address1') && watch('city') && watch('postal_code'));
        setIsComponentValid(hasRequiredFields)
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
                        <StatesCombobox 
                            ref={statesComboboxRef}
                            dbKey={watch('state')}
                            country_id={countryId}
                            control={this}
                            name="state"
                            error={errors.state}
                            onValidationChange={CheckFormValidity}
                            onChange={statesChange} />
                        <CountriesCombobox 
                            onChange={countriesChange}
                            ref={countriesComboboxRef}
                            dbKey={watch('country')}
                            control={this}
                            name="country"
                            error={errors.country}
                            onValidationChange={CheckFormValidity} />
                </Grid>
            </GridItem>
        </Grid>
    )
});

export default AddressBlock;