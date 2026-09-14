
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

import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { ContactEditCommand, ContactDto, ContactDeleteCommand } from '@/models/contact-models';
import CustomerCombobox, { CustomerComboboxRef } from '@/components/customer-combobox';
import { contactService } from '@/services/contact-service';
import ActivitiesListComponent from '@/components/lists/activities-list-component';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditContactPage() {
    const params = useParams();
    const router = useRouter();

    const { keycloak } = useKeycloak();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);
    const [hasDeletePermission, setHasDeletePermission] = useState(false);

    const customerComboboxRef = useRef<CustomerComboboxRef>(null);
    const hasInitialized = useRef(false);

    const [contact, setContact] = useState<ContactDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
        trigger,
    } = useForm<ContactEditCommand>({
      mode: 'onChange',
    });


    const loadContact = async () => {
        try {
            setLoading(true);
            const contactId = String(params.id);
            
            if (contactId == "") {
                setError('Invalid contact ID');
                return;
            }

            const response = await contactService.getByGuid(contactId, keycloak.token || "");
            if (response.success && response.data) {
                setContact(response.data);
                
                // Set form values with loaded data
                setValue('id', response.data.id);
                setValue('customer_id', response.data.customer_id);
                setValue('first_name', response.data.first_name || '', { shouldValidate: true });
                setValue('last_name', response.data.last_name || '');
                setValue('title', response.data.title || '');
                setValue('email', response.data.email || '');
                setValue('phone', response.data.phone || '');
                setValue('cell_phone', response.data.cell_phone || '');
                
            } else {
                setError('Failed to load contact');
            }
        } catch (err) {
            console.error('Error loading contact:', err);
            setError('Error loading contact');
        } finally {
            setLoading(false);
        }
    };

    // Load contact data on component mount
    useEffect(() => {

        if(keycloak.authenticated == false) return;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ContactModule,
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
          ERPModules.ContactModule,
          ERPModulePermission.Edit,
          realmRoles
        );
        setHasEditPermission(canEdit);

        // Check Delete permission
        const canDelete = permissionsService.HasPermission(
          ERPModules.ContactModule,
          ERPModulePermission.Delete,
          realmRoles
        );
        setHasDeletePermission(canDelete);

        if (hasInitialized.current) return;

        hasInitialized.current = true;
        

        loadContact();
    }, [params.id, setValue, keycloak.authenticated]);

    const handleCustomerSelect = (value: any) => {
      setValue('customer_id', value?.id);
      FormChange();
    }

    const handleDeleteClick = async () => {
        let command = new ContactDeleteCommand();
        command.id = contact?.id;

        try
        {
          await contactService.delete(command, keycloak.token || "").then((response) => {
              if(response.success)
              {
                router.push("/erp/contacts/");
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
          console.error(e);
          setSuccessSaved(false);
          setFailedSaved(true);
        }
    };

    const handleSaveClick = async () => {
        setSuccessSaved(false);

        let command = new ContactEditCommand();
        command.id = contact?.id;
        command.customer_id = watch('customer_id');
        command.first_name = watch('first_name');
        command.last_name = watch('last_name');
        command.title = watch('title');
        command.email = watch('email');
        command.phone = watch('phone');
        command.cell_phone = watch('cell_phone');

        try
        {
            await contactService.update(command, keycloak.token || "").then((response) =>
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

    const FormChange = () => {

      const hasRequiredFields = !IsDirty('first_name') && !IsDirty('last_name') && !IsDirty('customer_id')
                                          && !IsDirty('email');

      const customerValid = Boolean(customerComboboxRef.current?.isValid());

      const allValid = isValid && customerValid && hasRequiredFields;

      canSave(allValid);
    };

    const IsDirty = (formName: any) => {
      if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
      {
        return true;
      }

      return false;
    }

    if (loading) {
        return <div>Loading contact...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!contact) {
        return <div>Contact not found</div>;
    }

    return (
        <form onChange={FormChange}>
            <Grid
                templateColumns="repeat(5, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={6}>
                    <h1>Edit Contact</h1>
                </GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={!!errors.customer_id}>
                        <CustomerCombobox 
                            ref={customerComboboxRef}
                            dbKey={watch('customer_id')}
                            onChange={handleCustomerSelect}
                            control={control}
                            name="customer_id"
                            error={errors.customer_id}
                            disabled={false}
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('first_name')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> First Name</Field.Label>
                        <Input 
                            {...register('first_name', { required: 'First name is required' })}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('last_name')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Last Name</Field.Label>
                        <Input 
                            {...register('last_name', { required: 'Last name is required' })}
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
                    <Field.Root invalid={IsDirty('email')}  required={true}>
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
                <GridItem colSpan={1}></GridItem>
                <GridItem colSpan={5}>
                    <Tabs.Root lazyMount unmountOnExit defaultValue="activities">
                        <Tabs.List>
                            <Tabs.Trigger value="activities">Activities</Tabs.Trigger>
                        </Tabs.List>
                        <Tabs.Content value="activities">
                            <ActivitiesListComponent entity_id={contact.id} entity_type="contact" onChange={() => {}} />
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

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }
}

export default EditContactPage;