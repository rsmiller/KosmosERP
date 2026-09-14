"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';

import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Badge,
} from '@chakra-ui/react';
import { useEffect, useState, useRef } from "react";
import { useParams } from 'next/navigation';
import { FinancialTransactionDto } from '@/models/financial-transaction-models';
import { financialTransactionService } from '@/services/financial-transaction-service';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useRouter } from 'next/navigation';

function ViewFinancialTransactionPage() {
    const { keycloak } = useKeycloak();
    const params = useParams();
    const router = useRouter();

    const [transaction, setTransaction] = useState<FinancialTransactionDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [hasAccess, setHasAccess] = useState(true);

    const hasInitialized = useRef(false);

    const loadTransaction = async () => {
        try {
            setLoading(true);
            const transactionId = String(params.id);
            
            if (transactionId == "") {
                setError('Invalid transaction ID');
                return;
            }

            const response = await financialTransactionService.getByGuid(transactionId, keycloak.token || "");
            if (response.success && response.data) {
                setTransaction(response.data);
            } else {
                setError('Failed to load transaction');
            }
        } catch (err) {
            console.error('Error loading transaction:', err);
            setError('Error loading transaction');
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
          ERPModules.FinancialTransactionModule,
          ERPModulePermission.Read,
          realmRoles
        );

        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        loadTransaction();
    }, [params.id, keycloak.authenticated]);

    const formatCurrency = (value: number | undefined) => {
        if (value === undefined) return '$0.00';
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
    };

    const formatDate = (value: string | undefined) => {
        if (!value) return '';
        return new Date(value).toLocaleDateString();
    };

    if (loading) {
        return <div>Loading transaction...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!transaction) {
        return <div>Transaction not found</div>;
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
                <h1>
                    View Financial Transaction 
                    {transaction.is_reversal && <Badge colorPalette="red" ml={2}>Reversal</Badge>}
                </h1>
            </GridItem>
            
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Transaction Date</Field.Label>
                    <Input 
                        value={formatDate(transaction.transaction_date)}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Transaction Type</Field.Label>
                    <Input 
                        value={transaction.transaction_type_name || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Fiscal Period</Field.Label>
                    <Input 
                        value={transaction.fiscal_period || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={3}></GridItem>
            
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Account Number</Field.Label>
                    <Input 
                        value={transaction.account_number || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Account Name</Field.Label>
                    <Input 
                        value={transaction.account_name || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={4}></GridItem>

            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Debit Amount</Field.Label>
                    <Input 
                        value={formatCurrency(transaction.debit_amount)}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Credit Amount</Field.Label>
                    <Input 
                        value={formatCurrency(transaction.credit_amount)}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Running Balance</Field.Label>
                    <Input 
                        value={formatCurrency(transaction.running_balance)}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={3}></GridItem>

            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Source Module</Field.Label>
                    <Input 
                        value={transaction.source_module || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Source ID</Field.Label>
                    <Input 
                        value={transaction.source_id?.toString() || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={4}></GridItem>

            <GridItem colSpan={3}>
                <Field.Root>
                    <Field.Label>Description</Field.Label>
                    <Input 
                        value={transaction.description || ''}
                        readOnly
                    />
                </Field.Root>
            </GridItem>
            <GridItem colSpan={3}></GridItem>
        </Grid>
    );
}

export default ViewFinancialTransactionPage;
