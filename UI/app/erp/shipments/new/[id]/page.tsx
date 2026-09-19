"use client";

import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';


import { ShipmentHeaderCreateCommand, ShipmentHeaderDto, ShipmentHeaderEditCommand, ShipmentHeaderFindCommand, ShipmentLineCreateCommand, ShipmentLineDto } from '@/models/shipments-models';
import { shipmentService } from '@/services/shipment-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { DataList, Field, Grid, GridItem, NumberInput, Input } from '@chakra-ui/react';
import { useAuth } from '@/lib/auth/auth-context';

import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";

import { useParams, useRouter } from 'next/navigation';
import { useEffect, useRef, useState } from 'react';
import { AgGridReact } from 'ag-grid-react';
import PageActionsComponent from '@/components/page-actions';
import { orderService } from '@/services/order-service';
import { OrderHeaderDto } from '@/models/sales-order-models';
import { useForm } from 'react-hook-form';
import NumericForOrdersShipped from '@/components/ag-grid/numeric-for-orders-shipped';
import FreightCombobox, { FreightComboboxRef } from '@/components/freight-combobox';
import ShipmentMethodCombobox from '@/components/shipment-method-combobox';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewShipmentPage() {
    const auth = useAuth();

    const router = useRouter();
    const params = useParams();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasWritePermission, setHasWritePermission] = useState(false);
    const hasInitialized = useRef(false);

    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [shipment, setShipment] = useState<ShipmentHeaderDto | null>(null);
    const [orderHeader, setOrderHeader] = useState<OrderHeaderDto | null>(null);
    const [rowData, setRowData] = useState<ShipmentLineCreateCommand[]>([]);

    const freightComboboxRef = useRef<FreightComboboxRef>(null);
    
    const {
        register,
        handleSubmit,
        formState: { errors },
        setValue,
        watch,
        control,
    } = useForm<ShipmentHeaderCreateCommand>();

    useEffect(() => {
    
        if(auth.authenticated == false) return;
    
        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ShippingModule,
          ERPModulePermission.Write,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasWritePermission(true);

        if (hasInitialized.current) return;
        
        hasInitialized.current = true;
        
        
    
        loadShipment();
    }, [params.id, auth.authenticated, router]);

    const loadShipment = async () => {
        try {
            setLoading(true);
            const orderId = String(params.id);
            
            if (orderId == "") {
                setError('Invalid order ID');
                return;
            }
            setRowData([]);

            await orderService.getByGuid(orderId, auth.token || "").then(async (response) => {
                

                if (response.success && response.data) 
                {

                    setOrderHeader(response.data);
                    setValue("ship_via", response.data.shipping_method || '');


                    var findCommand = new ShipmentHeaderFindCommand();
                    findCommand.order_guid = orderId;
                    
                    await shipmentService.find(findCommand, auth.token || "").then(async (order_response) => {

                        let all_lines = new Array<ShipmentLineDto>();

                        if (order_response.success && order_response.data) 
                        {
                            for (let shipment of order_response.data) 
                            {
                                for (let line of shipment.shipment_lines || []) 
                                {
                                    all_lines.push(line);
                                }
                            }

                            for (let line of response.data?.order_lines || []) 
                            {
                                let found_order_line = all_lines.filter(sl => sl.order_line_id == line.id);

                                let shipmentLine = new ShipmentLineCreateCommand();
                                shipmentLine.order_line_id = line.id;
                                shipmentLine.line_number = line.line_number;
                                shipmentLine.line_description = line.line_description;
                                shipmentLine.units_to_ship = line.quantity;
                                shipmentLine.units_shipped = 0;
                                shipmentLine.units_ordered = line.quantity;

                                if(found_order_line != undefined)
                                {
                                    let shipped_already = found_order_line.reduce((acc, curr) => acc + (curr.units_shipped || 0), 0);
                                    shipmentLine.units_already_shipped = shipped_already || 0;

                                    //shipmentLine.units_already_shipped = (shipmentLine.units_ordered || 0) - (shipmentLine.units_shipped || 0);
                                }
                                
                                setRowData((prevRowData) => [...prevRowData, shipmentLine]);
                            }
                        }

                        setLoading(false);
                        
                    });

                    //console.log(response.data);
                }
                
            });
        } catch (err) {
            console.error('Error loading shipment:', err);
            setError('Error loading shipment');
        }
    }

    const handleFrieghtSelect = (value: any) => {
        //console.log(value)

        if(value == null)
        {
            setValue('freight_carrier', undefined);
        }
        else
        {
            setValue('freight_carrier', value?.key || '');
        }

        CheckFormValidity();
    };

    const handleShipmentSelect = (value: any) => {
        setValue('ship_via', value?.key);
        CheckFormValidity();
    };

    const CheckFormValidity = () => {
        const hasRequiredFields = Boolean(watch('freight_charge_amount') && watch("freight_carrier") && watch("ship_via"));

        //console.log("hasRequiredFields: ", hasRequiredFields);
        
        canSave(hasRequiredFields);
    };

    const handleSaveClick = async () => {
        let command = new ShipmentHeaderCreateCommand();

        command.order_header_id = orderHeader?.id || 0;
        command.freight_carrier = watch('freight_carrier') || '';
        command.freight_charge_amount = watch('freight_charge_amount') || 0;
        command.tax = watch('tax') || 0;
        command.address_id = orderHeader?.ship_to_address?.id || 0;
        command.ship_attn = watch('ship_attn') || '';
        command.ship_via = watch('ship_via') || '';
        command.shipment_lines = rowData;
        command.is_canceled = false;
        command.is_complete = false;

        //console.log("Create Shipment Command: ", command);

        //return;
        await shipmentService.create(command, auth.token || "").then((response) => {
            //console.log(response);

            if (response.success) {
                setSuccessSaved(true);

                router.push("/erp/shipments/view/" + response.data?.guid);
            } else {
                setFailedSaved(true);
            }
        });
    }

    const [colDefs, setColDefs] = useState<ColDef<ShipmentLineCreateCommand>[]>([
        { field: "line_number", headerName: "Line #"},
        { field: "line_description", headerName: "Description"},
        { field: "units_ordered", headerName: "Order Qty"},
        { field: "units_already_shipped", headerName: "Already Shipped"},
        { field: "units_shipped", headerName: "Units To Ship", editable: true, cellEditor: NumericForOrdersShipped },
    ]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: true,
    };

    if (loading) {
        return <div>Loading shipment...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    return (
        <form onChange={CheckFormValidity}>
        
            <Grid
                templateColumns="repeat(6, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={2}>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                            <DataList.ItemLabel>Customer</DataList.ItemLabel>
                            <DataList.ItemValue>{orderHeader?.customer_name}</DataList.ItemValue>
                        </DataList.Item>
                        <DataList.Item>
                            <DataList.ItemLabel>Ship To</DataList.ItemLabel>
                            <DataList.ItemValue>
                                {orderHeader?.ship_to_address?.street_address1}<br/>
                                {orderHeader?.ship_to_address?.city}, {orderHeader?.ship_to_address?.state}<br/>
                                {orderHeader?.ship_to_address?.country}
                            </DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                </GridItem>

                <GridItem colSpan={2}>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md" maxH="md">
                        <DataList.Item>
                            <DataList.ItemLabel>Order Number</DataList.ItemLabel>
                            <DataList.ItemValue>{orderHeader?.order_number}</DataList.ItemValue>
                        </DataList.Item>
                        <DataList.Item>
                            <DataList.ItemLabel>Order Date</DataList.ItemLabel>
                            <DataList.ItemValue>{orderHeader?.order_date}</DataList.ItemValue>
                        </DataList.Item>
                        <DataList.Item>
                            <DataList.ItemLabel>Required Date</DataList.ItemLabel>
                            <DataList.ItemValue>{orderHeader?.required_date}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                </GridItem>

                <GridItem colSpan={2}></GridItem>

                <GridItem colSpan={1}>
                    <Field.Root invalid={!!errors.ship_via} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Shipping Method</Field.Label>
                        <ShipmentMethodCombobox 
                            dbKey={watch('ship_via') || ''}
                            onChange={handleShipmentSelect}
                            disabled={false}
                        />
                </Field.Root>
                </GridItem>

                <GridItem colSpan={1}>
                    <Field.Root invalid={!!errors.freight_carrier} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Freight Carrier</Field.Label>
                        <FreightCombobox 
                            dbKey={watch('freight_carrier') || ''}
                            control={control}
                            name="freight_carrier"
                            error={errors.freight_carrier}
                            onChange={handleFrieghtSelect}
                            ref={freightComboboxRef} 
                            disabled={false}/>
                    </Field.Root>
                </GridItem>
                <GridItem colSpan={1}>
                    <Field.Root invalid={!!errors.freight_charge_amount}>
                        <Field.Label><Field.RequiredIndicator /> Freight Charge</Field.Label>
                        <NumberInput.Root 
                            defaultValue={String(watch('freight_charge_amount') || 0)}
                            min={0}
                        >
                            <NumberInput.Control/>
                            <NumberInput.Input {...register('freight_charge_amount')} />
                        </NumberInput.Root>
                    </Field.Root>
                </GridItem>
                <GridItem colSpan={1}>
                    <Field.Root invalid={!!errors.ship_attn}>
                        <Field.Label>Ship Tax</Field.Label>
                        <Field.Root invalid={!!errors.tax}>
                            <NumberInput.Root 
                                defaultValue={String(watch('tax') || 0)}
                                min={0}
                            >
                                <NumberInput.Control/>
                                <NumberInput.Input {...register('tax')} />
                            </NumberInput.Root>
                        </Field.Root>
                    </Field.Root>
                </GridItem>

                <GridItem colSpan={2}></GridItem>

                <GridItem colSpan={1}>
                    <Field.Root invalid={!!errors.ship_attn}>
                        <Field.Label>Ship Attention</Field.Label>
                        <Input 
                            {...register('ship_attn')} 
                        />
                    </Field.Root>
                </GridItem>

                <GridItem colSpan={6}></GridItem>
                <GridItem colSpan={6}>
                    <div style={{ width: "100%", height: "500px" }}>
                        <AgGridReact
                            loading={loading}
                            rowData={rowData}
                            columnDefs={colDefs}
                            defaultColDef={defaultColDef}
                        />
                    </div>
                </GridItem>

                <GridItem colSpan={6} >
                    <PageActionsComponent 
                        canSave={!saveable || !hasWritePermission} 
                        onSave={handleSaveClick} 
                        successSaved={successSaved}
                        failedSaved={failedSaved}
                    />
                </GridItem>
            </Grid>
        </form>
    );
}

export default NewShipmentPage;