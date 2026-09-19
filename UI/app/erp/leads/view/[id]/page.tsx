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
import { LeadDto } from '@/models/lead-models';
import { leadService } from '@/services/lead-service';
import { CountryDto } from '@/models/country-models';
import { countryService } from '@/services/country-service';
import ActivitiesListComponent from '@/components/lists/activities-list-component';
import { useAuth } from '@/lib/auth/auth-context';

import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewLeadPage() {
    const params = useParams();
    const router = useRouter();
    
    const auth = useAuth();
    const [hasAccess, setHasAccess] = useState(true);
    
    const [lead, setLead] = useState<LeadDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [countryId, setCountryId] = useState<number | null>(null);
    const hasInitialized = useRef(false);

    
    const loadLead = async () => {
        try {
            setLoading(true);
            const leadId = String(params.id);
            
            if (leadId == "") {
                setError('Invalid lead ID');
                return;
            }

            const response = await leadService.getByGuid(leadId, auth.token || "");
            if (response.success && response.data) {
                setLead(response.data);
                
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
        if(auth.authenticated == false) return;

        const realmRoles = auth.roles || [];
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

        if (hasInitialized.current) return;

        hasInitialized.current = true;

        
        loadLead();
    }, [params.id, auth.authenticated]);

    const fetchCountry = async (iso: string): Promise<CountryDto | undefined> => {
        try {
            setLoading(true);
            const response = await countryService.getByISOAsync(iso, auth.token || "");
            
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
        <Grid
            templateColumns="repeat(5, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
        >
            <GridItem colSpan={6}>
                <h1>View Lead</h1>
            </GridItem>
            <GridItem colSpan={1}>
                <Field.Root>
                    <Field.Label>Lead Stage</Field.Label>
                    <Input 
                        value={lead.stage_name || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </GridItem>
            <GridItem colSpan={5}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Company Name</Field.Label>
                    <Input 
                        value={lead.company_name || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={5}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>First Name</Field.Label>
                    <Input 
                        value={lead.first_name || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Last Name</Field.Label>
                    <Input 
                        value={lead.last_name || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Title</Field.Label>
                    <Input 
                        value={lead.title || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={3}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Email</Field.Label>
                    <Input 
                        value={lead.email || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Phone</Field.Label>
                    <Input 
                        value={lead.phone || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={4}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Cell Phone</Field.Label>
                    <Input 
                        value={lead.cell_phone || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={5}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Street Address 1</Field.Label>
                    <Input 
                        value={lead.address_line1 || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={5}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Street Address 2</Field.Label>
                    <Input 
                        value={lead.address_line2 || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={5}></GridItem>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>City</Field.Label>
                    <Input 
                        value={lead.city || ''}
                        readOnly={true}
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Postal Code</Field.Label>
                    <Input 
                        value={lead.zip || ''}
                        readOnly={true}
                    />
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
                        <Stack gap="4" align="flex-start" maxW="md">
                            <Field.Root>
                                <Field.Label>State</Field.Label>
                                <Input 
                                    value={lead.state || ''}
                                    readOnly={true}
                                />
                            </Field.Root>
                        </Stack>
                        <Stack gap="4" align="flex-start" maxW="md">
                            <Field.Root>
                                <Field.Label>Country</Field.Label>
                                <Input 
                                    value={lead.country || ''}
                                    readOnly={true}
                                />
                            </Field.Root>
                        </Stack>
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
        </Grid>
    )
}

export default ViewLeadPage;
