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
  Textarea,
  Checkbox,
  Tabs,
} from '@chakra-ui/react';

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useRef, useState } from "react";
import { useParams, useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { VendorEditCommand, VendorDto, VendorDeleteCommand } from '@/models/vendor-models';
import DocumentsListComponent from '@/components/lists/documents-list-component';
import PurchaseOrdersListComponentPage from '@/components/lists/purchase-orders-list-component';
import CommentsListComponent from '@/components/lists/comments-list-component';
import DatePicker from 'react-datepicker';
import { vendorService } from '@/services/vendor-service';
import NewAddressBlock, { NewAddressBlockRef, NewAddressBlockResponse } from '@/components/new-address-block';
import VendorCategoriesCombobox, { VendorCategoriesComboboxRef } from '@/components/vendor-categories-combobox';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditVendorsPage() {
  const { keycloak } = useKeycloak();
  const params = useParams();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);
  const [hasDeletePermission, setHasDeletePermission] = useState(false);

  const [vendor, setVendor] = useState<VendorDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);
  const [addressResponse, setAddressResponse] = useState<NewAddressBlockResponse | null>(null);

  const addressBlockRef = useRef<NewAddressBlockRef>(null);
  const categoryComboboxRef = useRef<VendorCategoriesComboboxRef>(null);
  const hasInitialized = useRef(false);

  useEffect(() => {
    if(keycloak.authenticated == false) return;

    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.VendorModule,
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
      ERPModules.VendorModule,
      ERPModulePermission.Edit,
      realmRoles
    );
    setHasEditPermission(canEdit);

    // Check Delete permission
    const canDelete = permissionsService.HasPermission(
      ERPModules.VendorModule,
      ERPModulePermission.Delete,
      realmRoles
    );
    setHasDeletePermission(canDelete);
  }, [keycloak.authenticated, router]);

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    setValue,
    watch,
    control,
  } = useForm<VendorEditCommand>();


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

  const loadVendor = async () => {
      try {
        setLoading(true);
        const vendorId = String(params.id);
        
        if (vendorId == "") {
          setError('Invalid vendor ID');
          return;
        }

        const response = await vendorService.getByGuid(vendorId, keycloak?.token || "");
        //console.log(response);

        if (response.success && response.data) {
          setVendor(response.data);
          
          // Set form values with loaded data
          setValue('id', response.data.id);
          setValue('vendor_name', response.data.vendor_name || '');
          setValue('vendor_description', response.data.vendor_description || '');
          setValue('phone', response.data.phone || '');
          setValue('fax', response.data.fax || '');
          setValue('general_email', response.data.general_email || '');
          setValue('website', response.data.website || '');
          setValue('category', response.data.category || '');
          setValue('is_critial_vendor', response.data.is_critial_vendor || false);
          setValue('approved_on', response.data.approved_on || undefined);
          setValue('audit_on', response.data.audit_on || undefined);
          setValue('retired_on', response.data.retired_on || undefined);
          setValue('address_id', response.data.address_id);
        } else {
          setError('Failed to load vendor');
        }
      } catch (err) {
        console.error('Error loading vendor:', err);
        setError('Error loading vendor');
      } finally {
        setLoading(false);
      }
  };

  useEffect(() => {
    if(keycloak.authenticated == false) return;

    if (hasInitialized.current) return;
    hasInitialized.current = true;
    

    loadVendor();
  }, [params.id, setValue, keycloak.authenticated]);


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

  const handleDeleteClick = async () => {
    let command = new VendorDeleteCommand();
    command.id = vendor?.id;

    try {
      await vendorService.delete(command, keycloak?.token || "").then((response) => {
        if (response.success) {
          router.push("/erp/vendors/");
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

  const handleSaveClick = async () => {
    setSuccessSaved(false);

    let command = new VendorEditCommand();
    command.id = vendor?.id;
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
      command.address = undefined;
    }

    //console.log(command);
    //return;

    try {
      await vendorService.update(command, keycloak?.token || "").then((response) => {
        if (response.success) {
          setSuccessSaved(true);
          setFailedSaved(false);
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

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  if (loading) {
    return <div>Loading vendor...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!vendor) {
    return <div>Vendor not found</div>;
  }

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
          <h1>Edit Vendor</h1>
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
          <Field.Root invalid={!!errors.fax}>
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

        <GridItem colSpan={6}>
          <Tabs.Root lazyMount unmountOnExit defaultValue="documents">
            <Tabs.List>
              <Tabs.Trigger value="documents">Documents</Tabs.Trigger>
              <Tabs.Trigger value="recent-orders">Recent Purchase Orders</Tabs.Trigger>
              <Tabs.Trigger value="comments">Comments</Tabs.Trigger>
            </Tabs.List>
            <Tabs.Content value="documents">
              <DocumentsListComponent />
            </Tabs.Content>
            <Tabs.Content value="recent-orders">
              <PurchaseOrdersListComponentPage vendor_id={vendor.id} onChange={() => {}} />
            </Tabs.Content>
            <Tabs.Content value="comments">
              <CommentsListComponent />
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

export default EditVendorsPage;