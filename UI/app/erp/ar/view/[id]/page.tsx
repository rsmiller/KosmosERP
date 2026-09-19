"use client"

import PageActionsComponent from '@/components/page-actions';
import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';

import { CustomerDto } from '@/models/customer-models';
import { OrderHeaderDto } from '@/models/sales-order-models';
import { AddressDto, AddressFindCommand } from '@/models/address-models';
import { ARInvoiceHeaderDto, ARInvoiceLineDto } from '@/models/ar-models';

import { arInvoiceService } from '@/services/ar-invoice-service';
import { customerService } from '@/services/customer-service';
import { orderService } from '@/services/order-service';

import { Button, Checkbox, DataList, Grid, GridItem } from '@chakra-ui/react';
import { useParams, useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";

import { format, parse } from 'date-fns';
import { AgGridReact } from 'ag-grid-react';
import { addressService } from '@/services/address-service';
import { useAuth } from '@/lib/auth/auth-context';


import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function AccountsReceivableViewPage() {
    const params = useParams();
    const router = useRouter();

    const auth = useAuth();
    const [hasAccess, setHasAccess] = useState(true);

    const [ar_invoice_header_id, setAR_invoice_header_id] = useState<string>("");

    const [arHeaderModel, setARHeaderModel] = useState<ARInvoiceHeaderDto>();
    const [customerModel, setCustomerModel] = useState<CustomerDto>();
    const [orderModel, setOrderModel] = useState<OrderHeaderDto>();

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [rowData, setRowData] = useState<ARInvoiceLineDto[]>([]);

    const [billingAddress, setBillingAddress] = useState<AddressDto | undefined>();
    
    const hasInitialized = useRef(false);
    
    useEffect(() => {
        
        //console.log(keycloak);

        if(auth.authenticated === false)
        {
            return;
        }

        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ARModule,
          ERPModulePermission.Read,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        setLoading(true);

        const invoice_header_id = String(params.id);
        
        if (invoice_header_id === "") {
            setError('Invalid AR Header ID');
            return;
        }

        setAR_invoice_header_id(invoice_header_id);

        if(hasInitialized.current)
            return;
        
        hasInitialized.current = true;

        //console.log("Loading AR Invoice Header ID: " + invoice_header_id);

        arInvoiceService.getByGuid(invoice_header_id, auth.token || "").then((arResponse) => {
            
            //console.log("AR Invoice Header Response: ", arResponse);
            if (arResponse.success && arResponse.data) {
                setARHeaderModel(arResponse.data);
                
                setRowData([]);

                if(arResponse.data.ar_invoice_lines != undefined)
                {
                    for(let i=0; i<arResponse.data.ar_invoice_lines.length; i++) 
                    {
                        setRowData(arResponse.data.ar_invoice_lines || []);
                    }
                }
                

                let address_find_comment = new AddressFindCommand();
                address_find_comment.customer_id = arResponse.data.customer_id || 0;
                address_find_comment.address_type_id = 2; // Billing

                addressService.find(address_find_comment, auth.token || "").then( (addresses_response) =>
                {
                    if(addresses_response.success && addresses_response.data)
                    {
                        setBillingAddress(addresses_response.data[0]);
                    }
                });


                orderService.get(arResponse.data.order_header_id || 0, auth.token || "").then((orderResponse) => {

                    if (orderResponse.success && orderResponse.data) 
                    {
                        setOrderModel(orderResponse.data);

                        customerService.get(orderResponse.data.customer_id || 0, auth.token || "").then((customerResponse) => {

                            if (customerResponse.success && customerResponse.data) 
                            {
                                setCustomerModel(customerResponse.data);

                                setLoading(false);
                            }
                        });
                    }
                });
            }
        });

    }, [params.id, ar_invoice_header_id, auth.authenticated]);

    const formatDateString = (dateString: string | undefined): string => {
        if(!dateString || dateString.trim() === ""){
            return "";
        }

        return format(dateString || "", 'MM-dd-yyyy');
    }

    const printInvoice = () => 
    {
        window.open(`/docs/api/?url=${encodeURIComponent('/docs/ar/' + arHeaderModel?.guid)}`, "_blank")
    }

    const makePayment = () =>
    {
        window.open(`/erp/payments/stripe/${arHeaderModel?.guid}`, "_blank")
    }

    const [colDefs, setColDefs] = useState<ColDef<ARInvoiceLineDto>[]>([
        { field: "line_description", headerName: "Line Description", filter: false },
        { field: "invoice_qty", headerName: "Units Invoiced", filter: false },
        { field: "is_taxable", headerName: "Tax", editable: false, filter: false },
        { field: "unit_price", headerName: "Unit Price", filter: false, valueFormatter: CurrencyFormatter },
        { field: "line_tax", headerName: "Line Tax", filter: false, valueFormatter: CurrencyFormatter },
        { headerName: "Line Total", cellRenderer: (params: any) => {
                const invoice_qty = params.data.invoice_qty || 0;
                const unit_price = params.data.unit_price || 0;
                const line_tax = params.data.line_tax || 0;
                const line_total = (invoice_qty * unit_price) + line_tax;

                return "$" + line_total.toFixed(2).toLocaleString();
            }, filter: false,
            valueFormatter: CurrencyFormatter
        },
    ]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: true,
        sortable: true,
    };

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    if (loading) {
        return <div>Loading...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    return (
        <form>
            <Grid
                templateColumns="repeat(8, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={8}>
                    <h1>Invoice - {arHeaderModel?.invoice_number}</h1>
                </GridItem>
                <GridItem colSpan={2}>
                    <div><h3>Customer</h3></div>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Number</DataList.ItemLabel>
                        <DataList.ItemValue>{customerModel?.customer_number}</DataList.ItemValue>
                    </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Name</DataList.ItemLabel>
                        <DataList.ItemValue>{customerModel?.customer_name}</DataList.ItemValue>
                    </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Terms</DataList.ItemLabel>
                        <DataList.ItemValue>{customerModel?.payment_terms_name}</DataList.ItemValue>
                    </DataList.Item>
                    </DataList.Root>
                </GridItem>

                <GridItem colSpan={2}>
                <div><h3>&nbsp;</h3></div>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                    <DataList.ItemLabel>Taxable</DataList.ItemLabel>
                    <DataList.ItemValue>
                        <Checkbox.Root checked={customerModel?.is_taxable}>
                            <Checkbox.Control />
                        </Checkbox.Root>
                    </DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                    <DataList.ItemLabel>Tax Rate</DataList.ItemLabel>
                    <DataList.ItemValue>{customerModel?.tax_rate}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                </GridItem>

                <GridItem colSpan={2}>
                <div><h3>Order</h3></div>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                    <DataList.ItemLabel>Number</DataList.ItemLabel>
                    <DataList.ItemValue>{orderModel?.order_number}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                    <DataList.ItemLabel>PO Number</DataList.ItemLabel>
                    <DataList.ItemValue>{orderModel?.po_number}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                    <DataList.ItemLabel>Order Date</DataList.ItemLabel>
                    <DataList.ItemValue>{formatDateString(orderModel?.order_date)}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                </GridItem>

                <GridItem colSpan={2}>
                    <div><h3>&nbsp;</h3></div>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Pay Method</DataList.ItemLabel>
                        <DataList.ItemValue>{orderModel?.pay_method_name}</DataList.ItemValue>
                    </DataList.Item>
                    </DataList.Root>
                        <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Shipping Method</DataList.ItemLabel>
                        <DataList.ItemValue>{orderModel?.shipping_method_name}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                </GridItem>
                <GridItem colSpan={8}></GridItem>
                <GridItem colSpan={2}>
                    <div><h3>Invoice</h3></div>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Invoice Date</DataList.ItemLabel>
                        <DataList.ItemValue>{formatDateString(arHeaderModel?.invoice_date)}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Due Date</DataList.ItemLabel>
                        <DataList.ItemValue>{formatDateString(arHeaderModel?.invoice_due_date)}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Billing Address</DataList.ItemLabel>
                        <DataList.ItemValue>
                            <div>
                            <div className="address_line">{billingAddress?.street_address1}</div>
                            <div className="address_line">{billingAddress?.street_address2}</div>
                            <div className="address_line">{billingAddress?.city}, {billingAddress?.state} {billingAddress?.postal_code} {billingAddress?.country}</div>
                            </div>
                        </DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                </GridItem>
                <GridItem colSpan={2}>
                    <div><h3>&nbsp;</h3></div>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Invoice Paid</DataList.ItemLabel>
                        <DataList.ItemValue>
                            <Checkbox.Root checked={arHeaderModel?.is_paid}>
                                <Checkbox.Control />
                            </Checkbox.Root>
                        </DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Paid On</DataList.ItemLabel>
                        <DataList.ItemValue>{formatDateString(arHeaderModel?.paid_on || "")}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                    <DataList.Root hidden={arHeaderModel?.is_paid} size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel></DataList.ItemLabel>
                        <DataList.ItemValue>
                            <Button type="button" colorPalette="blue" onClick={() => makePayment()}>Make Payment</Button>
                        </DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                </GridItem>

                <GridItem colSpan={8}></GridItem>
                <GridItem colSpan={8} >
                    <div style={{ width: "100%", height: "450px" }}>
                    <AgGridReact
                        rowData={rowData}
                        columnDefs={colDefs}
                        defaultColDef={defaultColDef}
                        stopEditingWhenCellsLoseFocus={true}
                        />
                    </div>
                </GridItem>

                <GridItem colSpan={8} >
                <PageActionsComponent canSave={false} saveText={"Print Invoice"} canDelete={true} onSave={() => { printInvoice() }} onDelete={() => {}}/>
                </GridItem>
            </Grid>
        </form>
    )
}

export default AccountsReceivableViewPage;