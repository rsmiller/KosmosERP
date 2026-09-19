"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
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
  Textarea,
  Tabs,
  Button,
  Dialog,
  Portal,
  CloseButton,
  Spinner,
} from '@chakra-ui/react';

import { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { ProductEditCommand, ProductDto, ProductDeleteCommand } from '@/models/product-models';
import { productService } from '@/services/product-service';
import ProductCategoryCombobox, { ProductCategoryComboboxRef } from '@/components/product-category-combobox';
import { AgGridReact } from 'ag-grid-react';
import { ColDef, ModuleRegistry, AllCommunityModule, GridApi, GridReadyEvent } from 'ag-grid-community';
import { BaseBOMData, BOMCreateCommand, BOMDeleteCommand, BOMEditCommand, BOMFindCommand, BOMListDto } from '@/models/bom-models';
import { bomService } from '@/services/bom-service';
import AddBOMItemDialog, { AddBomItemDialogRef } from '@/components/dialogs/add-bom-item';
import VendorCombobox, { VendorComboboxRef } from '@/components/vendor-combobox';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditProductPage() {
    const auth = useAuth();
    const params = useParams();
    const router = useRouter();

    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);
    const [hasDeletePermission, setHasDeletePermission] = useState(false);
    const [product, setProduct] = useState<ProductDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    const [rowBOMData, setRowBOMData] = useState<BaseBOMData[]>([]);
    const [colBOMDefs, setColBOMDefs] = useState<ColDef<BaseBOMData>[]>([
        { field: "product_name", headerName: "Product Name", rowDrag: true},
        { field: "quantity", headerName: "Quantity",
            editable: true,
            cellEditor: 'agNumberCellEditor',
            cellEditorParams: {
                min: 1
            } 
        },
        { field: "instructions", headerName: "Description",
            editable: true,
            cellEditor: 'agTextCellEditor'
        },
        { field: "id", headerName: "Actions",
            cellRenderer: (props: any) => {
            return ( 
                <div>
                    <Button type="button" colorPalette="red" onClick={() => handleDeleteBOMClick(props.value)}>Delete</Button>
                </div>
            );
      }
        }
    ]);

    const categoryComboboxRef = useRef<ProductCategoryComboboxRef>(null);
    const addBOMComboboxRef = useRef<AddBomItemDialogRef>(null);
    const vendorComboboxRef = useRef<VendorComboboxRef>(null);
    const hasInitialized = useRef(false);

    
    const [openBOMItemDialog, setOpenBOMItemDialog] = useState(false);

    const [gridApi, setGridApi] = useState<GridApi | null>(null);

    const onGridReady = (params: GridReadyEvent) => setGridApi(params.api);

    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [isWorking, setIsWorking] = useState(false);
    const [deleteBomId, setdDeleteBomId] = useState<number | null>(null);

    const {
        register,
        handleSubmit,
        formState: { errors, isValid },
        setValue,
        watch,
        control,
    } = useForm<ProductEditCommand>();

    const loadProduct = async () => {
        try {
            setLoading(true);
            const productId = String(params.id);
            
            if (productId == "") {
                setError('Invalid product ID');
                return;
            }

            const response = await productService.getByGuid(productId, auth.token || "");
            if (response.success && response.data) {
                setProduct(response.data);
                //console.log(response)
                // Set form values with loaded data
                setValue('id', response.data.id);
                setValue('vendor_id', response.data.vendor_id);
                setValue('product_class', response.data.product_class || '');
                setValue('category', response.data.category || '');
                setValue('identifier1', response.data.identifier1 || '');
                setValue('identifier2', response.data.identifier2 || '');
                setValue('identifier3', response.data.identifier3 || '');
                setValue('product_name', response.data.product_name || '');
                setValue('internal_description', response.data.internal_description || '');
                setValue('external_description', response.data.external_description || '');
                setValue('required_stock_level', response.data.required_stock_level);
                setValue('required_reorder_level', response.data.required_reorder_level);
                setValue('required_min_order', response.data.required_min_order);
                setValue('our_cost', response.data.our_cost);
                setValue('unit_cost', response.data.unit_cost);
                setValue('sales_price', response.data.sales_price);
                setValue('list_price', response.data.list_price);
                setValue('rfid_id', response.data.rfid_id || '');
                setValue('is_taxable', response.data.is_taxable);
                setValue('is_stock', response.data.is_stock);
                setValue('is_material', response.data.is_material);
                setValue('is_rental_item', response.data.is_rental_item);
                setValue('is_sales_item', response.data.is_sales_item);
                setValue('is_labor', response.data.is_labor);
                setValue('is_shippable', response.data.is_shippable);
                setValue('is_retired', response.data.is_retired);
                
                var bomFindCommand = new BOMFindCommand();
                bomFindCommand.parent_product_id = response.data.id;
                //console.log(bomFindCommand);
                
                await bomService.find(bomFindCommand, auth.token || "").then( (bom_response) =>
                {
                    setRowBOMData([]);

                    if(bom_response.success && bom_response.data)
                    {
                        //console.log(bom_response)
                        setRowBOMData(bom_response.data);
                    }
                });
            } else {
                setError('Failed to load product');
            }
        } catch (err) {
            console.error('Error loading product:', err);
            setError('Error loading product');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if(auth.authenticated == false) return;

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        // Check permission
        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ProductModule,
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
          ERPModules.ProductModule,
          ERPModulePermission.Edit,
          realmRoles
        );
        setHasEditPermission(canEdit);

        // Check Delete permission
        const canDelete = permissionsService.HasPermission(
          ERPModules.ProductModule,
          ERPModulePermission.Delete,
          realmRoles
        );
        setHasDeletePermission(canDelete);

        loadProduct();
    }, [params.id, setValue, auth.authenticated]);

    const handleDeleteClick = async () => {
        let command = new ProductDeleteCommand();
        command.id = product?.id;

        try
        {
            await productService.delete(command, auth.token || "").then((response) => {
                if(response.success)
                {
                    router.push("/erp/products/");
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

    const handleVendorSelect = (value: any) => {
        setValue('vendor_id', value?.id || 0);
        CheckFormValidity();
    }

    const handleSaveClick = async () => {
        setSuccessSaved(false);

        let command = new ProductEditCommand();
        command.id = product?.id;
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
            await productService.update(command, auth.token || "").then((response) =>
            {
                if(response.success)
                {
                    setSuccessSaved(true);
                    setFailedSaved(false);
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
        if(categoryComboboxRef.current)
        {
            const categoryValid = categoryComboboxRef.current?.isValid();
            const hasRequiredFields = Boolean(watch('product_name') && watch('product_class') 
                                                && watch('internal_description') && watch('identifier1')
                                                && watch('required_stock_level') && watch('unit_cost') 
                                                && watch('our_cost') && watch('sales_price'));

            const allValid = hasRequiredFields && categoryValid && isValid;

            //console.log('categoryValid: ', categoryValid);
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

    const handleNewBOMClick = () => {
        setOpenBOMItemDialog(!openBOMItemDialog);
    }

    const handleBOMItemSelect = async (item: BOMCreateCommand) =>
    {
        if(item != null)
        {
            item.order_number = rowBOMData.length + 1;
            item.parent_product_id = product?.id;

            try
            {
                await bomService.create(item, auth.token || "").then( (response) =>
                {
                    if(response.success && response.data && response.data !== undefined)
                    {
                        let data = response.data;
                        
                        setRowBOMData(prevData => [...prevData, data]);
                    }
                    else
                    {
                        console.error(response.exception);
                    }
                });
            }
            catch(e)
            {
                console.error(e);
            }
        }
        
        setOpenBOMItemDialog(false);
    }


    const handleEditBOMClick = (id: any) => {

    }


    const handleDeleteBOMClick = (id: any) => {
        setdDeleteBomId(id);
        setIsDeleteDialogOpen(true);
    }

    const doBOMDelete = async () => 
    {
        if(deleteBomId)
        {
            let command = new BOMDeleteCommand();
            command.id = deleteBomId;

            try
            {
                await bomService.delete(command, auth.token || "").then( (response) =>
                {
                    if(response.success && response.data && response.data !== undefined)
                    {
                        console.log(response);
                        
                        setRowBOMData(prevData => prevData.filter(item => item.id !== deleteBomId));
                    }

                    setdDeleteBomId(null);
                    setIsDeleteDialogOpen(false);

                });
            }
            catch(e)
            {
                console.error(e);
                setdDeleteBomId(null);
                setIsDeleteDialogOpen(false);
            }
        }
        else
        {
            setdDeleteBomId(null);
            setIsDeleteDialogOpen(false);
        }
    }

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }
    if (loading) {
        return <div>Loading product...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!product) {
        return <div>Product not found</div>;
    }

    const defaultColBOMDef: ColDef = {
        flex: 1,
        filter: true,
        sortable: true,
    };

    const onRowDragEnd = (shit: any) => 
    {
        let reordered: any[] = [];
        if(gridApi)
        {
            gridApi.forEachNodeAfterFilterAndSort(node => {
                reordered.push({
                    ...node.data
                });
            });
            
        }

        for(let i=0; i<reordered.length; i++)
        {
            var found = rowBOMData.filter(m => m.id == reordered[i].id);
            if(found != null && found.length> 0)
            {
                found[0].order_number = i + 1;
                found[0].isDirty = true;
            }
        }
    }

    const handleCellEdit = async (event: any) => {
        //console.log('Cell edited:', {
        //    colId: event.colDef.field,
        //    newValue: event.newValue,
        //    oldValue: event.oldValue,
        //    rowIndex: event.rowIndex,
        //    data: event.data,
        //});

        if(event != null && event.data != null)
        {
            let command = new BOMEditCommand();
            command.id = event.data.id;
            command.quantity = event.data.quantity; 
            command.instructions = event.data.instructions; 

            try
            {
                await bomService.update(command, auth.token || "").then( (response) => 
                {
                    if(!response.success)
                    {
                        console.error(response.exception);
                    }
                });
            }
            catch(e)
            {
                console.error(e);
            }   
        }
    };

    return (
        <Tabs.Root lazyMount unmountOnExit defaultValue="details">
            <Tabs.List>
            <Tabs.Trigger value="details">Details</Tabs.Trigger>
            <Tabs.Trigger value="bom">BOM</Tabs.Trigger>
            </Tabs.List>
            <Tabs.Content value="details">
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
                            <h1>Edit Product</h1>
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
                                <Field.Root >
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
                                canSave={!saveable || !hasEditPermission} 
                                canDelete={hasDeletePermission}
                                onSave={handleSaveClick} 
                                onDelete={handleDeleteClick} 
                                successSaved={successSaved}
                                failedSaved={failedSaved}
                            />
                        </GridItem>
                    </Grid>
                </form>
            </Tabs.Content> 
            <Tabs.Content value="bom">
                <div style={{ width: "100%", height: "500px" }}>
                    <h1>BOM List</h1>
                    <Button type="submit" colorPalette="blue" onClick={handleNewBOMClick}>New BOM Item</Button>
                    <AgGridReact
                        rowData={rowBOMData}
                        columnDefs={colBOMDefs}
                        defaultColDef={defaultColBOMDef}
                        rowDragManaged={true}
                        onRowDragEnd={onRowDragEnd}
                        onGridReady={onGridReady}
                        onCellValueChanged={handleCellEdit}
                    />
                </div>
                <AddBOMItemDialog
                    openDialog={openBOMItemDialog}
                    onChange={handleBOMItemSelect}
                    ref={addBOMComboboxRef}
                    control={control}
                    name="ship_to_address_id"
                    error={undefined}
                />
                <Dialog.Root open={isDeleteDialogOpen} onOpenChange={(details) => setIsDeleteDialogOpen(details.open)} role="alertdialog">
                    <Portal>
                        <Dialog.Backdrop />
                        <Dialog.Positioner>
                        <Dialog.Content>
                            <Dialog.Header>
                            <Dialog.Title>Are you sure?</Dialog.Title>
                            </Dialog.Header>
                            <Dialog.Body>
                            <p>
                                Are you sure you want to delete this BOM Item?
                            </p>
                            </Dialog.Body>
                            <Dialog.Footer>
                            <Dialog.ActionTrigger asChild>
                                <Button variant="outline" disabled={isWorking}>Cancel</Button>
                            </Dialog.ActionTrigger>
                            <Button colorPalette="red" onClick={() => doBOMDelete()} disabled={isWorking} ><Spinner hidden={!isWorking} /> Delete</Button>
                            </Dialog.Footer>
                            <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                            </Dialog.CloseTrigger>
                        </Dialog.Content>
                        </Dialog.Positioner>
                    </Portal>
                </Dialog.Root>
            </Tabs.Content>
        </Tabs.Root>
    );
}

export default EditProductPage;