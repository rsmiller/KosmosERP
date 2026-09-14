import { Button, CloseButton, Combobox, Dialog, Field, Grid, GridItem, Input, Portal, Stack, useFilter, useListCollection, VStack } from "@chakra-ui/react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { FaSearch } from "react-icons/fa";
import SalesOrdersSelectListComponent from "./lists/sales-orders-select-list-component";
import DatePicker from "react-datepicker";
import { format } from 'date-fns';

export class SalesOrderSelectorForm
{
    customer_id: any;
    form_order_header_id: any;
    form_order_number: any;
    terms_value: any;
    start_date: any;
    price: any;
    quantity: any;
    tax: any;

    constructor(init?: Partial<SalesOrderSelectorForm>) {
        Object.assign(this, init);
    }
}

export class SalesOrderSelectorComponentParams
{
    customer_id: any;
    order_header_id: any;
    onChange?: (SalesOrderSelectorForm: any) => void;
}


function SalesOrderSelectorComponent({customer_id, order_header_id, onChange}: SalesOrderSelectorComponentParams) {
    const { contains } = useFilter({ sensitivity: "base" })
    
    const [selectedValue, setSelectedValue] = useState<string>();
    const [selectedItem, setSelectedItem] = useState<any[]>([]);
    const [isDialogOpen, setDialogOpen] = useState<boolean>(false);
    const [saveable, canSave] = useState(false);

    const thevalues = [
        { label: "7 Days", value: "7" },
        { label: "14 Days", value: "14" },
        { label: "30 Days", value: "30" },
        { label: "182 Days", value: "182" },
        { label: "365 Days", value: "365" },
    ];
    
    
    const {
          register,
          formState: { errors, isValid },
          setValue,
          watch,
      } = useForm<SalesOrderSelectorForm>();


    const { collection, filter } = useListCollection({
        initialItems: thevalues,
        filter: contains,
    })

    

    const handleSearchClick = () => {
        setDialogOpen(true);
    }

    const inputChange = (inputValue: any) => {
        setValue("terms_value", inputValue.value[0]);

        CheckFormValidity();
    }

    const chooseOrder = (order: any) => {
        //console.log(order);
        
        setValue("price", order.price);
        setValue("form_order_header_id", order.id);
        setValue("form_order_number", order.order_number);
        setDialogOpen(false);

        CheckFormValidity();
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
        return startDate ? new Date(startDate) : undefined;
    };

    const handleStartDateChange = (date: Date | null) => {
        if(date != null)
        {
            setValue('start_date', format(date, 'yyyy-MM-dd'));
            CheckFormValidity();
        }
    };

    const CheckFormValidity = () =>{
        let valid = Boolean(watch('start_date') && watch('form_order_number') && watch('terms_value'));

        //console.log(valid)

        if(valid)
        {
            canSave(true);
            
            if(onChange != undefined)
            {
                onChange(new SalesOrderSelectorForm(
                {
                    customer_id: customer_id,
                    form_order_header_id: watch('form_order_header_id'),
                    form_order_number: watch('form_order_number'),
                    terms_value: watch('terms_value'),
                    start_date: watch('start_date'),
                    price: watch('price'),
                    quantity: 1,
                    tax: 0,
                }));
            }
        }
        else
        {
            canSave(false);
        }
    }
    
    return (
        <div>
            <Grid
                templateColumns="repeat(2, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto">
                <Stack gap="0" maxW="md" w="250px">
                    <Field.Root invalid={IsDirty('form_order_header_id')} required={true} disabled>
                        <Field.Label><Field.RequiredIndicator /> Order</Field.Label>
                        <Input {...register('form_order_number')} placeholder="Search Orders..."/>
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="0" maxW="md">
                    <Field.Root invalid={IsDirty('form_order_header_id')} required={true} disabled>
                        <Field.Label>&nbsp;</Field.Label>
                        <Button type="button" colorPalette="blue" onClick={() => handleSearchClick()}><FaSearch /></Button>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={2}>
                    <Field.Root invalid={IsDirty('terms_value')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Terms</Field.Label>
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
                </GridItem>

                <GridItem colSpan={1}>
                    <Field.Root invalid={IsDirty('start_date')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Start Date</Field.Label>
                        <DatePicker 
                            selected={getStartDate()}
                            onChange={handleStartDateChange}
                            dateFormat="MM/dd/yyyy"
                            placeholderText="Select date"
                            
                        />
                    </Field.Root>
                </GridItem>
            </Grid>

            <Dialog.Root open={isDialogOpen} onOpenChange={(details) => setDialogOpen(details.open)} role="alertdialog">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                        <Dialog.Content maxW="1200px" maxH="100vh" >
                            <Dialog.Header>
                                <Dialog.Title>Search Customer Orders</Dialog.Title>
                            </Dialog.Header>
                            <Dialog.Body>
                                <div style={{height: "605px"}}>
                                    <Grid
                                        templateColumns="repeat(4, 2fr)"
                                        gap={6}
                                        display="grid"
                                        width="100%"
                                        p="auto"
                                        m="auto"
                                        >
                                        <GridItem colSpan={4}>
                                            <SalesOrdersSelectListComponent customer_id={customer_id} onSelect={(order: any) => chooseOrder(order) }/>
                                        </GridItem>
                                    </Grid>
                                </div>
                            </Dialog.Body>
                            <Dialog.Footer>
                                <div style={{height: "20px"}}>

                                </div>
                                <Dialog.CloseTrigger asChild>
                                    <CloseButton size="sm" />
                                </Dialog.CloseTrigger>
                            </Dialog.Footer>
                        </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>
        </div>
    );
}

export default SalesOrderSelectorComponent;