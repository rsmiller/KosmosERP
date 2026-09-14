"use client";

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../../styles/date-picker.css';
import '../../../styles/page.component.css';
import '../../../styles/data-list.css';

import { Combobox, DataList, Field, Grid, GridItem, Input, Portal, useListCollection } from "@chakra-ui/react";
import { useEffect, useRef, useState } from "react";
import { useRouter } from 'next/navigation';
import { useForm } from "react-hook-form";

import { useKeycloak } from '@react-keycloak/web';

import PageActionsComponent from "@/components/page-actions";

import VendorCombobox, { VendorComboboxRef } from "@/components/vendor-combobox";

import DatePicker from "react-datepicker";
import { APInvoiceHeaderCreateCommand } from '@/models/ap-models';
import { ERPModulePermission, ERPModules, permissionsService } from '@/services/permissions-service';
import { OrderHeaderFindCommand } from '@/models/sales-order-models';
import { orderService } from '@/services/order-service';
import { PurchaseOrderHeaderFindCommand } from '@/models/purchase-order-models';
import { purchaseOrderService } from '@/services/purchase-order-service';
import { ARInvoiceHeaderFindCommand } from '@/models/ar-models';
import { arInvoiceService } from '@/services/ar-invoice-service';
import { apInvoiceService } from '@/services/ap-invoice-service';

class ObjectTypeDto
{
    id: number = 0;
    name: string = "";
}

class SuperCoolObjectDto
{
    id: number = 0;
    name: string = "";
}

function NewAPPage() {
    const router = useRouter();
    const { keycloak } = useKeycloak();
    const [hasAccess, setHasAccess] = useState(true);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [saveable, setSaveable] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    const [saving, setSaving] = useState(false);

    const [selectedObjectTypeValue, setSelectedObjectTypeValue] = useState<string>();
    const [selectedObjectTypeItem, setSelectedObjectTypeItem] = useState<any[]>([]);
    const [objectTypes, setObjectTypes] = useState<ObjectTypeDto[]>([]);
    const { collection : objectTypeCollection, set : setObjectTypeCollection } = useListCollection<ObjectTypeDto>({
        initialItems: objectTypes,
        itemToString: (item) => item.id.toString(),
        itemToValue: (item) => item.name ? item.name : "-- ERROR --",
    })

    const [selectedObjectValue, setSelectedObjectValue] = useState<string>();
    const [selectedObjectItem, setSelectedObjectItem] = useState<any[]>([]);
    const [objects, setObjects] = useState<SuperCoolObjectDto[]>([]);
    const { collection : objectCollection, set : setObjectCollection } = useListCollection<SuperCoolObjectDto>({
        initialItems: objects,
        itemToString: (item) => item.id.toString(),
        itemToValue: (item) => item.name ? item.name : "-- ERROR --",
    })

    const vendorComboboxRef = useRef<VendorComboboxRef>(null);

    
    const {
        register,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
    } = useForm<APInvoiceHeaderCreateCommand>();

      useEffect(() => {
    
        if(keycloak.authenticated == false) return;
    
        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.APModule,
          ERPModulePermission.Write,
          realmRoles
        );

        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        // Load object types
        const types: ObjectTypeDto[] = [
            { id: 1, name: "Purchase Order" },
            { id: 2, name: "Sales Order" },
            { id: 3, name: "AR Invoice" },
        ];
        setObjectTypes(types);
        setObjectTypeCollection(types);


    }, [keycloak.authenticated]);

    const handleReceivedDateChange = (date: Date | null) => {
        setValue('invoice_received_date', date || undefined);
        CheckFormValidity();
    };

    const getInvoiceDate = () => {
        const invoiceDate = watch('invoice_date');
        return invoiceDate ? new Date(invoiceDate) : undefined;
    };

    const handleInvoiceDateChange = (date: Date | null) => {
        setValue('invoice_date', date || undefined);
        CheckFormValidity();
    };

    const handleVendorSelect = (value: any) => {
        setValue('vendor_id', value?.id || 0);
        CheckFormValidity();
    }

    const getDueDate = () => {
        const auditOn = watch('invoice_due_date');
        return auditOn ? new Date(auditOn) : undefined;
    };

    const handleDueDateChange = (date: Date | null) => {
        setValue('invoice_due_date', date || undefined);
        CheckFormValidity();
    };

    const getReceivedDate = () => {
        const receivedOn = watch('invoice_received_date');
        return receivedOn ? new Date(receivedOn) : undefined;
    };

    const handleSaveClick = () => {
        
        let command = new APInvoiceHeaderCreateCommand();
        command.invoice_number = watch('invoice_number') || '';
        command.vendor_id = watch('vendor_id') || 0;
        command.invoice_date = watch('invoice_date') || new Date();
        command.invoice_due_date = watch('invoice_due_date') || new Date();
        command.invoice_received_date = watch('invoice_received_date') || new Date();
        command.association_object_id = selectedObjectItem[0]?.id || 0;

        console.log("Selected Object Type Value: ", selectedObjectTypeValue);   
        if(selectedObjectTypeValue && selectedObjectTypeValue == "Purchase Order")
        {
            command.association_is_purchase_order = true;
        }
        else if(selectedObjectTypeValue && selectedObjectTypeValue == "Sales Order")
        {
            command.association_is_sales_order = true;
        }
        else
        {
            command.association_is_ar_invoice = true;
        }

        //console.log("Save Command: ", command);

        //return;

        setSaving(true);
        apInvoiceService.create(command, keycloak?.token || "").then((response) => {
            setSaving(false);
            if (response.success) {
                setSuccessSaved(true);
                setFailedSaved(false);
                router.push('/erp/ap/edit/' + response.data?.guid);
            } else {
                setSuccessSaved(false);
                setFailedSaved(true);
            }
        });
    }
    

    const CheckFormValidity = () => {
        const vendorValid = vendorComboboxRef.current?.isValid() || false;
        const hasRequiredFields = Boolean(watch('invoice_number') 
                                            && watch('vendor_id') 
                                            && watch('invoice_date') 
                                            && watch('invoice_due_date') 
                                            && watch('invoice_received_date')
                                            && selectedObjectTypeValue
                                            && selectedObjectValue);

        const allValid = vendorValid && hasRequiredFields;

        setSaveable(allValid);
    }

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }

    const objectTypeInputChange = (inputValue: any) => {
        const selectedItem = objectTypes.find(item => item.name === inputValue.value[0]);

        if (selectedItem) {
            setSelectedObjectTypeValue(inputValue.value[0]);
            setSelectedObjectTypeItem([selectedItem]);
        } 
        else 
            {
            setSelectedObjectTypeValue('');
            setSelectedObjectTypeItem([]);
        }
    }

    const objectInputChange = (inputValue: any) => {
        //console.log("Selected Object Input Changed: ", inputValue);
        //console.log("Available Objects: ", objects);
        
        const selectedItem = objects.find(item => item.name === inputValue.value[0]);

        if (selectedItem) {
            setSelectedObjectValue(inputValue.value[0]);
            setSelectedObjectItem([selectedItem]);
        } 
        else 
            {
            setSelectedObjectValue('');
            setSelectedObjectItem([]);
        }
    }

    const objectInputValChange = (inputValue: any) => {
        //console.log("Input Value Changed: ", inputValue);

        if(selectedObjectTypeItem && selectedObjectTypeItem.length != 0)
        {
            if(selectedObjectTypeItem[0].id == 1) // Purchase Order
            {
                let command = new PurchaseOrderHeaderFindCommand();
                command.wildcard = inputValue;

                purchaseOrderService.find(command, keycloak?.token || "").then((response) =>
                {
                    if(response.success && response.data)
                    {
                        const newObjects: SuperCoolObjectDto[] = response.data.map(record => ({
                            id: record.id,
                            name: record.po_number?.toString() + " - " + record.vendor_name || ""
                        }));

                        setObjects(newObjects);
                        setObjectCollection(newObjects);
                    }
                });
            }
            else if(selectedObjectTypeItem[0].id == 2) // Sales Order
            {
                let command = new OrderHeaderFindCommand();
                command.wildcard = inputValue;

                orderService.find(command, keycloak?.token || "").then((response) =>
                {
                    if(response.success && response.data)
                    {
                        const newObjects: SuperCoolObjectDto[] = response.data.map(record => ({
                            id: record.id,
                            name: record.order_number?.toString() + " - " + record.customer_name || ""
                        }));

                        setObjects(newObjects);
                        setObjectCollection(newObjects);
                    }
                });
            }
            else if(selectedObjectTypeItem[0].id == 3) // AR Invoice
            {
                let command = new ARInvoiceHeaderFindCommand();
                command.wildcard = inputValue;

                arInvoiceService.find(command, keycloak?.token || "").then((response) =>
                {
                    if(response.success && response.data)
                    {
                        const newObjects: SuperCoolObjectDto[] = response.data.map(record => ({
                            id: record.id,
                            name: record.invoice_number?.toString() + " - " + record.customer_name || ""
                        }));
                        
                        setObjects(newObjects);
                        setObjectCollection(newObjects);
                    } 
                });
            }
            console.log("Currently selected object type: ", selectedObjectTypeItem);
        }
        
    }

    return (
        <form>
          <Grid
                templateColumns="repeat(5, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
              >
                <GridItem colSpan={2}>
                    
                    <div><h3>Invoice</h3></div>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                      <DataList.Item>
                        <DataList.ItemLabel>Invoice Number</DataList.ItemLabel>
                        <DataList.ItemValue>
                          <Field.Root required={true} invalid={IsDirty('invoice_number')}>
                            <Input  {...register('invoice_number')} />
                          </Field.Root>
                        </DataList.ItemValue>
                      </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                      <DataList.Item>
                        <DataList.ItemLabel>Vendor Name</DataList.ItemLabel>
                        <DataList.ItemValue>
                          <VendorCombobox 
                              ref={vendorComboboxRef}
                              dbKey={watch('vendor_id') || 0}
                              control={control}
                              name="vendor_id"
                              error={errors.vendor_id}
                              onChange={handleVendorSelect}
                              onValidationChange={CheckFormValidity} 
                              disabled={false}                      
                            />
                        </DataList.ItemValue>
                      </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                      <DataList.Item>
                        <DataList.ItemLabel>Invoice Date</DataList.ItemLabel>
                        <DataList.ItemValue>
                          <DatePicker 
                              selected={getInvoiceDate()}
                              onChange={handleInvoiceDateChange}
                              dateFormat="MM/dd/yyyy"
                              placeholderText="Select date"
                              required={true}
                            />
                        </DataList.ItemValue>
                      </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                      <DataList.Item>
                        <DataList.ItemLabel>Received Date</DataList.ItemLabel>
                        <DataList.ItemValue>
                           <DatePicker 
                              selected={getReceivedDate()}
                              onChange={handleReceivedDateChange}
                              dateFormat="MM/dd/yyyy"
                              placeholderText="Select date"
                              required={true}
                            />
                        </DataList.ItemValue>
                      </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                      <DataList.Item>
                        <DataList.ItemLabel>Due Date</DataList.ItemLabel>
                        <DataList.ItemValue>
                          <DatePicker 
                              selected={getDueDate()}
                              onChange={handleDueDateChange}
                              dateFormat="MM/dd/yyyy"
                              placeholderText="Select date"
                              required={true}
                              
                            />
                        </DataList.ItemValue>
                      </DataList.Item>
                    </DataList.Root>
                </GridItem>
    
                <GridItem colSpan={2}>
                    <div><h3>Associated Object</h3></div>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                      <DataList.Item>
                        <DataList.ItemLabel>Object Type</DataList.ItemLabel>
                        <DataList.ItemValue>
                            <Combobox.Root
                                collection={objectTypeCollection}
                                inputValue={selectedObjectTypeValue}
                                value={selectedObjectTypeItem}
                                onValueChange={(e) => objectTypeInputChange(e)}
                                allowCustomValue={false}
                                width="100%"
                                invalid={!(selectedObjectTypeValue && selectedObjectTypeValue.length > 0)}
                            >
                                <Combobox.Control>
                                <Combobox.Input/>
                                <Combobox.IndicatorGroup>
                                    <Combobox.ClearTrigger />
                                    <Combobox.Trigger />
                                </Combobox.IndicatorGroup>
                                </Combobox.Control>
                                <Portal>
                                    <Combobox.Positioner>
                                        <Combobox.Content>
                                        <Combobox.Empty>
                                            {loading ? "Loading..." : "No items found"}
                                        </Combobox.Empty>
                                        {objectTypeCollection.items.map((item) => (
                                            <Combobox.Item item={item} key={item.id}>
                                            {item.name}
                                            <Combobox.ItemIndicator />
                                            </Combobox.Item>
                                        ))}
                                        </Combobox.Content>
                                    </Combobox.Positioner>
                                </Portal>
                            </Combobox.Root>
                        </DataList.ItemValue>
                      </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                      <DataList.Item>
                        <DataList.ItemLabel>Object Number</DataList.ItemLabel>
                        <DataList.ItemValue>

                            <Combobox.Root
                                collection={objectCollection}
                                inputValue={selectedObjectValue}
                                value={selectedObjectItem}
                                onValueChange={(e) => objectInputChange(e)}
                                onInputValueChange={(e) => objectInputValChange(e.inputValue)}
                                allowCustomValue={false}
                                width="100%"
                                invalid={!(selectedObjectValue && selectedObjectValue.length > 0)}
                            >
                                <Combobox.Control>
                                <Combobox.Input placeholder="Type to search" />
                                <Combobox.IndicatorGroup>
                                    <Combobox.ClearTrigger />
                                    <Combobox.Trigger />
                                </Combobox.IndicatorGroup>
                                </Combobox.Control>
                                <Portal>
                                    <Combobox.Positioner>
                                        <Combobox.Content>
                                        <Combobox.Empty>
                                            {loading ? "Loading..." : "No items found"}
                                        </Combobox.Empty>
                                        {objectCollection.items.map((item) => (
                                            <Combobox.Item item={item} key={item.id}>
                                            {item.name}
                                            <Combobox.ItemIndicator />
                                            </Combobox.Item>
                                        ))}
                                        </Combobox.Content>
                                    </Combobox.Positioner>
                                </Portal>
                            </Combobox.Root>
                        </DataList.ItemValue>
                      </DataList.Item>
                    </DataList.Root>
                </GridItem>
    
                <GridItem colSpan={5} >
                  <PageActionsComponent 
                    canSave={!saveable} 
                    canDelete={false}
                    onSave={handleSaveClick} 
                    onDelete={() => {}} 
                    successSaved={successSaved}
                    failedSaved={failedSaved}
                  />
                </GridItem>
            </Grid>
          </form>
      )
}

export default NewAPPage;