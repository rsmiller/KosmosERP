"use client"

import '../../../styles/date-picker.css';
import '../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
} from '@chakra-ui/react';

import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import { useState, useRef, useEffect } from "react";
import { useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { LeadCreateCommand, LeadDto } from '@/models/lead-models';
import LeadStageCombobox, { LeadStageComboboxRef } from '@/components/lead-stage-combobox';
import StatesCombobox, { StatesComboboxRef } from '@/components/states-combobox';
import CountriesCombobox, { CountriesComboboxRef } from '@/components/countries-combobox';
import { leadService } from '@/services/lead-service';
import { useKeycloak } from '@react-keycloak/web';


import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewLeadPage() {
    const router = useRouter();

    const { keycloak } = useKeycloak();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasWritePermission, setHasWritePermission] = useState(false);

    const countriesComboboxRef = useRef<CountriesComboboxRef>(null);
    const statesComboboxRef = useRef<StatesComboboxRef>(null);
    const leadStageComboboxRef = useRef<LeadStageComboboxRef>(null);

    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    const [countryId, setCountryId] = useState<number | null>(null);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
    } = useForm<LeadCreateCommand>();

    useEffect(() => {
        if(keycloak.authenticated == false) return;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.LeadModule,
          ERPModulePermission.Write,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasWritePermission(true);
    }, [keycloak.authenticated]);

    const handleStageSelect = (value: any) => {
      //console.log(value)
      setValue('lead_stage', value?.key);
      CheckFormValidity();
    }

    const handleStateSelect = (value: any) => {
      //console.log(value)
      setValue('state', value?.iso2);
      CheckFormValidity();
    }

    const handleCountrySelect = (value: any) => {
        //console.log(value)
        
        setValue('state', undefined);
        setValue('country', value?.iso3);
        setCountryId(value?.id);
        CheckFormValidity();
    }

    const handleSaveClick = async () => {
        //console.log('Save clicked:');
        
        setSuccessSaved(false);

        let command = new LeadCreateCommand();
        command.company_name = watch('company_name');
        command.first_name = watch('first_name');
        command.last_name = watch('last_name');
        command.title = watch('title');
        command.email = watch('email');
        command.phone = watch('phone');
        command.cell_phone = watch('cell_phone');
        command.lead_stage = watch('lead_stage');
        command.time_zone = watch('time_zone');
        command.address_line1 = watch('address_line1');
        command.address_line2 = watch('address_line2');
        command.city = watch('city');
        command.state = watch('state');
        command.zip = watch('zip');
        command.country = watch('country');
        command.owner_id = watch('owner_id');

        //console.log(command);

        //return;
        try
        {
            await leadService.create(command, keycloak.token || "").then((response) =>
            {
                if(response.success)
                {
                    setSuccessSaved(true);
                    setFailedSaved(false);
                    router.push("/erp/leads");
                }
                else
                {
                    setSuccessSaved(false);
                    setFailedSaved(true);
                }
            });
        }
        catch(e)
        {
            setSuccessSaved(false);
            setFailedSaved(true);
        }
    };

    const CheckFormValidity = () => {

        if (leadStageComboboxRef.current && countriesComboboxRef.current && statesComboboxRef.current) {
            const hasRequiredFields = Boolean(watch('company_name') && watch('first_name') && watch('last_name'));

            const isStageValid = leadStageComboboxRef.current.isValid();
            const isCountryValid = countriesComboboxRef.current.isValid();
            const isStateValid = statesComboboxRef.current.isValid();

            const allValid = isStageValid && isCountryValid && isStateValid && hasRequiredFields && isValid;

            //console.log("isStageValid: ", isStageValid);
            //console.log("isCountryValid: ", isCountryValid);
            //console.log("isStateValid: ", isStateValid);
            //console.log("allValid: ", allValid);

            canSave(allValid);
        }
        else
        {
            canSave(false);
        }
    };

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    return (
        <form>
            <Grid
                templateColumns="repeat(5, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={6}>
                    <h1>New Lead</h1>
                </GridItem>
                <GridItem colSpan={1}>
                    <Field.Root invalid={IsDirty('lead_stage')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Lead Stage</Field.Label>
                        <LeadStageCombobox 
                            ref={leadStageComboboxRef}
                            dbKey={watch('lead_stage') ? [watch('lead_stage') as string] : []}
                            control={control}
                            name="lead_stage"
                            error={errors.lead_stage}
                            onChange={handleStageSelect}
                            onValidationChange={CheckFormValidity}
                        />
                    </Field.Root>
                </GridItem>
                <GridItem colSpan={5}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('company_name')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Company Name</Field.Label>
                        <Input 
                            {...register('company_name')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('first_name')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> First Name</Field.Label>
                        <Input 
                            {...register('first_name')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('last_name')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Last Name</Field.Label>
                        <Input 
                            {...register('last_name')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={!!errors.title}>
                        <Field.Label>Title</Field.Label>
                        <Input 
                            {...register('title')}
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={3}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('email')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Email</Field.Label>
                        <Input 
                            {...register('email')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={!!errors.phone}>
                        <Field.Label>Phone</Field.Label>
                        <Input 
                            {...register('phone')}
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={4}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={!!errors.cell_phone}>
                        <Field.Label>Cell Phone</Field.Label>
                        <Input 
                            {...register('cell_phone')}
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('address_line1')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Street Address 1</Field.Label>
                        <Input 
                            {...register('address_line1')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={!!errors.address_line2}>
                        <Field.Label>Street Address 2</Field.Label>
                        <Input 
                            {...register('address_line2')}
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('city')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> City</Field.Label>
                        <Input 
                            {...register('city')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('zip')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Postal Code</Field.Label>
                        <Input 
                            {...register('zip')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={4}></GridItem>
                <GridItem colSpan={2}>
                    <Grid templateColumns="repeat(2, 2fr)"
                        gap={6}
                        display="grid"
                        width="100%"
                        p="auto"
                        m="auto">
                            <StatesCombobox onChange={handleStateSelect}
                                      ref={statesComboboxRef}
                                      dbKey={watch('state')}
                                      country_id={countryId}
                                      control={control}
                                      name="state"
                                      error={errors.state}
                                      onValidationChange={CheckFormValidity}/>
                            <CountriesCombobox onChange={handleCountrySelect}
                                        ref={countriesComboboxRef}
                                        dbKey={watch('country')}
                                        control={control}
                                        name="country"
                                        error={errors.country}
                                        onValidationChange={CheckFormValidity}/>
                    </Grid>
                </GridItem>
                <GridItem colSpan={5}>
                    <PageActionsComponent 
                        canSave={!saveable || !hasWritePermission} 
                        onSave={handleSaveClick} 
                        onDelete={undefined} 
                        successSaved={successSaved}
                        failedSaved={failedSaved}
                    />
                </GridItem>
            </Grid>
        </form>
    )
}

export default NewLeadPage;