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
  Tabs,
} from '@chakra-ui/react';

import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useState, useRef } from "react";
import { useParams } from 'next/navigation';
import { VendorDto } from '@/models/vendor-models';
import DocumentsListComponent from '@/components/lists/documents-list-component';
import PurchaseOrdersListComponentPage from '@/components/lists/purchase-orders-list-component';
import CommentsListComponent from '@/components/lists/comments-list-component';
import { vendorService } from '@/services/vendor-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useKeycloak } from '@react-keycloak/web';
import { useRouter } from 'next/navigation';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewVendorsPage() {
  const { keycloak } = useKeycloak();
  const router = useRouter();
  const params = useParams();
  const [hasAccess, setHasAccess] = useState(true);

  const [vendor, setVendor] = useState<VendorDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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
  }, [keycloak.authenticated, router]);

  const loadVendor = async () => {
      try {
        setLoading(true);
        const vendorId = String(params.id);
        
        if (vendorId == "") {
          setError('Invalid vendor ID');
          return;
        }

        const response = await vendorService.getByGuid(vendorId, keycloak?.token || "");

        if (response.success && response.data) {
          setVendor(response.data);
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
    if (keycloak.authenticated == false) return;

    if (hasInitialized.current) return;
    hasInitialized.current = true;
    

    loadVendor();
  }, [params.id, keycloak.authenticated]);

  const getApprovedDate = () => {
    return vendor?.approved_on ? new Date(vendor.approved_on) : undefined;
  };

  const getAuditDate = () => {
    return vendor?.audit_on ? new Date(vendor.audit_on) : undefined;
  };

  const getRetiredDate = () => {
    return vendor?.retired_on ? new Date(vendor.retired_on) : undefined;
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
        <h1>View Vendor</h1>
      </GridItem>
      
      <GridItem colSpan={2}>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root>
            <Field.Label>Vendor Name</Field.Label>
            <Input 
              value={vendor.vendor_name || ''}
              readOnly
            />
          </Field.Root>
        </Stack>
      </GridItem>
      <GridItem colSpan={1}>
        <Field.Root>
          <Field.Label>Category</Field.Label>
          <Input 
            value={vendor.category || ''}
            readOnly
          />
        </Field.Root>
      </GridItem>
      <GridItem colSpan={3}></GridItem>
      <GridItem colSpan={2}>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root>
            <Field.Label>Description</Field.Label>
            <Textarea 
              value={vendor.vendor_description || ''}
              readOnly
            />
          </Field.Root>
        </Stack>
      </GridItem>
      <GridItem colSpan={4}></GridItem>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Email</Field.Label>
          <Input 
            value={vendor.general_email || ''}
            readOnly
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Phone</Field.Label>
          <Input 
            value={vendor.phone || ''}
            readOnly
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Fax</Field.Label>
          <Input 
            value={vendor.fax || ''}
            readOnly
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Website</Field.Label>
          <Input 
            value={vendor.website || ''}
            readOnly
          />
        </Field.Root>
      </Stack>
      <GridItem colSpan={2}></GridItem>

      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>&nbsp;</Field.Label>
          <Checkbox.Root checked={vendor.is_critial_vendor || false} disabled>
            <Checkbox.Control />
            <Checkbox.Label>Is Critical</Checkbox.Label>
          </Checkbox.Root>
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Approved Date</Field.Label>
          <Input 
            value={getApprovedDate() ? getApprovedDate()?.toLocaleDateString() : ''}
            readOnly
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Audit Date</Field.Label>
          <Input 
            value={getAuditDate() ? getAuditDate()?.toLocaleDateString() : ''}
            readOnly
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Retired Date</Field.Label>
          <Input 
            value={getRetiredDate() ? getRetiredDate()?.toLocaleDateString() : ''}
            readOnly
          />
        </Field.Root>
      </Stack>

      <GridItem colSpan={1}></GridItem>

      <GridItem colSpan={2}>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root>
            <Field.Label>Address ID</Field.Label>
            <Input 
              value={vendor.address_id || ''}
              readOnly
            />
          </Field.Root>
        </Stack>
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
    </Grid>
  )
}

export default ViewVendorsPage; 