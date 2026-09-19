"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../../styles/page.component.css'

import { Field, Grid, GridItem, Input, NumberInput, Tabs } from "@chakra-ui/react";

import { TransactionCreateCommand, TransactionEditCommand, TransactionFindCommand, TransactionListDto } from '@/models/transaction-models';
import { useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { format } from 'date-fns';
import { useAuth } from '@/lib/auth/auth-context';

import { AgGridReact } from 'ag-grid-react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { transactionService } from '@/services/transaction-service';
import { DateTimeRenderWithTime } from '@/components/ag-grid/date-time-renderer-with-time';
import TransactionTypeCombobox, { TransactionTypeComboboxRef } from '@/components/transaction-type-combobox';
import ProductCombobox, { ProductComboboxRef } from '@/components/product-combobox';
import PageActionsComponent from '@/components/page-actions';
import { useRouter } from 'next/navigation';
import { permissionsService, ERPModules } from '@/services/permissions-service';
import { useEffect } from 'react';


ModuleRegistry.registerModules([AllCommunityModule]);

class AdminAdjustmentForm
{
    salesOrderNumber?: string;
    purchaseOrderNumber?: string;

    transaction_type?: string;
    transaction_type_id?: number;
    product_id?: string;
    units_sold: number = 0;
    units_shipped: number = 0;
    units_purchased: number = 0;
    units_received: number = 0;
    purchased_unit_cost: number = 0;
    sold_unit_price: number = 0;
    tax_rate: number = 0;
}

function AdminAdjustmentsPage() {
    const auth = useAuth();
    const router = useRouter();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);

    useEffect(() => {
        if(auth.authenticated == false) return;

        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.Admin,
          '',
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasEditPermission(true);
    }, [auth.authenticated, router]);

    const [rowSalesData, setRowSalesData] = useState<TransactionListDto[]>([]);
    const [rowPurchaseData, setRowPurchaseData] = useState<TransactionListDto[]>([]);
    
    const [page, setPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(50);

    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);

    const transactionTypeComboboxRef = useRef<TransactionTypeComboboxRef>(null);
    const productComboboxRef = useRef<ProductComboboxRef>(null);
    
    const {
        register,
        formState: { errors, isValid },
        watch,
        control,
        setValue
    } = useForm<AdminAdjustmentForm>();

    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: true,
    };


    const [colDefs, setColDefs] = useState<ColDef<TransactionListDto>[]>([
        { field: "product_name", headerName: "Product", editable: false, filter: false },
        { field: "transaction_type_name", headerName: "Transaction Type", editable: false, filter: false },
        { field: "transaction_date", headerName: "Transaction Date", editable: false, filter: false, cellRenderer: DateTimeRenderWithTime },
        { field: "object_sub_reference_id", headerName: "Sub-Reference", editable: false, filter: false},
        { field: "units_sold", headerName: "Units Sold", editable: true, filter: false },
        { field: "units_shipped", headerName: "Units Shipped", editable: true, filter: false },
        { field: "units_purchased", headerName: "Units Purchased", editable: true, filter: false },
        { field: "units_received", headerName: "Units Received", editable: true, filter: false },
        { field: "purchased_unit_cost", headerName: "Purchased Unit Cost", editable: true, filter: false },
        { field: "sold_unit_price", headerName: "Sold Unit Price", editable: true, filter: false },
    ]);

    const performSalesSearch = async () => {
        let command = new TransactionFindCommand();
        command.sales_order_number = Number(watch("salesOrderNumber"));

        await transactionService.find(command, auth.token ?? "", page, pageSize).then((response) => {
            //console.log(response);
            if(response.data)
            {
                setRowSalesData(response.data);
            }
        });
    }

    const performPurchaseOrderSearch = async () => {
        let command = new TransactionFindCommand();
        command.purchase_order_number = Number(watch("purchaseOrderNumber"));

        await transactionService.find(command, auth.token ?? "", page, pageSize).then((response) => {
            //console.log(response);
            if(response.data)
            {
                setRowSalesData(response.data);
            }
        });
    }


    const salesKeyDown = (key: any) => {
        if(key.key == "Enter")
        {
            performSalesSearch();
        }
    }

    const purchaseOrderKeyDown = (key: any) => {
        if(key.key == "Enter")
        {
            performPurchaseOrderSearch();
        }
    }

    const purchaseOrderCellEdited = async (event: any) =>
    {
        let command = new TransactionEditCommand();
        command.id = event.data.id;
        command.units_sold = event.data.units_sold;
        command.units_shipped = event.data.units_shipped;
        command.units_purchased = event.data.units_purchased;
        command.units_received = event.data.units_received;
        command.purchased_unit_cost = event.data.purchased_unit_cost;
        command.sold_unit_price = event.data.sold_unit_price;

        await transactionService.update(command, auth.token ?? "").then((response) => {
            //console.log("Edit response:", response);
        });
    }

    const salesCellEdited = async (event: any) =>
    {
        //console.log("Cell edited:", event);
        let command = new TransactionEditCommand();
        command.id = event.data.id;
        command.units_sold = event.data.units_sold;
        command.units_shipped = event.data.units_shipped;
        command.units_purchased = event.data.units_purchased;
        command.units_received = event.data.units_received;
        command.purchased_unit_cost = event.data.purchased_unit_cost;
        command.sold_unit_price = event.data.sold_unit_price;

        await transactionService.update(command, auth.token ?? "").then((response) => {
            //console.log("Edit response:", response);
        });
    }

    const handleProductSelect = (selectedValue: any) =>
    {
        //console.log("Product selected:", selectedValue);
        setValue('product_id', selectedValue?.id);
        CheckFormValidity();
    }

    const handleTransactonTypeSelect = (selectedValue: any) =>
    {
        //console.log("Transaction type selected:", selectedValue);
        setValue('transaction_type', selectedValue?.key);
        setValue('transaction_type_id', selectedValue?.int_value);
        CheckFormValidity();
    }

    const CheckFormValidity = () => {
        const hasRequiredFields = Boolean(watch('transaction_type') 
                                            && watch('product_id') 
                                            && (watch('units_sold')
                                            || watch('units_shipped')
                                            || watch('units_purchased')
                                            || watch('units_received')));

        const isTypeValid = transactionTypeComboboxRef.current ? transactionTypeComboboxRef.current.isValid() : false;
        const isProductValid = productComboboxRef.current ? productComboboxRef.current.isValid() : false
    
        const allValid = isTypeValid && isProductValid && hasRequiredFields;

        //console.log("Form validity checked:", allValid);
        //console.log(" - hasRequiredFields:", hasRequiredFields);
        //console.log(" - isTypeValid:", isTypeValid);
        //console.log(" - isProductValid:", isProductValid);

        canSave(allValid);

    }

    const handleSaveClick = async () => 
    {
        let command = new TransactionCreateCommand();
        command.transaction_type = Number(watch('transaction_type_id'));
        command.product_id = Number(watch('product_id'));
        command.units_sold = Number(watch('units_sold'));
        command.units_shipped = Number(watch('units_shipped'));
        command.units_purchased = Number(watch('units_purchased'));
        command.units_received = Number(watch('units_received'));
        command.purchased_unit_cost = Number(watch('purchased_unit_cost'));
        command.sold_unit_price = Number(watch('sold_unit_price'));
        command.object_reference_id = 0;
        command.object_sub_reference_id = 0;
        command.transaction_date = format(new Date(), 'yyyy-MM-dd') + 'T' + format(new Date(), 'HH:mm:ss');

        //console.log("Creating transaction with command:", command);
        //return;
        await transactionService.create(command, auth.token ?? "").then((response) => {
            //console.log("Create response:", response);
        });
    }

    return (
        <div>
            <Tabs.Root lazyMount unmountOnExit defaultValue="sales-orders">
                <Tabs.List>
                    <Tabs.Trigger value="sales-orders">Sales Orders</Tabs.Trigger>
                    <Tabs.Trigger value="purchase-orders">Purchase Orders</Tabs.Trigger>
                    <Tabs.Trigger value="add-adjustment">Add Adjustment</Tabs.Trigger>
                </Tabs.List>
                <Tabs.Content value="sales-orders">
                    <Grid
                        templateColumns="repeat(6, 2fr)"
                        gap={6}
                        display="grid"
                        width="100%"
                        p="auto"
                        m="auto"
                        >
                        <GridItem colSpan={1}>
                            <Field.Root >
                                <Field.Label>Sales Order</Field.Label>
                                <Input {...register('salesOrderNumber')} placeholder="Sales Order" onKeyDown={(key: any) => salesKeyDown(key)}/>
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={6}>
                            <div style={{ width: "100%", height: "500px" }}>
                                <AgGridReact
                                    rowData={rowSalesData}
                                    columnDefs={colDefs}
                                    defaultColDef={defaultColDef}
                                    onCellEditingStopped={salesCellEdited}
                                />
                            </div>
                        </GridItem>
                    </Grid>
                </Tabs.Content>
                <Tabs.Content value="purchase-orders">
                    <Grid
                        templateColumns="repeat(6, 2fr)"
                        gap={6}
                        display="grid"
                        width="100%"
                        p="auto"
                        m="auto"
                        >
                        <GridItem colSpan={1}>
                            <Field.Root >
                                <Field.Label>Purchase Order</Field.Label>
                                <Input {...register('purchaseOrderNumber')} placeholder="Purchase Order" onKeyDown={(key: any) => purchaseOrderKeyDown(key)}/>
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={6}>
                            <div style={{ width: "100%", height: "500px" }}>
                                <AgGridReact
                                    rowData={rowPurchaseData}
                                    columnDefs={colDefs}
                                    defaultColDef={defaultColDef}
                                    onCellEditingStopped={purchaseOrderCellEdited}
                                />
                            </div>
                        </GridItem>
                    </Grid>
                </Tabs.Content>
                <Tabs.Content value="add-adjustment">
                    <Grid
                        templateColumns="repeat(6, 2fr)"
                        gap={6}
                        display="grid"
                        width="100%"
                        p="auto"
                        m="auto"
                        >
                        <GridItem colSpan={1}>
                            <Field.Root >
                                <Field.Label>Product</Field.Label>
                                <ProductCombobox 
                                    ref={productComboboxRef}
                                    dbKey={watch('product_id') || 0}
                                    control={control}
                                    name="product_id"
                                    error={errors.product_id}
                                    onChange={handleProductSelect}
                                    onValidationChange={ CheckFormValidity }
                                />
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={1}>
                            <Field.Root >
                                <Field.Label>Transaction Type</Field.Label>
                                <TransactionTypeCombobox 
                                    ref={transactionTypeComboboxRef}
                                    dbKey={watch('transaction_type') ? [watch('transaction_type') as string] : []}
                                    control={control}
                                    name="transaction_type"
                                    error={errors.transaction_type}
                                    onChange={handleTransactonTypeSelect}
                                    onValidationChange={ CheckFormValidity }
                                />
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={4}></GridItem>
                        <GridItem colSpan={1}>
                            <Field.Root>
                                <Field.Label>Units Sold</Field.Label>
                                <NumberInput.Root defaultValue="0" min={0} onValueChange={() => CheckFormValidity() }>
                                    <NumberInput.Control />
                                    <NumberInput.Input {...register('units_sold')}/>
                                </NumberInput.Root>
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={1}>
                            <Field.Root>
                                <Field.Label>Units Shipped</Field.Label>
                                <NumberInput.Root defaultValue="0" min={0} onValueChange={() => CheckFormValidity() }>
                                    <NumberInput.Control />
                                    <NumberInput.Input {...register('units_shipped')}/>
                                </NumberInput.Root>
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={1}>
                            <Field.Root>
                                <Field.Label>Units Purchased</Field.Label>
                                <NumberInput.Root defaultValue="0" min={0} onValueChange={() => CheckFormValidity() }>
                                    <NumberInput.Control />
                                    <NumberInput.Input {...register('units_purchased')}/>
                                </NumberInput.Root>
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={1}>
                            <Field.Root>
                                <Field.Label>Units Received</Field.Label>
                                <NumberInput.Root defaultValue="0" min={0} onValueChange={() => CheckFormValidity() }>
                                    <NumberInput.Control />
                                    <NumberInput.Input {...register('units_received')}/>
                                </NumberInput.Root>
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={2}></GridItem>
                        <GridItem colSpan={1}>
                            <Field.Root>
                                <Field.Label>Units Purchased Unit Cost</Field.Label>
                                <NumberInput.Root defaultValue="0" min={0} onValueChange={() => CheckFormValidity() }>
                                    <NumberInput.Control />
                                    <NumberInput.Input {...register('purchased_unit_cost')}/>
                                </NumberInput.Root>
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={1}>
                            <Field.Root>
                                <Field.Label>Units Sold Unit Cost</Field.Label>
                                <NumberInput.Root defaultValue="0" min={0} onValueChange={() => CheckFormValidity() }>
                                    <NumberInput.Control />
                                    <NumberInput.Input {...register('sold_unit_price')}/>
                                </NumberInput.Root>
                            </Field.Root>
                        </GridItem>
                        <GridItem colSpan={6}>
                            <PageActionsComponent 
                                canSave={!saveable || !hasEditPermission}
                                canDelete={false}
                                onSave={handleSaveClick} 
                                successSaved={successSaved}
                                failedSaved={failedSaved}
                            />
                        </GridItem>
                    </Grid>
                </Tabs.Content>
            </Tabs.Root>
            
        </div>
    );
}

export default AdminAdjustmentsPage;