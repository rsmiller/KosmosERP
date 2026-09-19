"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

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
} from '@chakra-ui/react';
import { useEffect, useState, useRef } from "react";
import { useParams } from 'next/navigation';
import { ProductDto } from '@/models/product-models';
import { productService } from '@/services/product-service';
import ProductCategoryCombobox from '@/components/product-category-combobox';
import { AgGridReact } from 'ag-grid-react';
import { ColDef, ModuleRegistry, AllCommunityModule } from 'ag-grid-community';
import { BaseBOMData, BOMFindCommand } from '@/models/bom-models';
import { bomService } from '@/services/bom-service';
import VendorCombobox from '@/components/vendor-combobox';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useRouter } from 'next/navigation';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewProductPage() {
    const auth = useAuth();
    const params = useParams();
    const router = useRouter();

    const [product, setProduct] = useState<ProductDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [hasAccess, setHasAccess] = useState(true);
    const [rowBOMData, setRowBOMData] = useState<BaseBOMData[]>([]);
    const [colBOMDefs] = useState<ColDef<BaseBOMData>[]>([
        { field: "product_name", headerName: "Product Name" },
        { field: "quantity", headerName: "Quantity" },
        { field: "instructions", headerName: "Description" }
    ]);

    const hasInitialized = useRef(false);

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
                
                var bomFindCommand = new BOMFindCommand();
                bomFindCommand.parent_product_id = response.data.id;
                
                await bomService.find(bomFindCommand, auth.token || "").then( (bom_response) =>
                {
                    setRowBOMData([]);

                    if(bom_response.success && bom_response.data)
                    {
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

        loadProduct();
    }, [params.id, auth.authenticated]);

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

    return (
        <Tabs.Root lazyMount unmountOnExit defaultValue="details">
            <Tabs.List>
            <Tabs.Trigger value="details">Details</Tabs.Trigger>
            <Tabs.Trigger value="bom">BOM</Tabs.Trigger>
            </Tabs.List>
            <Tabs.Content value="details">
                <Grid
                    templateColumns="repeat(5, 2fr)"
                    gap={6}
                    display="grid"
                    width="100%"
                    p="auto"
                    m="auto"
                >
                    <GridItem colSpan={6}>
                        <h1>View Product</h1>
                    </GridItem>
                    
                    {/* Basic Information */}
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Product Name</Field.Label>
                            <Input 
                                value={product.product_name || ''}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <GridItem>
                        <Field.Root>
                            <Field.Label>Vendor</Field.Label>
                            <VendorCombobox 
                                dbKey={product.vendor_id || 0}
                                control={undefined}
                                name="vendor_id"
                                error={undefined}
                                onChange={() => {}}
                                onValidationChange={() => {}}
                                disabled={true}
                            />
                        </Field.Root>
                    </GridItem>
                    <GridItem colSpan={4}></GridItem>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Product Class</Field.Label>
                            <Input 
                                value={product.product_class || ''}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Category</Field.Label>
                            <ProductCategoryCombobox 
                                dbKey={product.category ? [product.category as string] : []}
                                onChange={() => {}}
                                control={undefined}
                                name="category"
                                error={undefined}
                                onValidationChange={() => {}}
                                disabled={true}
                            />
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={4}></GridItem>

                    {/* Identifiers */}
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Identifier 1</Field.Label>
                            <Input 
                                value={product.identifier1 || ''}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Identifier 2</Field.Label>
                            <Input 
                                value={product.identifier2 || ''}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Identifier 3</Field.Label>
                            <Input 
                                value={product.identifier3 || ''}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={3}></GridItem>
                    <GridItem colSpan={4}>
                        <Stack gap="4" align="flex-start" maxW="md">
                            <Field.Root>
                                <Field.Label>Internal Description</Field.Label>
                                <Textarea 
                                    value={product.internal_description || ''}
                                    readOnly
                                />
                            </Field.Root>
                        </Stack>
                    </GridItem>
                    <GridItem colSpan={2}></GridItem>
                    <GridItem colSpan={4}>
                        <Stack gap="4" align="flex-start" maxW="md">
                            <Field.Root >
                                <Field.Label>External Description</Field.Label>
                                <Textarea 
                                    value={product.external_description || ''}
                                    readOnly
                                />
                            </Field.Root>
                        </Stack>
                    </GridItem>
                    <GridItem colSpan={2}></GridItem>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>RFID ID</Field.Label>
                            <Input 
                                value={product.rfid_id || ''}
                                readOnly
                            />
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={5}></GridItem>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Required Stock Level</Field.Label>
                            <NumberInput.Root value={String(product.required_stock_level || 0)} min={0}>
                                <NumberInput.Control />
                                <NumberInput.Input readOnly />
                            </NumberInput.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Required Reorder Level</Field.Label>
                            <NumberInput.Root value={String(product.required_reorder_level || 0)} min={0}>
                                <NumberInput.Control />
                                <NumberInput.Input readOnly />
                            </NumberInput.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Required Min Order</Field.Label>
                            <NumberInput.Root value={String(product.required_min_order || 0)} min={0}>
                                <NumberInput.Control />
                                <NumberInput.Input readOnly />
                            </NumberInput.Root>
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={3}></GridItem>

                    {/* Costs and Prices */}
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Our Cost</Field.Label>
                            <NumberInput.Root value={String(product.our_cost || 0)} min={0}>
                                <NumberInput.Control />
                                <NumberInput.Input readOnly />
                            </NumberInput.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Unit Cost</Field.Label>
                            <NumberInput.Root value={String(product.unit_cost || 0)} min={0}>
                                <NumberInput.Control />
                                <NumberInput.Input readOnly />
                            </NumberInput.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>Sales Price</Field.Label>
                            <NumberInput.Root value={String(product.sales_price || 0)} min={0}>
                                <NumberInput.Control />
                                <NumberInput.Input readOnly />
                            </NumberInput.Root>
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={3}></GridItem>

                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Field.Label>List Price</Field.Label>
                            <NumberInput.Root value={String(product.list_price || 0)} min={0}>
                                <NumberInput.Control />
                                <NumberInput.Input readOnly />
                            </NumberInput.Root>
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={5}></GridItem>

                    {/* Boolean Flags */}
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Checkbox.Root checked={product.is_taxable} disabled>
                                <Checkbox.Control />
                                <Checkbox.Label>Taxable</Checkbox.Label>
                            </Checkbox.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Checkbox.Root checked={product.is_stock} disabled>
                                <Checkbox.Control />
                                <Checkbox.Label>Stock Item</Checkbox.Label>
                            </Checkbox.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Checkbox.Root checked={product.is_material} disabled>
                                <Checkbox.Control />
                                <Checkbox.Label>Material</Checkbox.Label>
                            </Checkbox.Root>
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={3}></GridItem>

                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Checkbox.Root checked={product.is_rental_item} disabled>
                                <Checkbox.Control />
                                <Checkbox.Label>Rental Item</Checkbox.Label>
                            </Checkbox.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Checkbox.Root checked={product.is_sales_item} disabled>
                                <Checkbox.Control />
                                <Checkbox.Label>Sales Item</Checkbox.Label>
                            </Checkbox.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Checkbox.Root checked={product.is_labor} disabled>
                                <Checkbox.Control />
                                <Checkbox.Label>Labor</Checkbox.Label>
                            </Checkbox.Root>
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={3}></GridItem>

                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Checkbox.Root checked={product.is_shippable} disabled>
                                <Checkbox.Control />
                                <Checkbox.Label>Shippable</Checkbox.Label>
                            </Checkbox.Root>
                        </Field.Root>
                    </Stack>
                    <Stack gap="4" align="flex-start" maxW="md">
                        <Field.Root>
                            <Checkbox.Root checked={product.is_retired} disabled>
                                <Checkbox.Control />
                                <Checkbox.Label>Retired</Checkbox.Label>
                            </Checkbox.Root>
                        </Field.Root>
                    </Stack>
                    <GridItem colSpan={5}></GridItem>
                </Grid>
            </Tabs.Content> 
            <Tabs.Content value="bom">
                <div style={{ width: "100%", height: "500px" }}>
                    <h1>BOM List</h1>
                    <AgGridReact
                        rowData={rowBOMData}
                        columnDefs={colBOMDefs}
                        defaultColDef={defaultColBOMDef}
                        rowDragManaged={false}
                        onGridReady={() => {}}
                    />
                </div>
            </Tabs.Content>
        </Tabs.Root>
    );
}

export default ViewProductPage;
