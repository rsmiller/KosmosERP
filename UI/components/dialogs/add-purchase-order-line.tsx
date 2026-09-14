'use client'

import { Button, CloseButton, Portal, Input, Textarea, Grid, GridItem } from "@chakra-ui/react";
import { Dialog } from "@chakra-ui/react/dialog";
import { Field } from "@chakra-ui/react/field";
import { NumberInput } from "@chakra-ui/react/number-input";
import { forwardRef, useImperativeHandle, useState, useEffect, useRef } from "react";
import { Control, FieldError, useForm } from "react-hook-form";
import { PurchaseOrderLineCreateCommand } from "@/models/purchase-order-models";
import ProductCombobox, { ProductComboboxRef } from "../product-combobox";

export class AddPurchaseOrderLineDialogParams
{
    openDialog: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
}

export interface AddPurchaseOrderLineDialogRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const AddPurchaseOrderLineDialog = forwardRef<AddPurchaseOrderLineDialogRef, AddPurchaseOrderLineDialogParams>(
    ({onChange, control, name, error, openDialog}, ref) => {

    const [isValid, setIsValid] = useState(false);
    
    const productComboboxRef = useRef<ProductComboboxRef>(null);

    const { register, handleSubmit, watch, setValue, reset, formState: { errors } } = useForm<PurchaseOrderLineCreateCommand>({
        defaultValues: {
            product_id: undefined,
            product_name: '',
            quantity: 0,
            description: '',
            unit_price: 0,
            tax: 0,
            is_taxable: false
        }
    });

    const watchedValues = watch();

    // Check form validity
    const checkFormValidity = () => {
        const hasRequiredFields = Boolean(
            watchedValues.product_id && 
            watchedValues.quantity && 
            watchedValues.description && 
            watchedValues.unit_price !== undefined
        );

        //console.log('product_id: ', watchedValues.product_id);
        //console.log('quantity: ', watchedValues.quantity);
        //console.log('description: ', watchedValues.description);
        //console.log('unit_price: ', watchedValues.unit_price);
        //console.log(hasRequiredFields);

        setIsValid(hasRequiredFields);
    };

    // Expose methods to parent component
    useImperativeHandle(ref, () => ({
        isValid: () => isValid,
        getValue: () => watchedValues,
        clear: () => {
            reset();
            setIsValid(false);
        }
    }), [isValid, watchedValues, reset]);

    const doCancel = () => {
        reset();
        setIsValid(false);
        if(onChange) {
            onChange(null);
        }
    }

    const doAdd = (data: PurchaseOrderLineCreateCommand) => {
        if(onChange) {
            onChange(data);
        }
    }

    // Check validity whenever form values change
    useEffect(() => {
        checkFormValidity();
    }, [watchedValues]);

    const handleProductSelect = (inputValue: any) =>
    {
        //console.log(inputValue)

        if(inputValue == null || inputValue == undefined)
        {
            setValue('product_id', undefined);
            setValue('product_name', '');
        }
        else
        {
            setValue('product_id', inputValue.id);
            setValue('product_name', inputValue.product_name);
        }
    }

    return (
        <Dialog.Root size={'lg'} open={openDialog}>
            <Portal>
                <Dialog.Backdrop />
                <Dialog.Positioner>
                <Dialog.Content>
                    <Dialog.Header>
                    <Dialog.Title>Add Purchase Order Line</Dialog.Title>
                    </Dialog.Header>
                    <Dialog.Body style={{ overflow: 'visible' }}>
                        <form onSubmit={handleSubmit(doAdd)} onChange={checkFormValidity}>
                            <Grid
                                templateColumns="repeat(5, 2fr)"
                                gap={2}
                                display="grid"
                                width="100%"
                                p="auto"
                                m="auto"
                            >
                                <GridItem colSpan={3}>
                                    <div style={{ position: 'relative', zIndex: 1 }}>
                                        <ProductCombobox 
                                            ref={productComboboxRef}
                                            dbKey={watch('product_id') || 0}
                                            control={control}
                                            name="product_id"
                                            error={errors.product_id}
                                            onChange={handleProductSelect}
                                            onValidationChange={checkFormValidity}
                                        />
                                        <input type="hidden" value={watch('product_name') || ''}></input>
                                    </div>
                                </GridItem>
                                <GridItem colSpan={6}>
                                    <Field.Root invalid={!!errors.description} required={true}>
                                        <Field.Label><Field.RequiredIndicator /> Description</Field.Label>
                                        <Textarea 
                                            {...register('description', { 
                                                required: 'Description is required',
                                                maxLength: { value: 1000, message: 'Description cannot exceed 1000 characters' }
                                            })}
                                            placeholder="Enter product description"
                                            rows={3}
                                        />
                                        <Field.ErrorText>{errors.description?.message}</Field.ErrorText>
                                    </Field.Root>
                                </GridItem>
                                <Field.Root invalid={!!errors.quantity} required={true}>
                                    <Field.Label><Field.RequiredIndicator /> Quantity</Field.Label>
                                    <NumberInput.Root min={1} defaultValue="0">
                                        <NumberInput.Control />
                                        <NumberInput.Input {...register('quantity', { required: 'Quantity is required', min: { value: 1, message: 'Quantity must be at least 1' } })} />
                                    </NumberInput.Root>
                                    <Field.ErrorText>{errors.quantity?.message}</Field.ErrorText>
                                </Field.Root>
                                <Field.Root invalid={!!errors.unit_price} required={true}>
                                    <Field.Label><Field.RequiredIndicator /> Unit Price</Field.Label>
                                    <NumberInput.Root min={0} defaultValue="0">
                                        <NumberInput.Control />
                                        <NumberInput.Input {...register('unit_price', { required: 'Unit price is required', min: { value: 0, message: 'Unit price must be non-negative' } })} />
                                    </NumberInput.Root>
                                    <Field.ErrorText>{errors.unit_price?.message}</Field.ErrorText>
                                </Field.Root>
                                <Field.Root invalid={!!errors.tax}>
                                    <Field.Label>Tax</Field.Label>
                                    <NumberInput.Root min={0} defaultValue="0">
                                        <NumberInput.Control />
                                        <NumberInput.Input {...register('tax', { min: { value: 0, message: 'Tax must be non-negative' } })} />
                                    </NumberInput.Root>
                                    <Field.ErrorText>{errors.tax?.message}</Field.ErrorText>
                                </Field.Root>
                            </Grid>
                        </form>
                    </Dialog.Body>
                    <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline" onClick={doCancel}>Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button onClick={handleSubmit(doAdd)} disabled={!isValid}>Save</Button>
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

export default AddPurchaseOrderLineDialog;