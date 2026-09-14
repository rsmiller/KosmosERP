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
  Badge,
} from '@chakra-ui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { useParams } from 'next/navigation';
import { JournalEntryHeaderDto, JournalEntryLineDto } from '@/models/journal-entry-models';
import { journalEntryService } from '@/services/journal-entry-service';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useRouter } from 'next/navigation';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewJournalEntryPage() {
    const { keycloak } = useKeycloak();
    const params = useParams();
    const router = useRouter();

    const [entry, setEntry] = useState<JournalEntryHeaderDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [hasAccess, setHasAccess] = useState(true);

    const hasInitialized = useRef(false);

    const loadEntry = async () => {
        try {
            setLoading(true);
            const entryId = String(params.id);
            
            if (entryId == "") {
                setError('Invalid entry ID');
                return;
            }

            const response = await journalEntryService.getByGuid(entryId, keycloak.token || "");
            if (response.success && response.data) {
                setEntry(response.data);
            } else {
                setError('Failed to load journal entry');
            }
        } catch (err) {
            console.error('Error loading journal entry:', err);
            setError('Error loading journal entry');
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
          ERPModules.JournalEntryModule,
          ERPModulePermission.Read,
          realmRoles
        );

        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        loadEntry();
    }, [params.id, keycloak.authenticated]);

    const formatCurrency = (value: number | undefined) => {
        if (value === undefined) return '$0.00';
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
    };

    const formatDate = (value: string | undefined) => {
        if (!value) return '';
        return new Date(value).toLocaleDateString();
    };

    const colDefs = useMemo<ColDef<JournalEntryLineDto>[]>(() => [
        { field: "account_number", headerName: "Account #" },
        { field: "account_name", headerName: "Account Name" },
        { 
            field: "debit_amount", 
            headerName: "Debit",
            valueFormatter: (params) => formatCurrency(params.value)
        },
        { 
            field: "credit_amount", 
            headerName: "Credit",
            valueFormatter: (params) => formatCurrency(params.value)
        },
        { field: "description", headerName: "Description" }
    ], []);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: false,
    };

    if (loading) {
        return <div>Loading journal entry...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!entry) {
        return <div>Journal entry not found</div>;
    }

    const getStatusBadge = () => {
        if (entry.is_reversed) {
            return <Badge colorPalette="red">Reversed</Badge>;
        }
        return entry.is_posted 
            ? <Badge colorPalette="green">Posted</Badge> 
            : <Badge colorPalette="yellow">Draft</Badge>;
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
                <h1>View Journal Entry {getStatusBadge()}</h1>
            </GridItem>
            
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Entry Number</Field.Label>
                    <Input 
                        value={entry.entry_number || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Entry Date</Field.Label>
                    <Input 
                        value={formatDate(entry.entry_date)}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
                <Field.Root>
                    <Field.Label>Fiscal Period</Field.Label>
                    <Input 
                        value={entry.fiscal_period || ''}
                        readOnly
                    />
                </Field.Root>
            </Stack>
            <GridItem colSpan={3}></GridItem>

            {entry.is_posted && (
                <>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Posted On</Field.Label>
                            <Input 
                                value={formatDate(entry.posted_on || undefined)}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Posted By</Field.Label>
                            <Input 
                                value={entry.posted_by_name || ''}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={4}></GridItem>
                </>
            )}

            <GridItem colSpan={3}>
                <Field.Root>
                    <Field.Label>Description</Field.Label>
                    <Textarea 
                        value={entry.description || ''}
                        readOnly
                    />
                </Field.Root>
            </GridItem>
            <GridItem colSpan={3}></GridItem>

            <GridItem colSpan={6}>
                <h2>Lines</h2>
                <div style={{ width: "100%", height: "300px" }}>
                    <AgGridReact
                        rowData={entry.lines || []}
                        columnDefs={colDefs}
                        defaultColDef={defaultColDef}
                    />
                </div>
            </GridItem>

            <GridItem colSpan={6}>
                <Grid templateColumns="repeat(3, 1fr)" gap={4}>
                    <div>
                        <strong>Total Debits:</strong> {formatCurrency(entry.total_debits)}
                    </div>
                    <div>
                        <strong>Total Credits:</strong> {formatCurrency(entry.total_credits)}
                    </div>
                    <div>
                        <strong>Reference:</strong> {entry.reference_type_name || 'Manual Entry'}
                    </div>
                </Grid>
            </GridItem>
        </Grid>
    );
}

export default ViewJournalEntryPage;
