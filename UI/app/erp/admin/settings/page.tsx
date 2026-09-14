"use client"

import '../../../styles/date-picker.css';
import '../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import SessionStorage from "@/components/session-storage";
import { SettingsDto, SettingsEditCommand } from "@/models/settings-models";
import { Grid, GridItem, Stack, Input, Field } from "@chakra-ui/react";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import DatePicker from 'react-datepicker';
import { format } from 'date-fns';
import { settingsService } from '@/services/settings-service';
import PageActionsComponent from '@/components/page-actions';
import CountriesCombobox, { CountriesComboboxRef } from '@/components/countries-combobox';
import StatesCombobox, { StatesComboboxRef } from '@/components/states-combobox';
import { countryService } from '@/services/country-service';
import { CountryFindCommand } from '@/models/country-models';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules } from '@/services/permissions-service';


function AdminSettingsPage() {
    const router = useRouter();
    const { keycloak } = useKeycloak();
    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);
    
    const [settings, setSetting] = useState<SettingsDto | null>(null);

    const [loading, setLoading] = useState(true);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);

    const countriesComboboxRef = useRef<CountriesComboboxRef>(null);
    const statesComboboxRef = useRef<StatesComboboxRef>(null);
    const [countryId, setCountryId] = useState<number | null>(null);
    const hasInitialized = useRef(false);

    const {
        register,
        setValue,
        formState: { errors, isValid },
        watch,
    } = useForm<SettingsEditCommand>({
        mode: 'onChange',
    });
        
    useEffect(() => {
        if(keycloak.authenticated == false) return;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.Admin,
          '',
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasEditPermission(true);

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        setLoading(true);
        settingsService.getBaseSettings("").then( (response) => 
        {
            //console.log("Settings:", response);
            setLoading(false);

            if(response.success && response.data)
            {
                setSetting(response.data);

                setValue('company_name', response.data.company_name);
                setValue('tax_id', response.data.tax_id);
                setValue('fiscal_year_start', response.data.fiscal_year_start);
                setValue('company_ar_email', response.data.company_ar_email);
                setValue('company_ap_email', response.data.company_ap_email);
                setValue('company_general_email', response.data.company_general_email);
                setValue('company_website', response.data.company_website);
                setValue('company_phone', response.data.company_phone);
                setValue('company_address1', response.data.company_address1);
                setValue('company_address2', response.data.company_address2);
                setValue('company_city', response.data.company_city);
                setValue('company_state', response.data.company_state);
                setValue('company_zip', response.data.company_zip);
                setValue('company_country', response.data.company_country);

                if(response.data.company_country)
                {   
                    let countryCommand = new CountryFindCommand();
                    countryCommand.wildcard = response.data.company_country;

                    countryService.find(countryCommand, keycloak.token || "").then( (countryResponse) => {
                        if(countryResponse.success && countryResponse.data && countryResponse.data.length > 0)
                        {
                            var filtered = countryResponse.data.filter(c => c.iso3 == response.data?.company_country);
                            if(filtered.length > 0)
                            {
                                setCountryId(filtered[0].id);
                            }
                        }
                    });
                }
            }
        });
    }, [keycloak.authenticated]);

    const getStartDate = () => {
        const val = watch('fiscal_year_start');
        return val ? new Date(val + "T00:01:00") : undefined;
    };

    const handleStartDateChange = (date: Date | null) => {
        if(date != null)
        {
            setValue('fiscal_year_start', format(date, 'yyyy-MM-dd'));
        }
        
        CheckFormValidity();
    };

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const CheckFormValidity = () => {
        const hasRequiredFields = Boolean(watch('company_name') && watch('fiscal_year_start'));

        //console.log("PAGE hasRequiredFields: ", hasRequiredFields);
        //console.log("PAGE addressBlock", addressBlockValid)
        //console.log("PAGE allValid: ", allValid);

        canSave(hasRequiredFields)
    };


    const handleSaveClick = async () => {
        setSuccessSaved(false);

        let command = new SettingsEditCommand();
        command.id = settings?.id;
        command.company_name = watch('company_name');
        command.tax_id = watch('tax_id');
        command.fiscal_year_start = watch('fiscal_year_start');
        command.company_ar_email = watch('company_ar_email');
        command.company_ap_email = watch('company_ap_email');
        command.company_general_email = watch('company_general_email');
        command.company_website = watch('company_website');
        command.company_phone = watch('company_phone');
        command.company_address1 = watch('company_address1');
        command.company_address2 = watch('company_address2');
        command.company_city = watch('company_city');
        command.company_state = watch('company_state');
        command.company_zip = watch('company_zip');
        command.company_country = watch('company_country');

        await settingsService.update(command, keycloak.token || "").then((response) => {
            if(response.success)
            {
                setSuccessSaved(true);
                setFailedSaved(false);
            }
            else
            {
                setSuccessSaved(false);
                setFailedSaved(true);
            }
        });
    }

    const statesChange = (inputValue: any) =>
    {
        //console.log("States changed:", inputValue);
        setValue('company_state', inputValue.iso2);
    }

    const countryChange = (inputValue: any) =>
    {
        //console.log("Country changed:", inputValue);

        if(inputValue == null)
        {
            setCountryId(null);
            setValue('company_state', '');
        }
        else
        {
            setCountryId(inputValue.id);
        }
    }

    return (
        <div>
            <form onChange={ () => CheckFormValidity() }>
                <Grid
                    templateColumns="repeat(4, 2fr)"
                    gap={6}
                    display="grid"
                    width="100%"
                    p="auto"
                    m="auto"
                    >
                        <GridItem colSpan={4}>
                            <h1>Settings</h1>
                        </GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root invalid={IsDirty('company_name')} required={true}>
                                    <Field.Label><Field.RequiredIndicator />Company Name</Field.Label>
                                    <Input {...register('company_name')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={3}></GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>Tax ID</Field.Label>
                                    <Input {...register('tax_id')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root invalid={IsDirty('vendor_description')} required={true}>
                                    <Field.Label><Field.RequiredIndicator />Fiscal Year Start</Field.Label>
                                    <DatePicker 
                                        selected={getStartDate()}
                                        onChange={handleStartDateChange}
                                        dateFormat="MM/dd/yyyy"
                                        placeholderText="Select date"
                                    />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={2}></GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>AR Email</Field.Label>
                                    <Input {...register('company_ar_email')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>AP Email</Field.Label>
                                    <Input {...register('company_ap_email')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>General Email</Field.Label>
                                    <Input {...register('company_general_email')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={1}></GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>Website</Field.Label>
                                    <Input {...register('company_website')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>Phone</Field.Label>
                                    <Input {...register('company_phone')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={2}></GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>Address 1</Field.Label>
                                    <Input {...register('company_address1')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={3}></GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>Address 2</Field.Label>
                                    <Input {...register('company_address2')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={3}></GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>City</Field.Label>
                                    <Input {...register('company_city')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <StatesCombobox 
                                    ref={statesComboboxRef}
                                    dbKey={watch('company_state')}
                                    country_id={countryId}
                                    control={undefined}
                                    name="state"
                                    error={errors.company_state}
                                    onValidationChange={CheckFormValidity}
                                    onChange={statesChange} />
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={2}></GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <Field.Root>
                                    <Field.Label>Zip</Field.Label>
                                    <Input {...register('company_zip')} />
                                </Field.Root>
                            </Stack>
                        </GridItem>

                        <GridItem colSpan={3}></GridItem>

                        <GridItem colSpan={1}>
                            <Stack gap="4" align="flex-start" maxW="md">
                                <CountriesCombobox 
                                    ref={countriesComboboxRef}
                                    dbKey={watch('company_country')}
                                    control={undefined}
                                    name="state"
                                    error={errors.company_country}
                                    onValidationChange={CheckFormValidity}
                                    onChange={countryChange} />
                            </Stack>
                        </GridItem>

                        

                        <GridItem colSpan={4}>
                            <PageActionsComponent 
                                canSave={!saveable || !hasEditPermission} 
                                deleteable={false}
                                onSave={handleSaveClick} 
                                successSaved={successSaved}
                                failedSaved={failedSaved}
                            />
                        </GridItem>
                    </Grid>
            </form>
        </div>
    );
};

export default AdminSettingsPage;