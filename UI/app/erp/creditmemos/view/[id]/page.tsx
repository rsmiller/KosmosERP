"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  GridItem,
  Field,
  Textarea,
  Input,
} from '@chakra-ui/react'


import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { CreditMemoHeaderDto, CreditMemoLineDto } from '@/models/credit-memo-models';
import { AgGridReact } from 'ag-grid-react';
import { useCallback, useEffect, useState, useRef } from "react";

import { useParams, useRouter } from 'next/navigation';
import ARInvoiceSelectorComponent from '@/components/ar-invoice-selector';
import { keyValueService } from '@/services/keyvalue-service';
import DatePicker from 'react-datepicker';
import creditMemoService from '@/services/credit-memo-service';
import { useAuth } from '@/lib/auth/auth-context';

import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewCreditMemoPage() {
    const params = useParams();
    const router = useRouter();
    
    const auth = useAuth();
    const [hasAccess, setHasAccess] = useState(true);

    const [gLAccounts, setGLAccounts] = useState<any[]>([]);

    const hasInitialized = useRef(false);

    const [rowData, setRowData] = useState<CreditMemoLineDto[]>([]);

    const [colDefs, setColDefs] = useState<ColDef<CreditMemoLineDto>[]>([]);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [creditMemo, setCreditMemo] = useState<CreditMemoHeaderDto | null>(null);

    const {
        register,
        formState: { errors },
        setValue,
        watch,
        control,
    } = useForm<CreditMemoHeaderDto>();

    useEffect(() => {
        if(auth.authenticated == false) return;

        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.CreditMemoModule,
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

        const creditMemo = String(params.id);
            
        if (creditMemo == "") {
            setError('Invalid customer ID');
            return;
        }

        setLoading(true);

        keyValueService.GetDtoByModule("eea9df53-1b36-41ea-94fa-31420315ff60", auth.token || "").then((gl_response) =>
        {
            if (gl_response.success && gl_response.data !== undefined) 
            {
                const accounts = gl_response.data.map(kv => ({
                    label: kv.value,
                    value: kv.key
                }));

                setGLAccounts(accounts);
            }

            creditMemoService.getByGuid(creditMemo, auth.token || "").then((response) => {
                //console.log('Credit memo response:', response);

                if (response.success && response.data) 
                {
                    setCreditMemo(response.data);

                    setValue('id', response.data.id);
                    setValue('customer_id', response.data.customer_id);
                    setValue('customer_name', response.data.customer_name);
                    setValue('ar_invoice_header_id', response.data.ar_invoice_header_id);
                    setValue('ar_invoice_number', response.data.ar_invoice_number);
                    setValue('credit_memo_date', response.data.credit_memo_date);
                    setValue('credit_memo_due_date', response.data.credit_memo_due_date);
                    setValue('memo', response.data.memo);
                    setValue('credit_reason', response.data.credit_reason);
                    
                    setRowData(response.data.credit_memo_lines || []);

                    setLoading(false);

                } else {
                    setLoading(false);
                    setError('Failed to load credit memo');
                }
            });
        });
    }, [params.id, auth.authenticated]);

    useEffect(() => {
        setColDefs([
            { field: "description", headerName: "Description", editable: false },
            { field: "qty_credited", headerName: "Qty Credited", editable: false},
            { field: "line_total", headerName: "Amount", editable: false},
            { field: "gl_account_id", headerName: "GL Account", editable: false,
                cellRenderer: (param: any) => { 
                    let derp = gLAccounts.filter(m => m.value == param.value);

                    if(derp.length > 0)
                    {
                        return derp[0].label;
                    }
                    else
                    {
                        return '';
                    }
            }},
            { headerName: "Total", cellRenderer: (props: any) => {
                const total = props.data.qty_credited * (props.data.line_total || 0);
                return "$" + (total).toFixed(2);
                }
            }]);
    }, [gLAccounts]);

    const getDate = (field: string) => {
        const val = watch(field as any);
        return val ? new Date(val) : undefined;
    }

    const getRowId = useCallback((params: any) => String(params.data.id), []);
    const defaultColDef: ColDef = { flex: 1, filter: true, sortable: true };

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    if (loading) {
        return <div>Loading credit memo...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    return (
        <div>
        <form>
            <h1>Credit Memo - {creditMemo?.credit_memo_number}</h1>
            <Grid templateColumns="repeat(5, 2fr)" 
            gap={6} 
            display="grid" 
            width="100%" 
            p="auto" 
            m="auto"
            >
            <GridItem colSpan={1}>
                <Field.Root required={true}>
                <Field.Label><Field.RequiredIndicator /> Customer</Field.Label>
                <Input {...register('customer_name')} disabled={true} />
                </Field.Root>
            </GridItem>
            <GridItem colSpan={1}>
                <ARInvoiceSelectorComponent customer_id={watch('customer_id')} ar_invoice_number={watch('ar_invoice_number')} disabled={true}/>
            </GridItem>

            <GridItem colSpan={3}></GridItem>

            <GridItem colSpan={1}>
                <Field.Root>
                <Field.Label><Field.RequiredIndicator />Credit Memo Date</Field.Label>
                <DatePicker selected={getDate('credit_memo_date')} disabled={true} dateFormat="MM/dd/yyyy" />
                </Field.Root>
            </GridItem>

            <GridItem colSpan={1}>
                <Field.Root>
                <Field.Label><Field.RequiredIndicator />Due Date</Field.Label>
                <DatePicker selected={getDate('credit_memo_due_date')} dateFormat="MM/dd/yyyy" disabled={true}/>
                </Field.Root>
            </GridItem>

            <GridItem colSpan={3}></GridItem>

            <GridItem colSpan={2}>
                <Field.Root>
                <Field.Label>Memo</Field.Label>
                <Textarea {...register('memo')} disabled={true} />
                </Field.Root>
            </GridItem>

            <GridItem colSpan={2}>
                <Field.Root>
                <Field.Label><Field.RequiredIndicator />Credit Reason</Field.Label>
                <Textarea {...register('credit_reason')} disabled={true}/>
                </Field.Root>
            </GridItem>

            <GridItem colSpan={5}>
                <div style={{ width: "100%", height: "400px" }}>
                <AgGridReact rowData={rowData} columnDefs={colDefs} defaultColDef={defaultColDef} getRowId={getRowId}/>
                </div>
            </GridItem>
            </Grid>
        </form>

        
        </div>
    )
}

export default ViewCreditMemoPage;
