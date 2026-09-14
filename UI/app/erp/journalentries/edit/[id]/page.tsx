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
  Button,
  Badge
} from '@chakra-ui/react';

import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { useParams, useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { 
  JournalEntryHeaderEditCommand, 
  JournalEntryHeaderDeleteCommand,
  JournalEntryHeaderDto,
  JournalEntryLineDto,
  JournalEntryLineCreateCommand,
  JournalEntryLineEditCommand,
  JournalEntryLineDeleteCommand,
  JournalEntryPostCommand,
  JournalEntryReverseCommand
} from '@/models/journal-entry-models';
import { journalEntryService } from '@/services/journal-entry-service';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import DatePicker from 'react-datepicker';
import "react-datepicker/dist/react-datepicker.css";
import { MdDelete } from 'react-icons/md';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditJournalEntryPage() {
    const { keycloak } = useKeycloak();
    const params = useParams();
    const router = useRouter();

    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);
    const [hasDeletePermission, setHasDeletePermission] = useState(false);
    const [entry, setEntry] = useState<JournalEntryHeaderDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    const [entryDate, setEntryDate] = useState<Date | null>(null);
    const [lines, setLines] = useState<JournalEntryLineDto[]>([]);
    
    const hasInitialized = useRef(false);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
    } = useForm<JournalEntryHeaderEditCommand>();

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
                setValue('id', response.data.id);
                setValue('description', response.data.description);
                setValue('fiscal_period', response.data.fiscal_period);
                setEntryDate(response.data.entry_date ? new Date(response.data.entry_date) : null);
                setLines(response.data.lines || []);
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
        if (keycloak.authenticated == false) return;

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.JournalEntryModule,
          ERPModulePermission.Edit,
          realmRoles
        );

        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasEditPermission(true);
        setHasDeletePermission(permissionsService.HasPermission(
          ERPModules.JournalEntryModule,
          ERPModulePermission.Delete,
          realmRoles
        ));

        loadEntry();
    }, [params.id, keycloak.authenticated]);

    const handleDeleteClick = async () => {
        if (!entry) return;

        let deleteCommand = new JournalEntryHeaderDeleteCommand();
        deleteCommand.id = entry.id;

        try {
            await journalEntryService.delete(deleteCommand, keycloak.token || "").then((response) => {
                if(response.success) {
                    router.push("/erp/journalentries/");
                } else {
                    setFailedSaved(true);
                }
            });
        } catch(e) {
            setFailedSaved(true);
        }
    };

    const handleSaveClick = async () => {
        setSuccessSaved(false);

        let command = new JournalEntryHeaderEditCommand();
        command.id = entry?.id;
        command.entry_date = entryDate?.toISOString();
        command.description = watch('description');
        command.fiscal_period = entryDate ? `${entryDate.getFullYear()}-${String(entryDate.getMonth() + 1).padStart(2, '0')}` : undefined;

        try {
            await journalEntryService.update(command, keycloak.token || "").then((response) => {
                if(response.success) {
                    setSuccessSaved(true);
                    setFailedSaved(false);
                } else {
                    setSuccessSaved(false);
                    setFailedSaved(true);
                }
            });
        } catch(e) {
            setSuccessSaved(false);
            setFailedSaved(true);
        }
    };

    const handlePostClick = async () => {
        if (!entry) return;

        let postCommand = new JournalEntryPostCommand();
        postCommand.id = entry.id;

        try {
            await journalEntryService.post(postCommand, keycloak.token || "").then((response) => {
                if(response.success) {
                    setSuccessSaved(true);
                    setFailedSaved(false);
                    loadEntry(); // Reload to get updated status
                } else {
                    setSuccessSaved(false);
                    setFailedSaved(true);
                }
            });
        } catch(e) {
            setSuccessSaved(false);
            setFailedSaved(true);
        }
    };

    const handleReverseClick = async () => {
        if (!entry) return;

        let reverseCommand = new JournalEntryReverseCommand();
        reverseCommand.id = entry.id;
        reverseCommand.reversal_date = new Date().toISOString();
        reverseCommand.reversal_description = `Reversal of ${entry.entry_number}`;

        try {
            await journalEntryService.reverse(reverseCommand, keycloak.token || "").then((response) => {
                if(response.success && response.data) {
                    setSuccessSaved(true);
                    setFailedSaved(false);
                    // Navigate to the reversal entry
                    router.push("/erp/journalentries/view/" + response.data.guid);
                } else {
                    setSuccessSaved(false);
                    setFailedSaved(true);
                }
            });
        } catch(e) {
            setSuccessSaved(false);
            setFailedSaved(true);
        }
    };

    const addLine = async () => {
        if (!entry) return;

        let lineCmd = new JournalEntryLineCreateCommand();
        lineCmd.journal_entry_header_id = entry.id;
        lineCmd.debit_amount = 0;
        lineCmd.credit_amount = 0;

        try {
            await journalEntryService.createLine(lineCmd, keycloak.token || "").then((response) => {
                if(response.success && response.data) {
                    setLines([...lines, response.data]);
                    CheckFormValidity();
                }
            });
        } catch(e) {
            console.error('Error adding line:', e);
        }
    };

    const removeLine = async (lineId: number) => {
        let deleteCmd = new JournalEntryLineDeleteCommand();
        deleteCmd.id = lineId;

        try {
            await journalEntryService.deleteLine(deleteCmd, keycloak.token || "").then((response) => {
                if(response.success) {
                    setLines(lines.filter(line => line.id !== lineId));
                    CheckFormValidity();
                }
            });
        } catch(e) {
            console.error('Error removing line:', e);
        }
    };

    const updateLine = async (lineId: number, field: keyof JournalEntryLineDto, value: any) => {
        const line = lines.find(l => l.id === lineId);
        if (!line) return;

        let editCmd = new JournalEntryLineEditCommand();
        editCmd.id = lineId;
        editCmd.chart_of_account_id = field === 'chart_of_account_id' ? value : line.chart_of_account_id;
        editCmd.debit_amount = field === 'debit_amount' ? value : line.debit_amount;
        editCmd.credit_amount = field === 'credit_amount' ? value : line.credit_amount;
        editCmd.description = field === 'description' ? value : line.description;

        try {
            await journalEntryService.updateLine(editCmd, keycloak.token || "").then((response) => {
                if(response.success && response.data) {
                    const updatedLine = response.data;
                    setLines(lines.map(l => l.id === lineId ? updatedLine : l));
                    CheckFormValidity();
                }
            });
        } catch(e) {
            console.error('Error updating line:', e);
        }
    };

    const getTotalDebits = () => lines.reduce((sum, line) => sum + (line.debit_amount || 0), 0);
    const getTotalCredits = () => lines.reduce((sum, line) => sum + (line.credit_amount || 0), 0);
    const isBalanced = () => Math.abs(getTotalDebits() - getTotalCredits()) < 0.01;

    const CheckFormValidity = () => {
        const hasLines = lines.length >= 2;
        const allLinesHaveAccounts = lines.every(line => line.chart_of_account_id);
        const hasAmounts = lines.every(line => (line.debit_amount || 0) > 0 || (line.credit_amount || 0) > 0);
        canSave(!(hasLines && allLinesHaveAccounts && hasAmounts && isBalanced()));
    };

    const formatCurrency = (value: number | undefined) => {
        if (value === undefined) return '$0.00';
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
    };

    const colDefs = useMemo<ColDef<JournalEntryLineDto>[]>(() => [
        { 
            field: "account_number", 
            headerName: "Account #",
        },
        { 
            field: "account_name", 
            headerName: "Account Name",
        },
        { 
            field: "debit_amount", 
            headerName: "Debit",
            editable: !entry?.is_posted,
            cellEditor: 'agNumberCellEditor',
            valueFormatter: (params) => formatCurrency(params.value)
        },
        { 
            field: "credit_amount", 
            headerName: "Credit",
            editable: !entry?.is_posted,
            cellEditor: 'agNumberCellEditor',
            valueFormatter: (params) => formatCurrency(params.value)
        },
        { 
            field: "description", 
            headerName: "Description",
            editable: !entry?.is_posted
        },
        {
            field: "id",
            headerName: "",
            width: 80,
            hide: entry?.is_posted,
            cellRenderer: (props: any) => (
                <Button size="sm" colorPalette="red" variant="ghost" onClick={() => removeLine(props.value)}>
                    <MdDelete />
                </Button>
            )
        }
    ], [lines, entry?.is_posted]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: false,
    };

    const getStatusBadge = () => {
        if (entry?.is_reversed) {
            return <Badge colorPalette="red">Reversed</Badge>;
        }
        return entry?.is_posted 
            ? <Badge colorPalette="green">Posted</Badge> 
            : <Badge colorPalette="yellow">Draft</Badge>;
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

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    return (
        <form onSubmit={handleSubmit(handleSaveClick)}>
            <PageActionsComponent 
                showDelete={hasDeletePermission && !entry.is_posted}
                showSave={hasEditPermission && !entry.is_posted} 
                canSave={saveable}
                showSaveSuccess={successSaved}
                showSaveFailed={failedSaved}
                onDelete={handleDeleteClick} 
                onSave={handleSaveClick} 
            />
            <Grid
                templateColumns="repeat(5, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={4}>
                    <h1>Edit Journal Entry {getStatusBadge()}</h1>
                </GridItem>
                <GridItem colSpan={2} style={{ textAlign: 'right' }}>
                    {!entry.is_posted && isBalanced() && lines.length >= 2 && (
                        <Button colorPalette="green" onClick={handlePostClick} mr={2}>Post Entry</Button>
                    )}
                    {entry.is_posted && !entry.is_reversed && (
                        <Button colorPalette="orange" onClick={handleReverseClick}>Reverse Entry</Button>
                    )}
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
                    <Field.Root required>
                        <Field.Label>Entry Date</Field.Label>
                        <DatePicker 
                            selected={entryDate}
                            onChange={(date) => { setEntryDate(date); CheckFormValidity(); }}
                            className="chakra-input"
                            disabled={entry.is_posted}
                        />
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Field.Label>Fiscal Period</Field.Label>
                        <Input 
                            value={watch('fiscal_period') || ''}
                            readOnly
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={3}></GridItem>

                <GridItem colSpan={3}>
                    <Field.Root>
                        <Field.Label>Description</Field.Label>
                        <Textarea 
                            {...register('description')}
                            onChange={(e) => { setValue('description', e.target.value); CheckFormValidity(); }}
                            disabled={entry.is_posted}
                        />
                    </Field.Root>
                </GridItem>
                <GridItem colSpan={3}></GridItem>

                <GridItem colSpan={6}>
                    <h2>Lines</h2>
                    {!entry.is_posted && (
                        <Button colorPalette="blue" size="sm" onClick={addLine} mb={4}>Add Line</Button>
                    )}
                    <div style={{ width: "100%", height: "300px" }}>
                        <AgGridReact
                            rowData={lines}
                            columnDefs={colDefs}
                            defaultColDef={defaultColDef}
                            onCellValueChanged={(event) => {
                                updateLine(event.data.id, event.colDef.field as keyof JournalEntryLineDto, event.newValue);
                            }}
                        />
                    </div>
                </GridItem>

                <GridItem colSpan={6}>
                    <Grid templateColumns="repeat(3, 1fr)" gap={4}>
                        <div>
                            <strong>Total Debits:</strong> {formatCurrency(getTotalDebits())}
                        </div>
                        <div>
                            <strong>Total Credits:</strong> {formatCurrency(getTotalCredits())}
                        </div>
                        <div>
                            <strong>Balance:</strong> {isBalanced() ? '✓ Balanced' : '✗ Out of Balance'}
                        </div>
                    </Grid>
                </GridItem>
            </Grid>
        </form>
    );
}

export default EditJournalEntryPage;
