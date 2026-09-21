"use client"

import { ProductDto, ProductFindCommand, ProductListDto } from "@/models/product-models";
import { productService } from "@/services/product-service";
import {
    Combobox,
    Portal,
    useListCollection,
} from "@chakra-ui/react"
import { useAuth } from '@/lib/auth/auth-context';
import { useEffect, useState, useImperativeHandle, forwardRef } from "react"
import { Controller, Control, FieldError } from "react-hook-form";

export class ProductComboboxParams
{
    dbKey: any
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    onValidationChange?: (isValid: boolean) => void;
}

export interface ProductComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const ProductCombobox = forwardRef<ProductComboboxRef, ProductComboboxParams>(
    ({dbKey, onChange, control, name, error, onValidationChange}, ref) => {
        const auth = useAuth();

        const [products, setProducts] = useState<ProductListDto[]>([]);
        const [loading, setLoading] = useState(false);
        const [selectedValue, setSelectedValue] = useState<string>();
        const [selectedItem, setSelectedItem] = useState<any[]>([]);
        const [isValid, setIsValid] = useState<boolean>(false);

        const { collection, set } = useListCollection<ProductListDto>({
            initialItems: products,
            itemToString: (item) => item.id.toString(),
            itemToValue: (item) => item.product_name ? item.product_name : "-- ERROR --",
        })

        // Expose methods to parent component
        useImperativeHandle(ref, () => ({
            isValid: () => isValid,
            getValue: () => selectedItem[0] || null,
            clear: () => {
                setSelectedValue('');
                setSelectedItem([]);
                onChange(null);
            }
        }), [isValid, selectedItem, onChange]);

        // Update validation state when selectedItem changes
        useEffect(() => {
            const valid = selectedItem.length > 0 && selectedItem[0] !== null;
            setIsValid(valid);
            if (onValidationChange) {
                onValidationChange(valid);
            }
        }, [selectedItem, onValidationChange]);

        useEffect(() => {
            if (dbKey) {
                fetchProduct(dbKey).then( (response) => {
                    if(response != undefined)
                    {
                        setProducts([response]);
                        set([response]);
                        setSelectedValue(response.product_name);
                        setSelectedItem([response]);
                        setIsValid(true);

                        if (onValidationChange) {
                            onValidationChange(true);
                        }
                    } else {
                        setSelectedValue('');
                        setSelectedItem([]);
                        setIsValid(false);

                        if (onValidationChange) {
                            onValidationChange(false);
                        }
                    }
                });
            } else {
                setSelectedValue('');
                setSelectedItem([]);
                setIsValid(false);

                if (onValidationChange) {
                    onValidationChange(false);
                }
            }
        }, [dbKey]);

        const fetchProduct = async (id: number): Promise<ProductDto | undefined> => {
            //console.log("Fetching product with ID:", id);
            try {
                setLoading(true);
                const response = await productService.get(id, auth.token || "");
                
                if (response.success && response.data !== undefined) 
                {
                    setLoading(false);
                    return response.data;
                }

                return undefined;
            } catch (error) {
                console.error("Error fetching products:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const fetchProducts = async (searchTerm: string): Promise<ProductListDto[] | undefined> => {
            try {
                setLoading(true);
                let findCommand = new ProductFindCommand();
                findCommand.wildcard = searchTerm;
                
                const response = await productService.find(findCommand, auth.token || "", 1, 20, "product_name-asc");
                //console.log(response)
                if (response.success && response.data !== undefined) {
                    set(response.data);
                    setProducts(response.data);
                    return response.data;
                }
                return undefined;
            } catch (error) {
                console.error("Error fetching products:", error);
                return undefined;
            } finally {
                setLoading(false);
            }
        }

        const inputValChange = async (inputValue: string) => {
            // Only search if input is empty or has 3+ characters
            if (inputValue.length === 0 || inputValue.length >= 3) {
                await fetchProducts(inputValue);
            }
        }

        const inputChange = (inputValue: any) => {

            if(inputValue.value != undefined && inputValue.value.length > 0)
            {
                const selectedItem = products.find(item => item.product_name === inputValue.value[0]);

                if (selectedItem) {
                    setSelectedValue(inputValue.value[0]);
                    setSelectedItem([selectedItem]);
                    onChange(selectedItem);
                    setIsValid(true);

                    if (onValidationChange) {
                        onValidationChange(true);
                    }

                } else {
                    setSelectedValue('');
                    setSelectedItem([]);
                    onChange(null);
                    setIsValid(false);

                    if (onValidationChange) {
                        onValidationChange(false);
                    }
                }
            }
            else
            {
                setSelectedValue('');
                setSelectedItem([]);
                onChange(null);
                setIsValid(false);

                if (onValidationChange) {
                    onValidationChange(false);
                }
            }
        }

        const comboboxComponent = (
            <Combobox.Root
                collection={collection}
                inputValue={selectedValue}
                value={selectedItem}
                onValueChange={(e) => inputChange(e)}
                onInputValueChange={(e) => inputValChange(e.inputValue)}
                allowCustomValue={false}
                required={true}
                width="100%"
            >
                <Combobox.Control>
                <Combobox.Input placeholder="Type to search" />
                <Combobox.IndicatorGroup>
                    <Combobox.ClearTrigger />
                    <Combobox.Trigger />
                </Combobox.IndicatorGroup>
                </Combobox.Control>
                <Portal>
                <Combobox.Positioner style={{ zIndex: 9999 }}>
                    <Combobox.Content style={{ zIndex: 9999 }}>
                    <Combobox.Empty>
                        {loading ? "Loading..." : "No items found"}
                    </Combobox.Empty>
                    {collection.items.map((item) => (
                        <Combobox.Item item={item} key={item.id}>
                        {item.product_name}
                        <Combobox.ItemIndicator />
                        </Combobox.Item>
                    ))}
                    </Combobox.Content>
                </Combobox.Positioner>
                </Portal>
            </Combobox.Root>
        );

        // If control and name are provided, wrap with Controller for React Hook Form integration
        if (control && name) {
            return (
                <Controller
                    name={name}
                    control={control}
                    rules={{ required: "Customer is required" }}
                    render={({ field }) => (
                        <div className={"full-width"}>
                            {comboboxComponent}
                            {error && (
                                <div style={{ color: 'red', fontSize: '0.875rem', marginTop: '0.25rem' }}>
                                    {error.message}
                                </div>
                            )}
                        </div>
                    )}
                />
            );
        }

        // Return the component without Controller wrapper
        return comboboxComponent;
    }
);


export default ProductCombobox;