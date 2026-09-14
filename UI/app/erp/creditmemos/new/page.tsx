"use client"

import '../../../styles/date-picker.css';
import '../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Button,
  GridItem,
  Field,
  Textarea,
} from '@chakra-ui/react'

import CustomerCombobox, { CustomerComboboxRef } from '@/components/customer-combobox'
import DatePicker from "react-datepicker";

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { CreditMemoHeaderCreateCommand, CreditMemoLineCreateCommand } from '@/models/credit-memo-models';
import { AgGridReact } from 'ag-grid-react';
import { useCallback, useEffect, useRef, useState } from "react";
import PageActionsComponent from '@/components/page-actions';
import  GLAccountSelector from '@/components/ag-grid/gl-account-selector';

import { CurrencyFormatter} from '@/components/ag-grid/currency-formatter';
import { useRouter } from 'next/navigation';
import { format } from 'date-fns';
import { creditMemoService } from '@/services/credit-memo-service';
import ARInvoiceSelectorComponent from '@/components/ar-invoice-selector';
import { keyValueService } from '@/services/keyvalue-service';
import { useKeycloak } from '@react-keycloak/web';

import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewCreditMemoPage() {
  const router = useRouter();

  const { keycloak } = useKeycloak();
  const [hasAccess, setHasAccess] = useState(true);

  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);

  const [gLAccounts, setGLAccounts] = useState<any[]>([]);

  const customerComboboxRef = useRef<CustomerComboboxRef>(null);
  const [rowData, setRowData] = useState<CreditMemoLineCreateCommand[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<CreditMemoLineCreateCommand>[]>([]);
  const hasInitialized = useRef(false);

  const {
    register,
    formState: { errors },
    setValue,
    watch,
    control,
  } = useForm<CreditMemoHeaderCreateCommand>();

  useEffect(() => {
    if(keycloak.authenticated == false) return;

    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.CreditMemoModule,
      ERPModulePermission.Write,
      realmRoles
    );
    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    if (hasInitialized.current) return;
    hasInitialized.current = true;


    keyValueService.GetDtoByModule("eea9df53-1b36-41ea-94fa-31420315ff60", keycloak.token || "").then((gl_response) =>
    {
      //console.log("KEY VALUES ", response)
      if (gl_response.success && gl_response.data !== undefined) 
      {
          const accounts = gl_response.data.map(kv => ({
              label: kv.value,
              value: kv.key
          }));

          //console.log("Accounts ", accounts)

          setGLAccounts(accounts);
      }
    });
  }, [keycloak.authenticated]);

  useEffect(() => {
      setColDefs([
        { field: "description", headerName: "Description", editable: true },
        { field: "qty_credited", headerName: "Qty Credited", editable: true, cellEditor: 'agNumberCellEditor' },
        { field: "line_total", headerName: "Amount", editable: true, cellEditor: 'agNumberCellEditor', cellRenderer: CurrencyFormatter },
        { field: "gl_account_id", headerName: "GL Account", 
                  editable: true, 
                  cellEditor: GLAccountSelector, 
                  cellEditorParams:{ accounts: gLAccounts },
                  cellRenderer: (param: any) => { 
                    //console.log(param);
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
        },
        { field: "id", headerName: "Actions", cellRenderer: (props: any) => (
          <div>
            <Button type="button" colorPalette="red" onClick={() => handleDeleteLineClick(props.data.id)}>Delete</Button>
          </div>
        ) }
      ]);
  
  }, [gLAccounts]);

  const getRowId = useCallback((params: any) => String(params.data.id), []);

  const CheckFormValidity = () => {
    const hasBaseRequiredFields = Boolean(watch('customer_id') && watch('credit_memo_date') 
                                                          && watch('credit_memo_due_date') 
                                                          && watch("credit_reason") 
                                                          && watch('ar_invoice_header_id'));
    //console.log('customer_id: ' + watch('customer_id'));
    //console.log('credit_memo_date: ' + watch('credit_memo_date'));
    //console.log('credit_memo_due_date: ' + watch('credit_memo_due_date'));
    //console.log('credit_reason: ' + watch('credit_reason'));
    //console.log('ar_invoice_header_id: ' + watch('ar_invoice_header_id'));

    // Check lines
    if(rowData.length == 0)
    {
      canSave(false);
      return;
    }

    for(let line of rowData)
    {
      console.log('line check ', line);
      if(!line.description || line.description.trim() === '' 
            || !line.gl_account_id || line.gl_account_id.trim() === ''
            || !line.line_total || line.line_total <= 0)
      {
        canSave(false);
        return;
      }
    }


    canSave(hasBaseRequiredFields);
  };

  const handleSaveClick = async () => {
    setSuccessSaved(false);

    let command = new CreditMemoHeaderCreateCommand();
    command.customer_id = watch('customer_id');
    command.credit_memo_date = watch('credit_memo_date');
    command.credit_memo_due_date = watch('credit_memo_due_date');
    command.credit_memo_total = watch('credit_memo_total');
    command.memo = watch('memo');
    command.ar_invoice_header_id = watch('ar_invoice_header_id');
    command.credit_reason = watch('credit_reason');
    command.is_approved = watch('is_approved');
    command.is_applied = watch('is_applied');

    command.credit_memo_lines = new Array<CreditMemoLineCreateCommand>();

    for(let line of rowData)
    {

      command.credit_memo_lines.push(line);
    }

    //console.log(command);

    try {
      const response = await creditMemoService.create(command, keycloak.token || "");

      if (response && response.success) {
        setSuccessSaved(true);
        setFailedSaved(false);
        router.push('/erp/creditmemos');
      } else {
        setSuccessSaved(false);
        setFailedSaved(true);
      }
    } catch (e) {
      console.error(e);
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const handleCustomerSelect = (value: any) => {
    setValue('customer_id', value?.id);
    CheckFormValidity();
  };

  const requiredDaySelected = (value: any, field: string) => {
    if (value != null) {
      setValue(field as any, format(value, 'yyyy-MM-dd'));
    }
    CheckFormValidity();
  }

  const getDate = (field: string) => {
    const val = watch(field as any);
    return val ? new Date(val + "T00:01:00") : undefined;
  }

  const handleAddLineSelect = () => {
    let rand = Math.floor(Math.random() * 1000000);

    const newLine: CreditMemoLineCreateCommand = {
      id: rand,
      line_number: rowData.length + 1,
      description: '',
      qty_credited: 1,
      line_total: 0,
      gl_account_id: '',
    };

    setRowData(prev => [...prev, newLine]);

    CheckFormValidity();
  }

  

  const handleDeleteLineClick = (lineId: any) => {
    setRowData(prev => prev.filter(line => line.id !== lineId));
    CheckFormValidity();
  };

  const defaultColDef: ColDef = { flex: 1, filter: true, sortable: true };

  const onCellValueChanged = (params: any) => {

    const updatedRow = params.data;
    setRowData(prev => prev.map(r => r.id === updatedRow.id ? updatedRow : r));

    if (!params.colDef || params.colDef.field === 'description' || params.colDef.field === 'gl_account_id' || params.colDef.field === 'line_total') {
      CheckFormValidity();
    }
  };

  const arInvoiceSelected = (val: any) => {
    //console.log("arInvoiceSelected:",val);
    setValue('ar_invoice_header_id', val?.ar_invoice_header_id);
    CheckFormValidity();
  }

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  return (
    <div>
      <form onChange={CheckFormValidity}>
        <h1>New Credit Memo</h1>
        <Grid templateColumns="repeat(5, 2fr)" 
          gap={6} 
          display="grid" 
          width="100%" 
          p="auto" 
          m="auto"
        >
          <GridItem colSpan={1}>
            <Field.Root invalid={!!errors.customer_id} required={true}>
              <Field.Label><Field.RequiredIndicator /> Customer</Field.Label>
              <CustomerCombobox ref={customerComboboxRef} 
                                dbKey={watch('customer_id') || 0} 
                                onChange={handleCustomerSelect} 
                                control={control} 
                                name="customer_id" 
                                error={errors.customer_id} 
                                disabled={false} />
            </Field.Root>
          </GridItem>
          <GridItem colSpan={1}>
            <ARInvoiceSelectorComponent customer_id={watch('customer_id')} onChange={(val: any) => { arInvoiceSelected(val); }} />
          </GridItem>

          <GridItem colSpan={3}></GridItem>

          <GridItem colSpan={1}>
            <Field.Root invalid={!!errors.credit_memo_date} required={true}>
              <Field.Label><Field.RequiredIndicator />Credit Memo Date</Field.Label>
              <DatePicker selected={getDate('credit_memo_date')} onChange={(v) => requiredDaySelected(v, 'credit_memo_date')} dateFormat="MM/dd/yyyy" />
            </Field.Root>
          </GridItem>

          <GridItem colSpan={1}>
            <Field.Root invalid={!!errors.customer_id} required={true}>
              <Field.Label><Field.RequiredIndicator />Due Date</Field.Label>
              <DatePicker selected={getDate('credit_memo_due_date')} onChange={(v) => requiredDaySelected(v, 'credit_memo_due_date')} dateFormat="MM/dd/yyyy" />
            </Field.Root>
          </GridItem>

          <GridItem colSpan={3}></GridItem>

          <GridItem colSpan={2}>
            <Field.Root>
              <Field.Label>Memo</Field.Label>
              <Textarea {...register('memo')} />
            </Field.Root>
          </GridItem>

          <GridItem colSpan={2}>
            <Field.Root invalid={!!errors.credit_reason} required={true}>
              <Field.Label><Field.RequiredIndicator />Credit Reason</Field.Label>
              <Textarea {...register('credit_reason')} onKeyDown={() => { CheckFormValidity(); }}/>
            </Field.Root>
          </GridItem>

          <GridItem colSpan={5}>
            <div style={{ width: "100%", height: "400px" }}>
              <Button onClick={() => { handleAddLineSelect(); } } colorPalette="blue">Add New Line</Button>
              <AgGridReact rowData={rowData} columnDefs={colDefs} defaultColDef={defaultColDef} getRowId={getRowId} onCellValueChanged={onCellValueChanged} />
            </div>
          </GridItem>

          <GridItem colSpan={5}>
            <PageActionsComponent onSave={handleSaveClick} onDelete={() => {}} canSave={!saveable} canDelete={false} successSaved={successSaved} failedSaved={failedSaved} />
          </GridItem>
        </Grid>
      </form>

      
    </div>
  )
}

export default NewCreditMemoPage;
