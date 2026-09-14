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
  NumberInput,
  Tabs,
} from '@chakra-ui/react';

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { CustomerEditCommand, CustomerDto, CustomerDeleteCommand } from '@/models/customer-models';
import CustomerPaymentTermsCombobox, { CustomerPaymentTermsComboboxRef } from '@/components/customer-payment-terms-combobox';
import DocumentsListComponent from '@/components/lists/documents-list-component';
import SalesOrdersListComponent from '@/components/lists/sales-orders-list-component';
import CommentsListComponent from '@/components/lists/comments-list-component';
import { customerService } from '@/services/customer-service';
import CustomerCategoriesCombobox, { CustomerCategoriesComboboxRef } from '@/components/customer-categories-combobox';
import ActivitiesListComponent from '@/components/lists/activities-list-component';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditCustomerPage() {
  const params = useParams();
  const router = useRouter();

  const { keycloak } = useKeycloak();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);
  const [hasDeletePermission, setHasDeletePermission] = useState(false);

  const [customer, setCustomer] = useState<CustomerDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);

  const paymentTermsComboboxRef = useRef<CustomerPaymentTermsComboboxRef>(null);
  const categoryComboboxRef = useRef<CustomerCategoriesComboboxRef>(null);
  const hasInitialized = useRef(false);
  
  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    setValue,
    watch,
    control,
  } = useForm<CustomerEditCommand>();

  
  const loadCustomer = async () => {
      try {
        setLoading(true);
        const customerId = String(params.id);
        
        if (customerId == "") {
          setError('Invalid customer ID');
          return;
        }

        const response = await customerService.getByGuid(customerId, keycloak.token || "");
        if (response.success && response.data) {
          //console.log(response)
          setCustomer(response.data);
          
          // Set form values with loaded data
          setValue('id', response.data.id as number);
          setValue('customer_name', response.data.customer_name || '');
          setValue('customer_description', response.data.customer_description || '');
          setValue('phone', response.data.phone || '');
          setValue('fax', response.data.fax || '');
          setValue('general_email', response.data.general_email || '');
          setValue('website', response.data.website || '');
          setValue('category', response.data.category || '');
          setValue('is_taxable', response.data.is_taxable || false);
          setValue('tax_rate', response.data.tax_rate || 0);
          setValue('payment_terms', response.data.payment_terms || '');

        } else {
          setError('Failed to load customer');
        }
      } catch (err) {
        console.error('Error loading customer:', err);
        setError('Error loading customer');
      } finally {
        setLoading(false);
      }
  };

  useEffect(() => {
    if (keycloak.authenticated == false) return;

    if (hasInitialized.current) return;
    hasInitialized.current = true;
    
    // Check permission
    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.CustomerModule,
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
      ERPModules.CustomerModule,
      ERPModulePermission.Edit,
      realmRoles
    );
    setHasEditPermission(canEdit);

    // Check Delete permission
    const canDelete = permissionsService.HasPermission(
      ERPModules.CustomerModule,
      ERPModulePermission.Delete,
      realmRoles
    );
    setHasDeletePermission(canDelete);
    console.log("Delete Permission: ", canDelete);

    loadCustomer();
  }, [params.id, setValue, keycloak.authenticated]);

  const handleDeleteClick = async () => {
    let command = new CustomerDeleteCommand();
    command.id = customer?.id;

    try {
      await customerService.delete(command, keycloak.token || "").then((response) => {
        if (response.success) {
          router.push("/erp/customers/");
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

    let command = new CustomerEditCommand();
    command.id = customer?.id;
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
    //return;

    try {
      await customerService.update(command, keycloak.token || "").then((response) => {
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
      console.log("WTF")
      canSave(false);
    }
  };

  const IsDirty = (formName: any) => {
    if (watch(formName) == undefined || watch(formName) == null || watch(formName) == '') {
      return true;
    }
    return false;
  };
  
  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  const handleTermsSelect = (result: any) => {
    //console.log(result)
    setValue('payment_terms', result?.key);
    CheckFormValidity();
  };

  const handleCategorySelect = (result: any) => {
    setValue('category', result?.value);
    CheckFormValidity();
  };

  if (loading) {
    return <div>Loading customer...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!customer) {
    return <div>Customer not found</div>;
  }

  const defaultColDef: ColDef = {
    flex: 1,
    filter: true,
    sortable: true,
  };

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
          <h1>Edit Customer</h1>
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
        <GridItem colSpan={5}></GridItem>
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
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={IsDirty('general_email')} inputMode="email" required={true}>
            <Field.Label><Field.RequiredIndicator /> Email</Field.Label>
            <Input 
              {...register('general_email')}
            />
            <Field.ErrorText>This field is required</Field.ErrorText>
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={IsDirty('phone')} inputMode="tel" required={true}>
            <Field.Label><Field.RequiredIndicator /> Phone</Field.Label>
            <Input 
              {...register('phone')}
            />
            <Field.ErrorText>This field is required</Field.ErrorText>
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.website} inputMode="url">
            <Field.Label>Website</Field.Label>
            <Input 
              {...register('website')}
            />
          </Field.Root>
        </Stack>
        <GridItem colSpan={3}></GridItem>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.payment_terms} required={true}>
            <Field.Label><Field.RequiredIndicator /> Payment Terms</Field.Label>
            <CustomerPaymentTermsCombobox 
              ref={paymentTermsComboboxRef}
              dbKey={watch('payment_terms') ? [watch('payment_terms') as string] : []}
              control={control}
              name="lead_stage"
              error={errors.payment_terms}
              onChange={handleTermsSelect}
              onValidationChange={CheckFormValidity}
            />
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.tax_rate}>
            <Field.Label>Tax Rate</Field.Label>
            <NumberInput.Root defaultValue="0" min={0}>
              <NumberInput.Control />
              <NumberInput.Input {...register('tax_rate')}/>
            </NumberInput.Root>
          </Field.Root>
        </Stack>
        <Stack gap="4" align="flex-start" maxW="md">
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
        </Stack>
        <GridItem colSpan={6}>
          <Tabs.Root lazyMount unmountOnExit defaultValue="documents">
            <Tabs.List>
              <Tabs.Trigger value="documents">Documents</Tabs.Trigger>
              <Tabs.Trigger value="recent-orders">Recent Orders</Tabs.Trigger>
              <Tabs.Trigger value="activities">Activities</Tabs.Trigger>
              <Tabs.Trigger value="comments">Comments</Tabs.Trigger>
            </Tabs.List>
            <Tabs.Content value="documents">
              <DocumentsListComponent />
            </Tabs.Content>
            <Tabs.Content value="recent-orders">
              <SalesOrdersListComponent customer_id={customer.id} onChange={() => {}} />
            </Tabs.Content>
            <Tabs.Content value="activities">
              <ActivitiesListComponent entity_id={customer.id} entity_type="customer" onChange={() => {}} />
            </Tabs.Content>
            <Tabs.Content value="comments">
              <CommentsListComponent />
            </Tabs.Content>
          </Tabs.Root>
        </GridItem>
        <GridItem colSpan={6}></GridItem>
        <GridItem colSpan={6}>
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

export default EditCustomerPage;