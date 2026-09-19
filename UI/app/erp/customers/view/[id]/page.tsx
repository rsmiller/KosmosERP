"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

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
import { useParams } from 'next/navigation';
import { CustomerDto } from '@/models/customer-models';
import DocumentsListComponent from '@/components/lists/documents-list-component';
import SalesOrdersListComponent from '@/components/lists/sales-orders-list-component';
import CommentsListComponent from '@/components/lists/comments-list-component';
import { customerService } from '@/services/customer-service';
import ActivitiesListComponent from '@/components/lists/activities-list-component';
import SubscriptionListComponent from '@/components/lists/subscription-list-component';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useRouter } from 'next/navigation';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewCustomerPage() {
  const params = useParams();
  const router = useRouter();

  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);

  const [customer, setCustomer] = useState<CustomerDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const hasInitialized = useRef(false);

  const loadCustomer = async () => {
      try {
        setLoading(true);
        const customerId = String(params.id);
        
        if (customerId == "") {
          setError('Invalid customer ID');
          return;
        }

        const response = await customerService.getByGuid(customerId, auth.token || "");
        if (response.success && response.data) {
          setCustomer(response.data);
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
    if(auth.authenticated == false) return;

    if (hasInitialized.current) return;
    hasInitialized.current = true;
    
    // Check permission
    const realmRoles = auth.roles || [];
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

    loadCustomer();
  }, [params.id, auth.authenticated]);

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

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
    <Grid
      templateColumns="repeat(5, 2fr)"
      gap={6}
      display="grid"
      width="100%"
      p="auto"
      m="auto"
    >
      <GridItem colSpan={6}>
        <h1>View Customer</h1>
      </GridItem>
      
      <GridItem colSpan={2}>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root>
            <Field.Label>Customer Name</Field.Label>
            <Input 
              value={customer.customer_name || ''}
              readOnly={true}
            />
          </Field.Root>
        </Stack>
      </GridItem>
      <GridItem colSpan={1}>
        <Field.Root>
          <Field.Label>Category</Field.Label>
          <Input 
            value={customer.category || ''}
            disabled={true}
          />
        </Field.Root>
      </GridItem>
      <GridItem colSpan={5}></GridItem>
      <GridItem colSpan={2}>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root>
            <Field.Label>Description</Field.Label>
            <Textarea 
              value={customer.customer_description || ''}
              readOnly={true}
            />
          </Field.Root>
        </Stack>
      </GridItem>
      <GridItem colSpan={4}></GridItem>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Email</Field.Label>
          <Input 
            value={customer.general_email || ''}
            readOnly={true}
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Phone</Field.Label>
          <Input 
            value={customer.phone || ''}
            readOnly={true}
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Website</Field.Label>
          <Input 
            value={customer.website || ''}
            readOnly={true}
          />
        </Field.Root>
      </Stack>
      <GridItem colSpan={3}></GridItem>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Payment Terms</Field.Label>
          <Input 
            value={customer.payment_terms_name || ''}
            readOnly={true}
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Tax Rate</Field.Label>
          <NumberInput.Root defaultValue={customer.tax_rate?.toString() || "0"} readOnly={true}>
            <NumberInput.Control />
            <NumberInput.Input />
          </NumberInput.Root>
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>&nbsp;</Field.Label>
          <Checkbox.Root checked={customer.is_taxable || false} readOnly={true}>
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
            <Tabs.Trigger value="subscriptions">Subscriptions</Tabs.Trigger>
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
          <Tabs.Content value="subscriptions">
            <SubscriptionListComponent customer_id={customer.id} onChange={() => {}} />
          </Tabs.Content>
          <Tabs.Content value="comments">
            <CommentsListComponent />
          </Tabs.Content>
        </Tabs.Root>
      </GridItem>
    </Grid>
  )
}

export default ViewCustomerPage;

