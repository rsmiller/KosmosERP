"use client"

import '../../../styles/date-picker.css';
import '../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Textarea,
  Button
} from '@chakra-ui/react';

import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { 
  JournalEntryHeaderCreateCommand, 
  JournalEntryLineCreateCommand,
  JournalEntryHeaderDto 
} from '@/models/journal-entry-models';
import { journalEntryService } from '@/services/journal-entry-service';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import DatePicker from 'react-datepicker';
import "react-datepicker/dist/react-datepicker.css";
import { MdDelete } from 'react-icons/md';

ModuleRegistry.registerModules([AllCommunityModule]);

interface LineItem {
  id: number;
  chart_of_account_id?: number;
  account_display?: string;
  debit_amount: number;
  credit_amount: number;
  description?: string;
}

function NewJournalEntryPage() {
    const { keycloak } = useKeycloak();
    const router = useRouter();

    const [hasAccess, setHasAccess] = useState(true);
    const [hasWritePermission, setHasWritePermission] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    const [entryDate, setEntryDate] = useState<Date | null>(new Date());
    const [lines, setLines] = useState<LineItem[]>([]);
    const [nextLineId, setNextLineId] = useState(1);
    
    const hasInitialized = useRef(false);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
    } = useForm<JournalEntryHeaderCreateCommand>();

    useEffect(() => {
        if (keycloak.authenticated == false) return;

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.JournalEntryModule,
          ERPModulePermission.Write,
          realmRoles
        );

        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasWritePermission(true);
    }, [keycloak.authenticated]);

    const handleDeleteClick = async () => {
        router.push("/erp/journalentries/");
    };

    const handleSaveClick = async () => {
        setSuccessSaved(false);

        let command = new JournalEntryHeaderCreateCommand();
        command.entry_date = entryDate?.toISOString();
        command.description = watch('description');
        command.reference_type = 1; // Manual
        command.fiscal_period = entryDate ? `${entryDate.getFullYear()}-${String(entryDate.getMonth() + 1).padStart(2, '0')}` : undefined;
        command.lines = lines.map(line => {
            let lineCmd = new JournalEntryLineCreateCommand();
            lineCmd.chart_of_account_id = line.chart_of_account_id;
            lineCmd.debit_amount = line.debit_amount;
            lineCmd.credit_amount = line.credit_amount;
            lineCmd.description = line.description;
            return lineCmd;
        });

        try {
            await journalEntryService.create(command, keycloak.token || "").then((response) => {
                if(response.success) {
                    setSuccessSaved(true);
                    setFailedSaved(false);
                    router.push("/erp/journalentries/edit/" + response.data?.guid);
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

    const addLine = () => {
        setLines([...lines, {
            id: nextLineId,
            chart_of_account_id: undefined,
            account_display: '',
            debit_amount: 0,
            credit_amount: 0,
            description: ''
        }]);
        setNextLineId(nextLineId + 1);
        CheckFormValidity();
    };

    const removeLine = (id: number) => {
        setLines(lines.filter(line => line.id !== id));
        CheckFormValidity();
    };

    const updateLine = (id: number, field: keyof LineItem, value: any) => {
        setLines(lines.map(line => {
            if (line.id === id) {
                return { ...line, [field]: value };
            }
            return line;
        }));
        CheckFormValidity();
    };

    const getTotalDebits = () => lines.reduce((sum, line) => sum + (line.debit_amount || 0), 0);
    const getTotalCredits = () => lines.reduce((sum, line) => sum + (line.credit_amount || 0), 0);
    const isBalanced = () => Math.abs(getTotalDebits() - getTotalCredits()) < 0.01;

    const CheckFormValidity = () => {
        const hasLines = lines.length >= 2;
        const allLinesHaveAccounts = lines.every(line => line.chart_of_account_id);
        const hasAmounts = lines.every(line => line.debit_amount > 0 || line.credit_amount > 0);
        canSave(!(hasLines && allLinesHaveAccounts && hasAmounts && isBalanced()));
    };

    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
    };

    const colDefs = useMemo<ColDef<LineItem>[]>(() => [
        { 
            field: "chart_of_account_id", 
            headerName: "Account",
            editable: true,
            cellEditor: 'agTextCellEditor'
        },
        { 
            field: "debit_amount", 
            headerName: "Debit",
            editable: true,
            cellEditor: 'agNumberCellEditor',
            valueFormatter: (params) => formatCurrency(params.value || 0)
        },
        { 
            field: "credit_amount", 
            headerName: "Credit",
            editable: true,
            cellEditor: 'agNumberCellEditor',
            valueFormatter: (params) => formatCurrency(params.value || 0)
        },
        { 
            field: "description", 
            headerName: "Description",
            editable: true
        },
        {
            field: "id",
            headerName: "",
            width: 80,
            cellRenderer: (props: any) => (
                <Button size="sm" colorPalette="red" variant="ghost" onClick={() => removeLine(props.value)}>
                    <MdDelete />
                </Button>
            )
        }
    ], [lines]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: false,
    };

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    return (
        <form onSubmit={handleSubmit(handleSaveClick)}>
            <PageActionsComponent 
                showDelete={false}
                showSave={hasWritePermission} 
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
                <GridItem colSpan={6}>
                    <h1>New Journal Entry</h1>
                </GridItem>
                
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root required>
                        <Field.Label>Entry Date</Field.Label>
                        <DatePicker 
                            selected={entryDate}
                            onChange={(date) => { setEntryDate(date); CheckFormValidity(); }}
                            className="chakra-input"
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>

                <GridItem colSpan={3}>
                    <Field.Root>
                        <Field.Label>Description</Field.Label>
                        <Textarea 
                            {...register('description')}
                            onChange={(e) => { setValue('description', e.target.value); CheckFormValidity(); }}
                        />
                    </Field.Root>
                </GridItem>
                <GridItem colSpan={3}></GridItem>

                <GridItem colSpan={6}>
                    <h2>Lines</h2>
                    <Button colorPalette="blue" size="sm" onClick={addLine} mb={4}>Add Line</Button>
                    <div style={{ width: "100%", height: "300px" }}>
                        <AgGridReact
                            rowData={lines}
                            columnDefs={colDefs}
                            defaultColDef={defaultColDef}
                            onCellValueChanged={(event) => {
                                updateLine(event.data.id, event.colDef.field as keyof LineItem, event.newValue);
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

export default NewJournalEntryPage;
