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

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useState, useRef } from "react";
import { useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { ContactCreateCommand } from '@/models/contact-models';
import CustomerCombobox, { CustomerComboboxRef } from '@/components/customer-combobox';
import { contactService } from '@/services/contact-service';
import SessionStorage from '@/components/session-storage';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewContactPage() {
    const { keycloak } = useKeycloak();
    const router = useRouter();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasWritePermission, setHasWritePermission] = useState(false);

    const customerComboboxRef = useRef<CustomerComboboxRef>(null);

    const [loading, setLoading] = useState(false);
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
    } = useForm<ContactCreateCommand>({
      mode: 'onChange',
    });

    const handleCustomerSelect = (value: any) => {
      setValue('customer_id', value?.id);
      FormChange();
    }

    const handleSaveClick = async () => {
        setSuccessSaved(false);
        setLoading(true);

        let command = new ContactCreateCommand();
        command.customer_id = watch('customer_id');
        command.first_name = watch('first_name');
        command.last_name = watch('last_name');
        command.title = watch('title');
        command.email = watch('email');
        command.phone = watch('phone');
        command.cell_phone = watch('cell_phone');

        try
        {
            await contactService.create(command, keycloak?.token || "").then((response) =>
            {
                if(response.success)
                {
                    setSuccessSaved(true);
                    setFailedSaved(false);
                    // Route to contacts list on successful save
                    router.push("/erp/contacts");
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
        } finally {
            setLoading(false);
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

    useEffect(() => {
      if(keycloak.authenticated == false) return;

      const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
      const hasPermission = permissionsService.HasPermission(
        ERPModules.ContactModule,
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
                    <h1>New Contact</h1>
                </GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={!!errors.customer_id}>
                         <Field.Label><Field.RequiredIndicator /> Customer</Field.Label>
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
                <GridItem colSpan={6}>
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

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }
}

export default NewContactPage;