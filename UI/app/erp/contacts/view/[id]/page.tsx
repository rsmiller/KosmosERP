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
  Tabs,
} from '@chakra-ui/react';

import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from 'next/navigation';
import { ContactDto } from '@/models/contact-models';
import CustomerCombobox from '@/components/customer-combobox';
import { contactService } from '@/services/contact-service';
import ActivitiesListComponent from '@/components/lists/activities-list-component';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewContactPage() {
    const params = useParams();
    const router = useRouter();
    const { keycloak } = useKeycloak();
    const [hasAccess, setHasAccess] = useState(true);

    const [contact, setContact] = useState<ContactDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const hasInitialized = useRef(false);

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
        if(!keycloak.authenticated) return;
        
        if (hasInitialized.current) return;
        hasInitialized.current = true;

        // Check permission
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

        loadContact();
    }, [params.id, keycloak.authenticated]);

    if (loading) {
        return <div>Loading contact...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!contact) {
        return <div>Contact not found</div>;
    }

    if (!hasAccess) {
        return <div>Redirecting...</div>;
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
                <h1>View Contact</h1>
            </GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Customer</Field.Label>
                    <CustomerCombobox 
                        dbKey={contact.customer_id}
                        onChange={() => {}}
                        control={undefined}
                        name="customer_id"
                        error={undefined}
                        disabled={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={5}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>First Name</Field.Label>
                    <Input 
                        value={contact.first_name || ''}
                        disabled={true}
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Last Name</Field.Label>
                    <Input 
                        value={contact.last_name || ''}
                        disabled={true}
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Title</Field.Label>
                    <Input 
                        value={contact.title || ''}
                        disabled={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={3}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Email</Field.Label>
                    <Input 
                        value={contact.email || ''}
                        disabled={true}
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Phone</Field.Label>
                    <Input 
                        value={contact.phone || ''}
                        disabled={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={4}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Cell Phone</Field.Label>
                    <Input 
                        value={contact.cell_phone || ''}
                        disabled={true}
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
        </Grid>
    )
}

export default ViewContactPage;
