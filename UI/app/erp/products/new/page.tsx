"use client"

import '../../../styles/date-picker.css';
import '../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
  Checkbox,
  NumberInput,
  Textarea
} from '@chakra-ui/react';

import { useEffect, useState, useRef } from "react";
import { useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { ProductCreateCommand, ProductDto } from '@/models/product-models';
import { productService } from '@/services/product-service';
import ProductCategoryCombobox, { ProductCategoryComboboxRef } from '@/components/product-category-combobox';
import VendorCombobox, { VendorComboboxRef } from '@/components/vendor-combobox';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';


function NewProductPage() {
    const auth = useAuth();
    const router = useRouter();

    const [hasAccess, setHasAccess] = useState(true);
    const [hasWritePermission, setHasWritePermission] = useState(false);
    const [product, setProduct] = useState<ProductDto | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    
    const categoryComboboxRef = useRef<ProductCategoryComboboxRef>(null);
    const vendorComboboxRef = useRef<VendorComboboxRef>(null);
    const hasInitialized = useRef(false);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
    } = useForm<ProductCreateCommand>();

    // Load product data on component mount
    useEffect(() => {
        if (auth.authenticated == false) return;

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        // Check permission
        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ProductModule,
          ERPModulePermission.Write,
          realmRoles
        );

        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }
        setHasWritePermission(true);

        const loadProduct = async () => {
            try {
                setLoading(false);
                setProduct(null);
                // Set default values for new product
                setValue('required_stock_level', 0);
                setValue('required_reorder_level', 0);
                setValue('required_min_order', 0);
                setValue('our_cost', 0);
                setValue('unit_cost', 0);
                setValue('sales_price', 0);
                setValue('list_price', 0);
                setValue('is_taxable', false);
                setValue('is_stock', false);
                setValue('is_material', false);
                setValue('is_rental_item', false);
                setValue('is_sales_item', false);
                setValue('is_labor', false);
                setValue('is_shippable', false);
                setValue('is_retired', false);
            } catch (err) {
                console.error('Error initializing product:', err);
                setError('Error initializing product');
            } finally {
                setLoading(false);
            }
        };

        loadProduct();
    }, [setValue]);

    const handleDeleteClick = async () => {
        // For new product, delete is not applicable
        router.push("/erp/products/");
    };

    const handleSaveClick = async () => {
        setSuccessSaved(false);

        let command = new ProductCreateCommand();
        command.vendor_id = watch('vendor_id');
        command.product_class = watch('product_class');
        command.category = watch('category');
        command.identifier1 = watch('identifier1');
        command.identifier2 = watch('identifier2');
        command.identifier3 = watch('identifier3');
        command.product_name = watch('product_name');
        command.internal_description = watch('internal_description');
        command.external_description = watch('external_description');
        command.required_stock_level = Number(watch('required_stock_level'));
        command.required_reorder_level = Number(watch('required_reorder_level'));
        command.required_min_order = Number(watch('required_min_order'));
        command.our_cost = Number(watch('our_cost'));
        command.unit_cost = Number(watch('unit_cost'));
        command.sales_price = Number(watch('sales_price'));
        command.list_price = Number(watch('list_price'));
        command.rfid_id = watch('rfid_id');
        command.is_taxable = Boolean(watch('is_taxable'));
        command.is_stock = Boolean(watch('is_stock'));
        command.is_material = Boolean(watch('is_material'));
        command.is_rental_item = Boolean(watch('is_rental_item'));
        command.is_sales_item = Boolean(watch('is_sales_item'));
        command.is_labor = Boolean(watch('is_labor'));
        command.is_shippable = Boolean(watch('is_shippable'));
        command.is_retired = Boolean(watch('is_retired'));

        //console.log(command);
        //return

        try
        {
            await productService.create(command, auth.token || "").then((response) =>
            {
                if(response.success)
                {
                    setSuccessSaved(true);
                    setFailedSaved(false);
                    // Route to purchase orders on successful save
                    router.push("/erp/products/edit/" + response.data?.guid);
                }
                else
                {
                    setSuccessSaved(false);
                    setFailedSaved(true);
                }
            });
        }
        catch(e)
        {
            setSuccessSaved(false);
            setFailedSaved(true);
        }
    };

    const CheckFormValidity = () => {
        if(categoryComboboxRef.current && vendorComboboxRef.current)
        {
            const categoryValid = categoryComboboxRef.current?.isValid() || false;
            const vendorValid = vendorComboboxRef.current?.isValid() || false;

            const hasRequiredFields = Boolean(watch('product_name') && watch('product_class') 
                                                && watch('internal_description') && watch('identifier1')
                                                && watch('required_stock_level') && watch('unit_cost') 
                                                && watch('our_cost') && watch('sales_price'));

            const allValid = hasRequiredFields && categoryValid && vendorValid&& isValid;

            //console.log('categoryValid: ', categoryValid);
            //console.log('vendorValid: ', vendorValid);
            //console.log('hasRequiredFields: ', hasRequiredFields);
            //console.log('allValid: ', allValid);

            canSave(!allValid);
        }
        else
        {
            canSave(false);
        }


    };

    const IsDirty = (formName: any) => {
        if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
        {
            return true;
        }
        return false;
    }

    const handleCategorySelect = (value: any) => {
        //console.log(value)
        setValue('category', value?.key);
        CheckFormValidity();
    }

    const handleVendorSelect = (value: any) => {
        setValue('vendor_id', value?.id || 0);
        CheckFormValidity();
    }


    if (loading) {
        return <div>Loading product...</div>;
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
                templateColumns="repeat(5, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={6}>
                    <h1>New Product</h1>
                </GridItem>
                
                {/* Basic Information */}
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('product_name')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Product Name</Field.Label>
                        <Input 
                            {...register('product_name')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <GridItem>
                    <Field.Root invalid={!!errors.vendor_id} required={true}>
                    <Field.Label><Field.RequiredIndicator /> Vendor</Field.Label>
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
                </Field.Root>
                </GridItem>
                <GridItem colSpan={4}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('product_class')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Product Class</Field.Label>
                        <Input 
                            {...register('product_class')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('category')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Category</Field.Label>
                        <ProductCategoryCombobox 
                            ref={categoryComboboxRef}
                            dbKey={watch('category') ? [watch('category') as string] : []}
                            onChange={handleCategorySelect}
                            control={control}
                            name="category"
                            error={errors.category}
                            onValidationChange={CheckFormValidity}/>
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={4}></GridItem>

                {/* Identifiers */}
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('identifier1')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Identifier 1</Field.Label>
                        <Input 
                            {...register('identifier1')}
                        />
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Field.Label>Identifier 2</Field.Label>
                        <Input 
                            {...register('identifier2')}
                        />
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Field.Label>Identifier 3</Field.Label>
                        <Input 
                            {...register('identifier3')}
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={3}></GridItem>
                <GridItem colSpan={4}>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root invalid={IsDirty('internal_description')} required={true}>
                            <Field.Label><Field.RequiredIndicator /> Internal Description</Field.Label>
                            <Textarea 
                                {...register('internal_description')}
                            />
                            <Field.ErrorText>This field is required</Field.ErrorText>
                        </Field.Root>
                    </Stack>
                </GridItem>
                <GridItem colSpan={2}></GridItem>
                <GridItem colSpan={4}>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>External Description</Field.Label>
                            <Textarea 
                                {...register('external_description')}
                            />
                        </Field.Root>
                    </Stack>
                </GridItem>
                <GridItem colSpan={2}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Field.Label>RFID ID</Field.Label>
                        <Input 
                            {...register('rfid_id')}
                        />
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('required_stock_level')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Required Stock Level</Field.Label>
                        <NumberInput.Root defaultValue="0" min={0}>
                            <NumberInput.Control />
                            <NumberInput.Input {...register('required_stock_level')}/>
                        </NumberInput.Root>
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('required_reorder_level')}>
                        <Field.Label>Required Reorder Level</Field.Label>
                        <NumberInput.Root defaultValue="0" min={0}>
                            <NumberInput.Control />
                            <NumberInput.Input {...register('required_reorder_level')}/>
                        </NumberInput.Root>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('required_min_order')}>
                        <Field.Label>Required Min Order</Field.Label>
                        <NumberInput.Root defaultValue="0" min={0}>
                            <NumberInput.Control />
                            <NumberInput.Input {...register('required_min_order')}/>
                        </NumberInput.Root>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={3}></GridItem>

                {/* Costs and Prices */}
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('our_cost')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Our Cost</Field.Label>
                        <NumberInput.Root defaultValue="0" min={0}>
                            <NumberInput.Control />
                            <NumberInput.Input {...register('our_cost')}/>
                        </NumberInput.Root>
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('unit_cost')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Unit Cost</Field.Label>
                        <NumberInput.Root defaultValue="0" min={0}>
                            <NumberInput.Control />
                            <NumberInput.Input {...register('unit_cost')}/>
                        </NumberInput.Root>
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('sales_price')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> Sales Price</Field.Label>
                        <NumberInput.Root defaultValue="0" min={0}>
                            <NumberInput.Control />
                            <NumberInput.Input {...register('sales_price')}/>
                        </NumberInput.Root>
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={3}></GridItem>

                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root invalid={IsDirty('list_price')} required={true}>
                        <Field.Label><Field.RequiredIndicator /> List Price</Field.Label>
                        <NumberInput.Root defaultValue="0" min={0}>
                            <NumberInput.Control />
                            <NumberInput.Input {...register('list_price')}/>
                        </NumberInput.Root>
                        <Field.ErrorText>This field is required</Field.ErrorText>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>

                {/* Boolean Flags */}
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root checked={watch('is_taxable')} value={"1"}>
                            <Checkbox.HiddenInput {...register('is_taxable')} />
                            <Checkbox.Control />
                            <Checkbox.Label>Taxable</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root checked={watch('is_stock')} value={"1"}>
                            <Checkbox.HiddenInput {...register('is_stock')} />
                            <Checkbox.Control />
                            <Checkbox.Label>Stock Item</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root checked={watch('is_material')} value={"1"}>
                            <Checkbox.HiddenInput {...register('is_material')} />
                            <Checkbox.Control />
                            <Checkbox.Label>Material</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={3}></GridItem>

                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root checked={watch('is_rental_item')} value={"1"}>
                            <Checkbox.HiddenInput {...register('is_rental_item')} />
                            <Checkbox.Control />
                            <Checkbox.Label>Rental Item</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root checked={watch('is_sales_item')} value={"1"}>
                            <Checkbox.HiddenInput {...register('is_sales_item')} />
                            <Checkbox.Control />
                            <Checkbox.Label>Sales Item</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root checked={watch('is_labor')} value={"1"}>
                            <Checkbox.HiddenInput {...register('is_labor')} />
                            <Checkbox.Control />
                            <Checkbox.Label>Labor</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={3}></GridItem>

                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root checked={watch('is_shippable')} value={"1"}>
                            <Checkbox.HiddenInput {...register('is_shippable')} />
                            <Checkbox.Control />
                            <Checkbox.Label>Shippable</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <Stack gap="4" align="flex-start" maxW="md">
                    <Field.Root>
                        <Checkbox.Root checked={watch('is_retired')} value={"1"}>
                            <Checkbox.HiddenInput {...register('is_retired')} />
                            <Checkbox.Control />
                            <Checkbox.Label>Retired</Checkbox.Label>
                        </Checkbox.Root>
                    </Field.Root>
                </Stack>
                <GridItem colSpan={5}></GridItem>

                <GridItem colSpan={6}>
                    <PageActionsComponent 
                        canSave={saveable || !hasWritePermission} 
                        onSave={handleSaveClick} 
                        onDelete={handleDeleteClick} 
                        successSaved={successSaved}
                        failedSaved={failedSaved}
                    />
                </GridItem>
            </Grid>
        </form>
    );
}

export default NewProductPage;