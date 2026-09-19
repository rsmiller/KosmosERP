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
} from '@chakra-ui/react';

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useRef, useState } from "react";
import { useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { VendorCreateCommand } from '@/models/vendor-models';
import DatePicker from 'react-datepicker';
import { vendorService } from '@/services/vendor-service';
import NewAddressBlock, { NewAddressBlockRef, NewAddressBlockResponse } from '@/components/new-address-block';
import { AddressEditCommand } from '@/models/address-models';
import VendorCategoriesCombobox, { VendorCategoriesComboboxRef } from '@/components/vendor-categories-combobox';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewVendorsPage() {
  const auth = useAuth();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasWritePermission, setHasWritePermission] = useState(false);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);
  const [addressResponse, setAddressResponse] = useState<NewAddressBlockResponse | null>(null);

  const addressBlockRef = useRef<NewAddressBlockRef>(null);
  const categoryComboboxRef = useRef<VendorCategoriesComboboxRef>(null);

  useEffect(() => {
    if(auth.authenticated == false) return;

    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.VendorModule,
      ERPModulePermission.Write,
      realmRoles
    );
    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }
    setHasWritePermission(true);
  }, [auth.authenticated, router]);

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    setValue,
    watch,
    control,
  } = useForm<VendorCreateCommand>();


  const CheckFormValidity = () => {
    const hasRequiredFields = Boolean(watch('vendor_name') && watch('general_email') && watch('vendor_description') && watch('phone') && watch('category'));

    let addressBlockValid = false;

    if(addressResponse)
    {
      addressBlockValid = addressResponse.isValid;
    }

    const allValid = addressBlockValid && isValid && hasRequiredFields;

    //console.log("PAGE hasRequiredFields: ", hasRequiredFields);
    //console.log("PAGE addressBlock", addressBlockValid)
    //console.log("PAGE allValid: ", allValid);

    canSave(allValid)
  };

  useEffect(() => {
    CheckFormValidity();
  }, [addressResponse]);


  const IsDirty = (formName: any) => {
    if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
    {
        return true;
    }

    return false;
  }

  const getApprovedDate = () => {
    const approvedOn = watch('approved_on');
    return approvedOn ? new Date(approvedOn) : undefined;
  };

  const getAuditDate = () => {
    const auditOn = watch('audit_on');
    return auditOn ? new Date(auditOn) : undefined;
  };

  const getRetiredDate = () => {
    const retiredOn = watch('retired_on');
    return retiredOn ? new Date(retiredOn) : undefined;
  };

  const handleApprovedDateChange = (date: Date | null) => {
    setValue('approved_on', date || undefined);
    CheckFormValidity();
  };

  const handleAuditDateChange = (date: Date | null) => {
    setValue('audit_on', date || undefined);
    CheckFormValidity();
  };

  const handleRetiredDateChange = (date: Date | null) => {
    setValue('retired_on', date || undefined);
    CheckFormValidity();
  };

  const handleSaveClick = async () => {
    setSuccessSaved(false);

    let command = new VendorCreateCommand();
    command.vendor_name = watch('vendor_name');
    command.vendor_description = watch('vendor_description');
    command.phone = watch('phone');
    command.fax = watch('fax');
    command.general_email = watch('general_email');
    command.website = watch('website');
    command.category = watch('category');
    command.is_critial_vendor = Boolean(watch('is_critial_vendor'));
    command.approved_on = watch('approved_on');
    command.audit_on = watch('audit_on');
    command.retired_on = watch('retired_on');
    command.address_id = watch('address_id');


    if(addressResponse && addressResponse.editCommand)
    {
      command.address = addressResponse.editCommand;
    }
    else
    {
      command.address = new AddressEditCommand();
    }

    console.log(command);
    //return;

    try {
      await vendorService.create(command, auth.token || "").then((response) => {
        if (response.success) {
          setSuccessSaved(true);
          setFailedSaved(false);
          // Route to vendors list on successful save
          router.push("/erp/vendors");
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

    
  const handleCategorySelect = (result: any) => {
    setValue('category', result?.value);
    CheckFormValidity();
  };

  const addressBlockChange = (command: any) => 
  {
  }

  const addressBlockValidationChange = (response: NewAddressBlockResponse) =>
  {
    //console.log("ADDRESS BLOCK: ", response);
    setAddressResponse(response);
  }



  return (
    <form onChange={CheckFormValidity}>
      <Grid
        templateColumns="repeat(5, 2fr)"
        gap={6}
        display="grid"
        width="100%"
        p="auto"
        m="auto"
      >
        <GridItem colSpan={6}>
          <h1>New Vendor</h1>
        </GridItem>
        
        <GridItem colSpan={2}>
          <Stack gap="4" align="flex-start" maxW="md">
            <Field.Root invalid={IsDirty('vendor_name')} required={true}>
              <Field.Label><Field.RequiredIndicator /> Vendor Name</Field.Label>
              <Input 
                {...register('vendor_name')}
              />
              <Field.ErrorText>This field is required</Field.ErrorText>
            </Field.Root>
          </Stack>
        </GridItem>
        <GridItem colSpan={1}>
            <Field.Root invalid={!!errors.category} required={true}>
            <Field.Label><Field.RequiredIndicator /> Category</Field.Label>
                <VendorCategoriesCombobox
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
            <Field.Root invalid={IsDirty('vendor_description')} required={true}>
              <Field.Label><Field.RequiredIndicator /> Description</Field.Label>
              <Textarea 
                {...register('vendor_description')}
              />
            </Field.Root>
          </Stack>
        </GridItem>
        <GridItem colSpan={4}></GridItem>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={IsDirty('general_email')} inputMode="email" required={true}>
            <Field.Label><Field.RequiredIndicator /> Email</Field.Label>
            <Input 
              {...register('general_email')}
            />
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.phone} inputMode="tel" required={true}>
            <Field.Label><Field.RequiredIndicator /> Phone</Field.Label>
            <Input 
              {...register('phone')}
            />
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={IsDirty('fax')}>
            <Field.Label>Fax</Field.Label>
            <Input 
              {...register('fax')}
            />
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={IsDirty('website')} inputMode="url">
            <Field.Label>Website</Field.Label>
            <Input 
              {...register('website')}
            />
          </Field.Root>
        </Stack>
        <GridItem colSpan={2}></GridItem>

        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root>
            <Field.Label>&nbsp;</Field.Label>
            <Checkbox.Root >
              <Checkbox.HiddenInput {...register('is_critial_vendor')}/>
              <Checkbox.Control />
              <Checkbox.Label>Is Critical</Checkbox.Label>
            </Checkbox.Root>
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={IsDirty('approved_on')}>
            <Field.Label>Approved Date</Field.Label>
            <DatePicker 
              selected={getApprovedDate()}
              onChange={handleApprovedDateChange}
              dateFormat="MM/dd/yyyy"
              placeholderText="Select date"
            />
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={IsDirty('audit_on')}>
            <Field.Label>Audit Date</Field.Label>
            <DatePicker 
              selected={getAuditDate()}
              onChange={handleAuditDateChange}
              dateFormat="MM/dd/yyyy"
              placeholderText="Select date"
            />
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root>
            <Field.Label>Retired Date</Field.Label>
            <DatePicker 
              selected={getRetiredDate()}
              onChange={handleRetiredDateChange}
              dateFormat="MM/dd/yyyy"
              placeholderText="Select date"
            />
          </Field.Root>
        </Stack>

        <GridItem colSpan={1}></GridItem>

        <GridItem colSpan={2}>
          <NewAddressBlock
              ref={addressBlockRef}
              address_id={watch('address_id')}
              control={control}
              name="address_id"
              error={errors.address_id}
              onValidationChange={addressBlockValidationChange}
              onChange={addressBlockChange} />
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

export default NewVendorsPage;