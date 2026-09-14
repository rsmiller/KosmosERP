import { AddressCreateCommand } from "@/models/address-models";
import { BOMCreateCommand } from "@/models/bom-models";
import { Button, CloseButton, Dialog, Field, Grid, GridItem, NumberInput, Portal, Textarea } from "@chakra-ui/react";
import { forwardRef, useRef, useState } from "react";
import { Control, FieldError, useForm } from "react-hook-form";
import ProductCombobox, { ProductComboboxRef } from "../product-combobox";

export class AddBomItemDialogParams
{
    openDialog: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
}

export interface AddBomItemDialogRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const AddBOMItemDialog = forwardRef<AddBomItemDialogRef, AddBomItemDialogParams>(
    ({onChange, control, name, error, openDialog}, ref) => {
    
    const [isValid, setIsValid] = useState(false);
    
    const productComboboxRef = useRef<ProductComboboxRef>(null);

    
    const { register, handleSubmit, watch, setValue, reset, formState: { errors } } = useForm<BOMCreateCommand>();
    
    const watchedValues = watch();

    const doCancel = () => {
        setIsValid(false);
        if(onChange) {
            onChange(null);
        }
    }

    const doAdd = () => {
        let data = new BOMCreateCommand();
        data.product_id = watchedValues.product_id;
        data.quantity = watchedValues.quantity;
        data.instructions = watchedValues.instructions
        
        if(onChange) {
            onChange(data);
        }
    }

    const handleProductSelect = (inputValue: any) =>
    {
        //console.log(inputValue)

        if(inputValue == null || inputValue == undefined)
        {
            setValue('product_id', undefined);
        }
        else
        {
            setValue('product_id', inputValue.id);
        }
    }

    const checkFormValidity = () => {
        const hasRequiredFields = Boolean(
            watchedValues.product_id && 
            watchedValues.quantity && 
            watchedValues.instructions != ''

        );

        //console.log('product_id: ', watchedValues.product_id);
        //console.log('quantity: ', watchedValues.quantity);
        //console.log('description: ', watchedValues.description);
        //console.log(hasRequiredFields);

        setIsValid(hasRequiredFields);
    };

    return (
        <Dialog.Root size={'sm'} open={openDialog}>
            <Portal>
                <Dialog.Backdrop />
                <Dialog.Positioner>
                <Dialog.Content>
                    <Dialog.Header>
                    <Dialog.Title>Add Address</Dialog.Title>
                    </Dialog.Header>
                    <Dialog.Body style={{ overflow: 'visible' }}>
                        <Grid 
                            templateColumns="repeat(5, 2fr)"
                            gap={4}
                            display="grid"
                            width="100%"
                            p="auto"
                            m="auto">
                            <GridItem colSpan={5}>
                                <Field.Root>
                                    <Field.Label><Field.RequiredIndicator /> Product</Field.Label>
                                    
                                    <ProductCombobox
                                        dbKey={null}
                                        ref={productComboboxRef}
                                        control={control}
                                        name="product_id"
                                        error={errors.product_id}
                                        onChange={handleProductSelect}
                                        onValidationChange={checkFormValidity}
                                    />
                                </Field.Root>
                            </GridItem>
                            <GridItem colSpan={5}>
                                <Field.Root>
                                    <Field.Label><Field.RequiredIndicator /> Description</Field.Label>
                                    <Textarea {...register('instructions')} placeholder="Details about this item"/>
                                </Field.Root>
                            </GridItem>
                            <GridItem colSpan={3}>
                                <Field.Root>
                                    <Field.Label><Field.RequiredIndicator /> Quantity</Field.Label>
                                    <NumberInput.Root defaultValue="1" min={1} onValueChange={checkFormValidity}>
                                        <NumberInput.Control />
                                        <NumberInput.Input {...register('quantity')}/>
                                    </NumberInput.Root>
                                </Field.Root>
                            </GridItem>
                        </Grid>
                    </Dialog.Body>
                    <Dialog.Footer>
                        <Dialog.ActionTrigger asChild>
                            <Button variant="outline" onClick={doCancel}>Cancel</Button>
                        </Dialog.ActionTrigger>
                        <Button onClick={doAdd} disabled={!isValid}>Save</Button>
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

export default AddBOMItemDialog;