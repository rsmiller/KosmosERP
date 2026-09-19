"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';
import '../../../../styles/date-picker.css';


import { useEffect, useState, useRef } from "react";
import { Combobox, DataList, Field, Grid, GridItem, Portal, useFilter, useListCollection } from '@chakra-ui/react'
import { useParams, useRouter } from 'next/navigation';
import { SubscriptionDeleteCommand, SubscriptionDto, SubscriptionEditCommand } from '@/models/subscription-models';
import { subscriptionService } from '@/services/subscription-service';
import { useForm } from 'react-hook-form';
import { format } from 'date-fns';
import DatePicker from 'react-datepicker';
import SalesOrdersLinesComponent from '@/components/lists/sales-order-lines-component';
import { CurrencyHelper } from '@/helpers/CurrencyHelper';
import PageActionsComponent from '@/components/page-actions';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

function EditSubscriptionsPage() {
    const auth = useAuth();
    const params = useParams();
    const router = useRouter();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);
    const [hasDeletePermission, setHasDeletePermission] = useState(false);
    
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);

    const [subscription, setSubscription] = useState<SubscriptionDto | null>(null);

    const [error, setError] = useState<string | null>(null);

    const [loading, setLoading] = useState(true);

    const { contains } = useFilter({ sensitivity: "base" })
        
    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [saveable, canSave] = useState(false);

    const thevalues = [
        { label: "7 Days", value: "7" },
        { label: "14 Days", value: "14" },
        { label: "30 Days", value: "30" },
        { label: "182 Days", value: "182" },
        { label: "365 Days", value: "365" },
    ];

    const { collection, filter } = useListCollection({
        initialItems: thevalues,
        filter: contains,
    })

    const {
        formState: { errors, isValid },
        setValue,
        watch,
      } = useForm<SubscriptionEditCommand>();

    const hasInitialized = useRef(false);

    const loadSubscription = async () => {
        try {
            setLoading(true);
            const subscriptionId = String(params.id);
            
            if (subscriptionId == "") {
                setError('Invalid customer ID');
                return;
            }
    
            const response = await subscriptionService.getByGuid(subscriptionId, auth.token || "");
            if (response.success && response.data) {
                //console.log(response)
                setSubscription(response.data);
                
                let cycle_days = (response.data.cycle_days ? response.data.cycle_days : 0);

                // Set form values with loaded data
                setValue('id', response.data.id as number);
                setValue('cycle_days', cycle_days);
                setValue('start_date', response.data.start_date);

                
                let selectedItem = thevalues.filter(m => m.value == cycle_days.toString());

                if(selectedItem.length > 0)
                {
                    setSelectedItem([selectedItem[0].value]);
                    setSelectedValue(selectedItem[0].label)
                }
                
            } else {
                setError('Failed to load subscription');
            }
        } catch (err) {
            console.error('Error loading Subscription:', err);
            setError('Error loading Subscription');
        } finally {
            setLoading(false);
        }
    };


    useEffect(() => {
        if(auth.authenticated == false) return;

        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.SubscriptionModule,
          ERPModulePermission.Read,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        // Check Edit permission
        const canEdit = permissionsService.HasPermission(
          ERPModules.SubscriptionModule,
          ERPModulePermission.Edit,
          realmRoles
        );
        setHasEditPermission(canEdit);

        // Check Delete permission
        const canDelete = permissionsService.HasPermission(
          ERPModules.SubscriptionModule,
          ERPModulePermission.Delete,
          realmRoles
        );
        setHasDeletePermission(canDelete);

        if (hasInitialized.current) return;
        hasInitialized.current = true;
        
    
        loadSubscription();
    }, [params.id, setValue, auth.authenticated]);


    const DateFormatter = (date: any) =>
    {
        if(date)
        {
            return format(date, 'MM-dd-yyyy');
        }
        else
        {
            return "";
        }
    }

    if (loading) {
        return <div>Loading Subscription...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }

        return false;
    }
    
    const getStartDate = () => {
        const startDate = watch('start_date');

        return startDate ? new Date(startDate + "T00:01:00") : undefined;
    };

    const handleStartDateChange = (date: Date | null) => {
        if(date != null)
        {
            setValue('start_date', format(date, 'yyyy-MM-dd'));
            CheckFormValidity();
        }
    };

    const inputChange = (inputValue: any) => {
        if(inputValue.value[0] != undefined)
        {
            setValue("cycle_days", inputValue.value[0]);
        
            let selectedItem = thevalues.filter(m => m.value == inputValue.value[0].toString());

            if(selectedItem.length > 0)
            {
                setSelectedItem([selectedItem[0].value]);
                setSelectedValue(selectedItem[0].label)
            }
        }
        

        CheckFormValidity();
    }

    const CheckFormValidity = () => 
    {
        var valid = Boolean(watch("start_date") && watch("cycle_days"));

        if(valid)
        {
            canSave(true);
        }
        else
        {
            canSave(false);
        }
    }

    const handleSaveClick = async () => 
    {
        let command = new SubscriptionEditCommand();
        command.id = subscription?.id;
        command.cycle_days = watch("cycle_days");
        command.start_date = watch("start_date");

        try {
            await subscriptionService.update(command, auth.token || "").then((response) => {
            if (response.success && response.data) {

                setSubscription(response.data);

                setSuccessSaved(true);
                setFailedSaved(false);
            } else {
                setSuccessSaved(false);
                setFailedSaved(true);
            }
            });
        } catch (e) {
            console.error(e);
            setSuccessSaved(false);
            setFailedSaved(true);
        }
    }

    const handleDeleteClick = async () => {
        let command = new SubscriptionDeleteCommand();
        command.id = subscription?.id;
    
        try {
            await subscriptionService.delete(command, auth.token || "").then((response) => {
                if (response.success) {
                    router.push("/erp/subscriptions/");
                } else {
                    setSuccessSaved(false);
                    setFailedSaved(true);
                }
            });
        } catch (e) {
            setSuccessSaved(false);
            setFailedSaved(true);
        }
    }

    return (
        <Grid
            templateColumns="repeat(4, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
            >
            <GridItem colSpan={1} gap={0}>
                <h3>Subscription Information</h3>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Subscription #</DataList.ItemLabel>
                        <DataList.ItemValue>{subscription?.subscription_number}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md" className="chunky">
                    <DataList.Item>
                        <DataList.ItemLabel>Terms</DataList.ItemLabel>
                        <DataList.ItemValue>
                            <Field.Root invalid={IsDirty('cycle_days')} required={true}>
                            <Combobox.Root
                                collection={collection}
                                inputValue={selectedValue}
                                value={selectedItem}
                                onValueChange={(e) => inputChange(e)}
                                allowCustomValue={false}
                                required={true}
                                width="100%"
                            >
                                <Combobox.Control>
                                <Combobox.Input placeholder="Choose Type" />
                                <Combobox.IndicatorGroup>
                                    <Combobox.ClearTrigger />
                                    <Combobox.Trigger />
                                </Combobox.IndicatorGroup>
                                </Combobox.Control>
                                <Portal>
                                    <Combobox.Positioner>
                                        <Combobox.Content>
                                        <Combobox.Empty>No items found</Combobox.Empty>
                                        {collection.items.map((item) => (
                                            <Combobox.Item item={item} key={item.value}>
                                            {item.label}
                                            <Combobox.ItemIndicator />
                                            </Combobox.Item>
                                        ))}
                                        </Combobox.Content>
                                    </Combobox.Positioner>
                                </Portal>
                            </Combobox.Root>
                        </Field.Root>
                        </DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md" className="chunky">
                    <DataList.Item>
                        <DataList.ItemLabel>Start Date</DataList.ItemLabel>
                        <DataList.ItemValue>
                            <Field.Root invalid={IsDirty('start_date')} required={true}>
                                <DatePicker 
                                    selected={getStartDate()}
                                    onChange={handleStartDateChange}
                                    dateFormat="MM/dd/yyyy"
                                    placeholderText="Select date"
                                    
                                />
                            </Field.Root>
                        </DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={1}>
                <h3>&nbsp;</h3>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>&nbsp;</DataList.ItemLabel>
                        <DataList.ItemValue>&nbsp;</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md" className="chunky">
                    <DataList.Item>
                        <DataList.ItemLabel>&nbsp;</DataList.ItemLabel>
                        <DataList.ItemValue>&nbsp;</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md" className="chunky">
                    <DataList.Item>
                        <DataList.ItemLabel>Next Date</DataList.ItemLabel>
                        <DataList.ItemValue>{DateFormatter(subscription?.next_date)}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={1}>
                <h3>Order Information</h3>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Order Number</DataList.ItemLabel>
                        <DataList.ItemValue>{subscription?.order?.order_number}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md" className="chunky">
                    <DataList.Item>
                        <DataList.ItemLabel>Order Date</DataList.ItemLabel>
                        <DataList.ItemValue>{DateFormatter(subscription?.order?.order_date)}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={1}>
                <h3>&nbsp;</h3>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Price</DataList.ItemLabel>
                        <DataList.ItemValue>{CurrencyHelper(subscription?.order?.price)}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md" className="chunky">
                    <DataList.Item>
                        <DataList.ItemLabel>Tax</DataList.ItemLabel>
                        <DataList.ItemValue>{CurrencyHelper(subscription?.order?.tax)}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={4} >
                <h3>Order Lines</h3>
                <SalesOrdersLinesComponent order_header_id={subscription?.order_header_id} onSelect={() => {}}/>
            </GridItem>

            <GridItem colSpan={4} >
              <PageActionsComponent 
                onSave={handleSaveClick} 
                onDelete={handleDeleteClick}
                canSave={!saveable || !hasEditPermission}
                canDelete={hasDeletePermission}
                successSaved={successSaved}
                failedSaved={failedSaved}
                deleteText="Cancel Subscription"
              />
            </GridItem>
        </Grid>
    );
}

export default EditSubscriptionsPage;