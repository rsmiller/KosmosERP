"use client"

import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';

import { useForm } from 'react-hook-form';
import {
  Checkbox,
  DataList,
  Grid,
  GridItem,
} from '@chakra-ui/react';

import { useCallback, useEffect, useState, useRef } from "react";
import { AllCommunityModule, ColDef, GridApi, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import { useParams, useRouter } from 'next/navigation';
import { ARInvoiceHeaderCreateCommand, ARInvoiceLineCreateCommand } from '@/models/ar-models';
import { CustomerDto } from '@/models/customer-models';
import { OrderHeaderDto } from '@/models/sales-order-models';
import { orderService } from '@/services/order-service';
import { customerService } from '@/services/customer-service';
import PageActionsComponent from '@/components/page-actions';
import ARInvoiceQuantityEditor from '@/components/ag-grid/ar-invoice-quantity-editor';
import { addressService } from '@/services/address-service';
import { AddressDto, AddressFindCommand } from '@/models/address-models';
import { arInvoiceService } from '@/services/ar-invoice-service';
import { useAuth } from '@/lib/auth/auth-context';

import { format, parse } from 'date-fns';

import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewARFromCustomerPage() {
  const params = useParams();
  const router = useRouter();

  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);


  const [customerModel, setCustomerModel] = useState<CustomerDto>();
  const [orderModel, setOrderModel] = useState<OrderHeaderDto>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [canSave, setCanSave] = useState(false);
  const [saving, setSaving] = useState(false);

  const [invoiceDate, setInvoiceDate] = useState<string>();
  const [invoiceDueDate, setInvoiceDueDate] = useState<string>();

  const [billingAddress, setBillingAddress] = useState<AddressDto | undefined>();
  
  const hasInitialized = useRef(false);


  const loadOrderData = async () => {
      try {
        setLoading(true);
        const orderId = String(params.id);
        
        if (orderId === "") {
          setError('Invalid order ID');
          return;
        }

        const response = await orderService.getByGuid(orderId, auth.token || "");
        if (response.success && response.data) {

          setOrderModel(response.data);
          
          //console.log("getByGuid: ", response)

          // Get customer information if available
          if (response.data.customer_id) {
              try {
              const customerResponse = await customerService.get(response.data.customer_id, auth.token || "");
              if (customerResponse.success && customerResponse.data) {
                console.log(customerResponse)
                setCustomerModel(customerResponse.data);
                
                let days = 0;
                if(customerResponse.data.payment_terms_name)
                {
                  // TODO: If the user changes the name of the payment terms this will break
                  days = Number(customerResponse.data.payment_terms_name.replace("NET", ""));
                }

                const invoice_date = new Date();
                const due_date = new Date(invoice_date);
                due_date.setDate(due_date.getDate() + days);
                
                setInvoiceDueDate(due_date.toLocaleDateString());
                setInvoiceDate(new Date().toLocaleDateString());
                
                // Populate grid with order lines if available
                if (response.data.order_lines && response.data.order_lines.length > 0) {
                  setRowData([]);

                  for(let i=0; i<response.data.order_lines.length; i++)
                  {
                    let line = response.data.order_lines[i];
                    let ar_lines = response.data.ar_lines?.filter(m => m.order_line_id == line.id);
                    
                    let command = new ARInvoiceLineCreateCommand();
                    command.line_number = line.line_number;
                    command.line_description = line.line_description;
                    command.order_line_id = line.id;
                    command.product_id = line.product_id;
                    command.units_ordered = line.quantity;
                    command.unit_price = line.unit_price;
                    command.tax_rate = customerResponse.data?.tax_rate ? customerResponse.data?.tax_rate : 0;
                    command.is_taxable = customerResponse.data?.is_taxable ? customerResponse.data?.is_taxable : true;
                    command.shipped_qty = line.shipped_qty;

                    //console.log(command)

                    if(ar_lines && ar_lines.length)
                    {
                      let sum_of_invoice_qty = ar_lines.reduce((sum, ar_line) => sum + (ar_line.invoice_qty || 0), 0);
                      command.max_invoice_qty = line.quantity - sum_of_invoice_qty;
                      command.already_invoiced_qty = sum_of_invoice_qty;

                      if(sum_of_invoice_qty < line.quantity)
                      {
                        setRowData(prev => [...prev, command]);
                      }
                    }
                    else
                    {
                      command.max_invoice_qty = line.quantity;

                      setRowData(prev => [...prev, command]);
                    }
                  }
                }

                // Get them addresses and junk
                let address_find_comment = new AddressFindCommand();
                address_find_comment.customer_id = customerResponse.data.id;
                address_find_comment.address_type_id = 2; // Billing

                addressService.find(address_find_comment, auth.token || "").then( (addresses_response) =>
                {
                  if(addresses_response.success && addresses_response.data)
                  {
                    setBillingAddress(addresses_response.data[0]);
                  }
                  //console.log(addresses_response);
                });
              }
            } catch (err) {
              console.error('Error loading customer:', err);
            }
          }

          
          
        } else {
          setError('Failed to load order');
        }
      } catch (err) {
        console.error('Error loading order:', err);
        setError('Error loading order');
      } finally {
        setLoading(false);
      }
  };

  useEffect(() => {

    if(auth.authenticated == false){
      return;
    }

    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.ARModule,
      ERPModulePermission.Write,
      realmRoles
    );
    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    if (hasInitialized.current){
      return;
    }

    hasInitialized.current = true;



    loadOrderData();
  }, [params.id, auth.authenticated]);

  const handleSaveClick = async () => {
    let command = new ARInvoiceHeaderCreateCommand();
    command.customer_id = customerModel?.id;
    command.order_header_id = orderModel?.id;
    command.tax_percentage = customerModel?.tax_rate;
    command.is_taxable = customerModel?.is_taxable;
    command.payment_terms = customerModel?.payment_terms;
    command.ar_invoice_lines = new Array<ARInvoiceLineCreateCommand>();

    if(rowData)
    {
      for(let i=0;i<rowData.length; i++)
      {
        if(rowData[i] && rowData[i].invoice_qty != undefined && rowData[i].invoice_qty !== undefined && rowData[i].invoice_qty! >= 0)
        {
          let chaos = rowData[i];

          command.ar_invoice_lines.push(chaos);
        }
      }
    }


    if(invoiceDate)
    {
      command.invoice_date = format(invoiceDate, 'yyyy-MM-dd');
    }
    
    if(invoiceDueDate)
    {
      command.invoice_due_date = format(invoiceDueDate, 'yyyy-MM-dd');
    }

    /*
    if(invoiceDate)
    {
      command.invoice_date = new Date(invoiceDate);
    }
    
    if(invoiceDueDate)
    {
      command.invoice_due_date = new Date(invoiceDueDate);
    }
      */
    //console.log(rowData);
    //console.log(command);
    //return;

    setSaving(true);
    try
    {
      await arInvoiceService.create(command, auth.token || "").then( (response) =>
      {
        setSaving(false);

        if(response.success && response.data)
        {
          window.open(`/docs/api/?url=${encodeURIComponent('/docs/ar/' + response.data.guid)}`, "_blank")

          window.setTimeout(() => {
            router.push("/erp/ar/view/" + response.data?.guid);
          }, 2000)
          
        }
        else
        {
          console.error(response);
        }
      });
    }
    catch(e)
    {
      console.error(e);
    }
    
  };


  const invoiceQtyChanged = (changeObj: any) => {
    const api = changeObj.api as GridApi<ARInvoiceLineCreateCommand>;

    if(changeObj && changeObj.data)
    {
      let row = changeObj.data as ARInvoiceLineCreateCommand;

      console.log("ROW: ", row);

      let new_line_tax = 0;
      let new_total_price = 0;
      let new_invoice_qty = 0;

      if(row.invoice_qty)
      {
        new_invoice_qty = row.invoice_qty;

        if(row.unit_price)
        {
          if(row.is_taxable && row.is_taxable == true && row.tax_rate && row.tax_rate > 0)
          {
            const truncated_tax = Math.floor(((row.unit_price * row.tax_rate) * 100) * row.invoice_qty) / 100;
            //console.log("TAX: ", truncated_tax);

            const truncated_total = Math.floor((row.unit_price * row.invoice_qty) + truncated_tax);
            //console.log("Total: ", truncated_total);

            new_line_tax = truncated_tax;
            new_total_price = truncated_total;
          }
          else
          {
            const truncated_total = Math.floor((row.unit_price * row.invoice_qty) * 100) / 100;

            new_line_tax = 0;
            new_total_price = truncated_total;
          }
        }
      }
      else
      {
        setCanSave(false);
      }

      updateRowById(api, String(row.order_line_id), { invoice_qty: new_invoice_qty, line_tax: new_line_tax, total_price: new_total_price });

      api.forEachNode(node => {
        if (node.data) {
          //console.log("NODE: ", node.data)
          if(node.data.invoice_qty && node.data.invoice_qty > 0)
          {
            setCanSave(true);
          }
        }
      });
    }
    else
    {
      setCanSave(false);
    }
  }


  const updateRowById = (api: GridApi<ARInvoiceLineCreateCommand>, id: string, patch: Partial<ARInvoiceLineCreateCommand>) => {
    const node = api.getRowNode(id);
    if (!node) {
      //console.warn(`Row node not found for ID: ${id}`);
      return;
    }
    const updated = { ...node.data, ...patch };
    api.applyTransaction({ update: [updated] });

    setRowData(prev => prev.map(row => row.order_line_id === Number(id) ? updated : row));
  };

  const [rowData, setRowData] = useState<ARInvoiceLineCreateCommand[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<ARInvoiceLineCreateCommand>[]>([
    { field: "line_description", headerName: "Line Description", editable: true, filter: false },
    { headerName: "Units Ordered", cellRenderer: (params: any) => { return params.data.units_ordered }, filter: false },
    { field: "shipped_qty", headerName: "Units Shipped", filter: false,  },
    { field: "already_invoiced_qty", headerName: "Already Invoiced", filter: false,  },
    { field: "invoice_qty", headerName: "Units To Invoice", cellEditor: ARInvoiceQuantityEditor, editable: true, filter: false, onCellValueChanged: invoiceQtyChanged },
    { field: "is_taxable", headerName: "Tax", editable: true, filter: false },
    { field: "unit_price", headerName: "Unit Price", filter: false },
    { field: "line_tax", headerName: "Line Tax", filter: false },
    { field: "total_price", headerName: "Line Total", filter: false },
  ]);

  const getRowId = useCallback((params: any) => { 
    const id = String(params.data.order_line_id);
    //console.log('getRowId called with:', params.data, 'returning:', id);
    return id;
  }, []);

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
                  <DataList.ItemValue>{orderModel?.order_date}</DataList.ItemValue>
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
                  <DataList.ItemValue>{invoiceDate}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Due Date</DataList.ItemLabel>
                  <DataList.ItemValue>{invoiceDueDate}</DataList.ItemValue>
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
            <GridItem colSpan={8}></GridItem>
            <GridItem colSpan={8} >
              <div style={{ width: "100%", height: "450px" }}>
                <AgGridReact
                    rowData={rowData}
                    columnDefs={colDefs}
                    defaultColDef={defaultColDef}
                    stopEditingWhenCellsLoseFocus={true}
                    getRowId={getRowId}
                  />
                </div>
            </GridItem>
            

            <GridItem colSpan={8} >
              <PageActionsComponent canSave={!canSave} saveText={"Save and Print Invoice"} canDelete={false} onSave={() => handleSaveClick() } onDelete={() => {}}/>
            </GridItem>
        </Grid>
      </form>
  )
}

export default NewARFromCustomerPage;