"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';

import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Checkbox,
  Textarea,
} from '@chakra-ui/react';
import { useEffect, useState, useRef } from "react";
import { useParams } from 'next/navigation';
import { ChartOfAccountDto } from '@/models/chart-of-account-models';
import { chartOfAccountService } from '@/services/chart-of-account-service';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useRouter } from 'next/navigation';

function ViewChartOfAccountPage() {
    const { keycloak } = useKeycloak();
    const params = useParams();
    const router = useRouter();

    const [account, setAccount] = useState<ChartOfAccountDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [hasAccess, setHasAccess] = useState(true);

    const hasInitialized = useRef(false);

    const loadAccount = async () => {
        try {
            setLoading(true);
            const accountId = String(params.id);
            
            if (accountId == "") {
                setError('Invalid account ID');
                return;
            }

            const response = await chartOfAccountService.getByGuid(accountId, keycloak.token || "");
            if (response.success && response.data) {
                setAccount(response.data);
            } else {
                setError('Failed to load account');
            }
        } catch (err) {
            console.error('Error loading account:', err);
            setError('Error loading account');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if(keycloak.authenticated == false) return;

        if (hasInitialized.current) return;
        hasInitialized.current = true;
        
        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ChartOfAccountModule,
          ERPModulePermission.Read,
          realmRoles
        );

        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        loadAccount();
    }, [params.id, keycloak.authenticated]);

    if (loading) {
        return <div>Loading account...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!account) {
        return <div>Account not found</div>;
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
                <h1>View Account</h1>
            </GridItem>
            
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Account Number</Field.Label>
                    <Input 
                        value={account.account_number || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Account Name</Field.Label>
                    <Input 
                        value={account.account_name || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={4}></GridItem>
            
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Account Type</Field.Label>
                    <Input 
                        value={account.account_type_name || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Normal Balance</Field.Label>
                    <Input 
                        value={account.normal_balance_name || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={4}></GridItem>

            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Checkbox.Root checked={account.is_active} disabled>
                        <Checkbox.HiddenInput />
                        <Checkbox.Control />
                        <Checkbox.Label>Active</Checkbox.Label>
                    </Checkbox.Root>
                </Field.Root>
            </Stack>
            <GridItem colSpan={5}></GridItem>

            {account.parent_account_name && (
                <>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Parent Account</Field.Label>
                            <Input 
                                value={`${account.parent_account_number} - ${account.parent_account_name}`}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={5}></GridItem>
                </>
            )}

            <GridItem colSpan={3}>
                <Field.Root>
                    <Field.Label>Description</Field.Label>
                    <Textarea 
                        value={account.description || ''}
                        readOnly
                    />
                </Field.Root>
            </GridItem>
            <GridItem colSpan={3}></GridItem>
        </Grid>
    );
}

export default ViewChartOfAccountPage;
