
"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Tabs,
} from '@chakra-ui/react';

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { LeadEditCommand, LeadDto, LeadDeleteCommand } from '@/models/lead-models';
import LeadStageCombobox, { LeadStageComboboxRef } from '@/components/lead-stage-combobox';
import StatesCombobox, { StatesComboboxRef } from '@/components/states-combobox';
import CountriesCombobox, { CountriesComboboxRef } from '@/components/countries-combobox';
import { leadService } from '@/services/lead-service';
import { CountryDto } from '@/models/country-models';
import { countryService } from '@/services/country-service';
import ActivitiesListComponent from '@/components/lists/activities-list-component';
import { useKeycloak } from '@react-keycloak/web';

import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditLeadPage() {
    const params = useParams();
    const router = useRouter();

    const { keycloak } = useKeycloak();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);
    const [hasDeletePermission, setHasDeletePermission] = useState(false);

    const countriesComboboxRef = useRef<CountriesComboboxRef>(null);
    const statesComboboxRef = useRef<StatesComboboxRef>(null);
    const leadStageComboboxRef = useRef<LeadStageComboboxRef>(null);
    const hasInitialized = useRef(false);

    const [lead, setLead] = useState<LeadDto | null>(null);
    const [loading, setLoading] = useState(true);
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
    } = useForm<LeadEditCommand>();

    const loadLead = async () => {
        try {
            setLoading(true);
            const leadId = String(params.id);
            
            if (leadId == "") {
                setError('Invalid lead ID');
                return;
            }

            const response = await leadService.getByGuid(leadId, keycloak.token || "");
            if (response.success && response.data) {
                setLead(response.data);
                
                // Set form values with loaded data
                setValue('id', response.data.id);
                setValue('company_name', response.data.company_name || '');
                setValue('first_name', response.data.first_name || '');
                setValue('last_name', response.data.last_name || '');
                setValue('title', response.data.title || '');
                setValue('email', response.data.email || '');
                setValue('phone', response.data.phone || '');
                setValue('cell_phone', response.data.cell_phone || '');
                setValue('lead_stage', response.data.lead_stage || '');
                setValue('time_zone', response.data.time_zone || '');
                setValue('address_line1', response.data.address_line1 || '');
                setValue('address_line2', response.data.address_line2 || '');
                setValue('city', response.data.city || '');
                setValue('state', response.data.state || '');
                setValue('zip', response.data.zip || '');
                setValue('country', response.data.country || '');
                setValue('owner_id', response.data.owner_id);

                if(response.data.country)
                {
                    fetchCountry(response.data.country);
                }
                
            } else {
                setError('Failed to load lead');
            }
        } catch (err) {
            console.error('Error loading lead:', err);
            setError('Error loading lead');
        } finally {
            setLoading(false);
        }
    };


    useEffect(() => {
        if(keycloak.authenticated == false) return;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.LeadModule,
          ERPModulePermission.Read,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        // Check Edit permission
        const canEdit = permissionsService.HasPermission(
          ERPModules.LeadModule,
          ERPModulePermission.Edit,
          realmRoles
        );
        setHasEditPermission(canEdit);

        // Check Delete permission
        const canDelete = permissionsService.HasPermission(
          ERPModules.LeadModule,
          ERPModulePermission.Delete,
          realmRoles
        );
        setHasDeletePermission(canDelete);

        if (hasInitialized.current) return;

        hasInitialized.current = true;

        loadLead();
    }, [params.id, setValue, keycloak.authenticated]);

    const fetchCountry = async (iso: string): Promise<CountryDto | undefined> => {
        try {
            setLoading(true);
            const response = await countryService.getByISOAsync(iso, keycloak.token || "");
            
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

    const handleDeleteClick = async () => {
      //console.log('Delete clicked:');
      let command = new LeadDeleteCommand();
      command.id = lead?.id;

      try
      {
          await leadService.delete(command, keycloak.token || "").then((response) => {
              //console.log(response)
              if(response.success)
              {
                  router.push("/erp/leads/");
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

    const handleSaveClick = async () => {
        //console.log('Save clicked:');
        
        setSuccessSaved(false);

        let command = new LeadEditCommand();
        command.id = lead?.id;
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
            await leadService.update(command, keycloak.token || "").then((response) =>
            {
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

    if (loading) {
        return <div>Loading lead...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!lead) {
        return <div>Lead not found</div>;
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
                    <h1>Edit Lead</h1>
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
                    <Tabs.Root lazyMount unmountOnExit defaultValue="activities">
                        <Tabs.List>
                            <Tabs.Trigger value="activities">Activities</Tabs.Trigger>
                        </Tabs.List>
                        <Tabs.Content value="activities">
                            <ActivitiesListComponent entity_id={lead.id} entity_type="lead" onChange={() => {}} />
                        </Tabs.Content>
                    </Tabs.Root>
                </GridItem>
                <GridItem colSpan={5}>
                    <PageActionsComponent 
                        canSave={!saveable || !hasEditPermission} 
                        canDelete={hasDeletePermission}
                        onSave={handleSaveClick} 
                        onDelete={handleDeleteClick} 
                        successSaved={successSaved}
                        failedSaved={failedSaved}
                    />
                </GridItem>
            </Grid>
        </form>
    )
}

export default EditLeadPage;