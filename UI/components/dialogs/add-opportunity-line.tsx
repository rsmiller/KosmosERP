
import '@/app/styles/add-order-line-dialog.css';

import { OrderLineCreateCommand } from "@/models/sales-order-models";
import { Button, CloseButton, Field, Grid, GridItem, Input, NumberInput, Portal, Stack, Textarea } from "@chakra-ui/react";
import { Dialog } from "@chakra-ui/react/dialog";
import { forwardRef, useEffect, useRef, useState } from "react";
import { Control, FieldError, useForm } from "react-hook-form";
import ProductCombobox, { ProductComboboxRef } from '../product-combobox';
import { OpportunityLineCreateCommand } from '@/models/opportunity-models';


export class AddOpportunityLineDialogParams
{
    openDialog: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
}

export interface AddOpportunityLineDialogRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const AddOpportunityLineDialog = forwardRef<AddOpportunityLineDialogRef, AddOpportunityLineDialogParams>(
    ({onChange, control, name, error, openDialog}, ref) => {
    
    const [isValid, setIsValid] = useState(false);
    
    const productComboboxRef = useRef<ProductComboboxRef>(null);
    const [minProductPrice, setMinProductPrice] = useState<number>(1);

    const { register, handleSubmit, watch, setValue, reset, formState: { errors } } = useForm<OpportunityLineCreateCommand>();

    useEffect(() => {
        
    });

    const doCancel = () => {
        reset();
        setIsValid(false);
        if(onChange) {
            onChange(null);
        }
    }

    const doAdd = (data: OrderLineCreateCommand) => {
        if(onChange) {
            onChange(data);
        }

        setValue('product_id', 0);
        setValue('description', '');
        setValue('quantity', 0);
        setValue('unit_price', 0);
    }
    
    const handleProductSelect = (inputValue: any) =>
    {
        if(inputValue == null || inputValue == undefined)
        {
            setValue('product_id', 0);
        }
        else
        {
            setValue('product_id', inputValue.id);
            setMinProductPrice(inputValue.sales_price);
            setValue('unit_price', inputValue.list_price);
            setValue('product_name', inputValue.product_name);

            if(watch('description') == '')
            {
                setValue('description', inputValue.external_description);
            }

            checkFormValidity();
        }
    }

    const checkFormValidity = () =>
    {
        const hasRequiredFields = Boolean(watch('description') && watch('quantity') && watch('unit_price'));

        const isAllValid = watch('product_id') != 0 && watch('product_id') != undefined && hasRequiredFields

        //console.log('hasRequiredFields: ', hasRequiredFields);
        //console.log('isAllValid: ', isAllValid);

        setIsValid(isAllValid);
    }

    return (
        <Dialog.Root size={'xl'} open={openDialog}>
            <Portal>
                <Dialog.Backdrop />
                <Dialog.Positioner>
                <Dialog.Content className='add-order-line-dialog-content'>
                    <Dialog.Header>
                    <Dialog.Title>Add Order Line</Dialog.Title>
                    </Dialog.Header>
                    <Dialog.Body style={{ overflow: 'visible' }}>
                        <form onChange={checkFormValidity}>
                            <Stack direction="column" h="20">
                                <Grid
                                    templateColumns="repeat(5, 2fr)"
                                    gap={4}
                                    display="grid"
                                    width="100%"
                                    p="auto"
                                    m="auto"
                                >
                                    <GridItem colSpan={2}>
                                        <Field.Root>
                                            <Field.Label>Product</Field.Label>
                                            <ProductCombobox 
                                                ref={productComboboxRef}
                                                dbKey={watch('product_id') || 0}
                                                control={control}
                                                name="product_id"
                                                error={errors.product_id}
                                                onChange={handleProductSelect}
                                                onValidationChange={checkFormValidity}
                                            />
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={4}>
                                        <Field.Root>
                                            <Field.Label>Description</Field.Label>
                                            <Textarea {...register('description')}/>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root>
                                            <Field.Label>Quantity</Field.Label>
                                            <NumberInput.Root defaultValue="0" min={1} onValueChange={checkFormValidity}>
                                                <NumberInput.Control />
                                                <NumberInput.Input {...register('quantity')}/>
                                            </NumberInput.Root>
                                        </Field.Root>
                                    </GridItem>
                                    <GridItem colSpan={2}>
                                        <Field.Root>
                                            <Field.Label>Unit Price</Field.Label>
                                            <NumberInput.Root defaultValue="0" min={minProductPrice} onValueChange={checkFormValidity}>
                                                <NumberInput.Control />
                                                <NumberInput.Input {...register('unit_price')}/>
                                            </NumberInput.Root>
                                        </Field.Root>
                                    </GridItem>
                                </Grid>
                            </Stack>
                        </form>
                    </Dialog.Body>
                    <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline" onClick={doCancel}>Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button onClick={handleSubmit(doAdd)} disabled={!isValid}>Add Line</Button>
                    </Dialog.Footer>
                    <Dialog.CloseTrigger asChild>
                    <CloseButton size="sm" onClick={doCancel}/>
                    </Dialog.CloseTrigger>
                </Dialog.Content>
                </Dialog.Positioner>
            </Portal>
        </Dialog.Root>
    );
});

export default AddOpportunityLineDialog;