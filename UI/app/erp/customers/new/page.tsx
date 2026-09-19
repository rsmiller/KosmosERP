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
  Textarea,
  Checkbox,
  NumberInput,
} from '@chakra-ui/react';

import { useState, useRef, useEffect } from "react";
import { useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { CustomerCreateCommand } from '@/models/customer-models';
import CustomerPaymentTermsCombobox, { CustomerPaymentTermsComboboxRef } from '@/components/customer-payment-terms-combobox';
import { customerService } from '@/services/customer-service';
import CustomerCategoriesCombobox, { CustomerCategoriesComboboxRef } from '@/components/customer-categories-combobox';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

function NewCustomerPage() {
  const router = useRouter();

  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasWritePermission, setHasWritePermission] = useState(false);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);

  const paymentTermsComboboxRef = useRef<CustomerPaymentTermsComboboxRef>(null);
  const categoryComboboxRef = useRef<CustomerCategoriesComboboxRef>(null);

  useEffect(() => {
    if (auth.authenticated == false) return;

    // Check permission
    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.CustomerModule,
      ERPModulePermission.Write,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }
    setHasWritePermission(true);
  }, [auth.authenticated]);

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    setValue,
    watch,
    control,
  } = useForm<CustomerCreateCommand>();

  const handleSaveClick = async () => {
    setSuccessSaved(false);

    let command = new CustomerCreateCommand();
    command.customer_name = watch('customer_name');
    command.customer_description = watch('customer_description');
    command.phone = watch('phone');
    command.fax = watch('fax');
    command.general_email = watch('general_email');
    command.website = watch('website');
    command.category = watch('category');
    command.is_taxable = Boolean(watch('is_taxable'));
    command.tax_rate = Number(watch('tax_rate'));
    command.payment_terms = watch('payment_terms');


    //console.log(command);
    try {
      await customerService.create(command, auth.token || "").then((response) => {
        if (response.success) {
          setSuccessSaved(true);
          setFailedSaved(false);
          // Route to customers page on successful save
          router.push("/erp/customers");
        } else {
          setSuccessSaved(false);
          setFailedSaved(true);
        }
      });
    } catch (e) {
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const CheckFormValidity = () => {
    if(paymentTermsComboboxRef.current)
    {
        const paymentTermsValid = paymentTermsComboboxRef.current?.isValid();
        const categoryValid = categoryComboboxRef.current?.isValid();

        const hasRequiredFields = Boolean(watch('customer_name') && watch('general_email') && watch('phone'));
        const allValid = hasRequiredFields && paymentTermsValid && categoryValid;

        canSave(!allValid);
    }
    else
    {
      canSave(false);
    }
  };

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  
  const IsDirty = (formName: any) => {
    if (watch(formName) == undefined || watch(formName) == null || watch(formName) == '') {
      return true;
    }
    return false;
  };

  const handleTermsSelect = (result: any) => {
    setValue('payment_terms', result?.key);
    CheckFormValidity();
  };

  const handleCategorySelect = (result: any) => {
    setValue('category', result?.value);
    CheckFormValidity();
  };

  return (
    <form onChange={CheckFormValidity}>
      <Grid
        templateColumns="repeat(6, 2fr)"
        gap={6}
        display="grid"
        width="100%"
        p="auto"
        m="auto"
      >
        <GridItem colSpan={5}>
          <h1>New Customer</h1>
        </GridItem>

        <GridItem colSpan={2}>
          <Stack gap="4" align="flex-start" maxW="md">
            <Field.Root invalid={IsDirty('customer_name')} required={true}>
              <Field.Label><Field.RequiredIndicator /> Customer Name</Field.Label>
              <Input 
                {...register('customer_name')}
              />
              <Field.ErrorText>This field is required</Field.ErrorText>
            </Field.Root>
          </Stack>
        </GridItem>
        <GridItem colSpan={1}>
            <Field.Root invalid={!!errors.category} required={true}>
            <Field.Label><Field.RequiredIndicator /> Category</Field.Label>
                <CustomerCategoriesCombobox
                    ref={categoryComboboxRef}
                    dbKey={watch('category')}
                    control={control}
                    name="categories"
                    error={errors.category}
                    onChange={handleCategorySelect}
                    onValidationChange={CheckFormValidity}
                />
            </Field.Root>
        </GridItem>
        <GridItem colSpan={3}></GridItem>
        <GridItem colSpan={2}>
          <Stack gap="4" align="flex-start" maxW="md">
            <Field.Root invalid={!!errors.customer_description}>
              <Field.Label>Description</Field.Label>
              <Textarea 
                {...register('customer_description')}
              />
            </Field.Root>
          </Stack>
        </GridItem>
        <GridItem colSpan={4}></GridItem>
        <GridItem colSpan={1}>
            <Field.Root invalid={IsDirty('accounting_email')} inputMode="email" required={true}>
                <Field.Label><Field.RequiredIndicator /> Accounting Email</Field.Label>
                <Input 
                {...register('accounting_email')}
                />
                <Field.ErrorText>This field is required</Field.ErrorText>
            </Field.Root>
        </GridItem>
        <GridItem colSpan={1}>
            <Field.Root invalid={IsDirty('general_email')} inputMode="email" required={true}>
                <Field.Label><Field.RequiredIndicator /> General Email</Field.Label>
                <Input 
                {...register('general_email')}
                />
                <Field.ErrorText>This field is required</Field.ErrorText>
            </Field.Root>
        </GridItem>
        <GridItem>
            <Field.Root invalid={IsDirty('phone')} inputMode="tel" required={true}>
                <Field.Label><Field.RequiredIndicator /> Phone</Field.Label>
                <Input 
                {...register('phone')}
                />
                <Field.ErrorText>This field is required</Field.ErrorText>
          </Field.Root>
        </GridItem>
        <GridItem>
            <Field.Root inputMode="tel">
                <Field.Label>Fax</Field.Label>
                <Input 
                {...register('fax')}
                />
          </Field.Root>
        </GridItem>
        <GridItem>
            <Field.Root invalid={!!errors.website} inputMode="url">
            <Field.Label>Website</Field.Label>
            <Input 
              {...register('website')}
            />
          </Field.Root>
        </GridItem>
        <GridItem colSpan={6}></GridItem>
        <GridItem>
            <Field.Root invalid={!!errors.payment_terms} required={true}>
                <Field.Label><Field.RequiredIndicator /> Payment Terms</Field.Label>
                <CustomerPaymentTermsCombobox 
                ref={paymentTermsComboboxRef}
                dbKey={watch('payment_terms') ? [watch('payment_terms') as string] : []}
                control={control}
                name="payment_terms"
                error={errors.payment_terms}
                onChange={handleTermsSelect}
                onValidationChange={CheckFormValidity}
                />
          </Field.Root>
        </GridItem>
        <GridItem>
            <Field.Root invalid={!!errors.tax_rate}>
                <Field.Label>Tax Rate</Field.Label>
                <NumberInput.Root defaultValue="0" min={0}>
                <NumberInput.Control />
                <NumberInput.Input {...register('tax_rate')}/>
                </NumberInput.Root>
            </Field.Root>
        </GridItem>
        <GridItem>
            <Field.Root invalid={!!errors.is_taxable}>
                <Field.Label>&nbsp;</Field.Label>
                <Checkbox.Root checked={watch('is_taxable')} value={"1"}>
                <Checkbox.HiddenInput 
                    {...register('is_taxable')}
                />
                <Checkbox.Control />
                <Checkbox.Label>Taxable</Checkbox.Label>
                </Checkbox.Root>
            </Field.Root>
        </GridItem>

        <GridItem colSpan={6}>
            <PageActionsComponent 
                canSave={saveable || !hasWritePermission} 
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

export default NewCustomerPage;