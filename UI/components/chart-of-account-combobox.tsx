"use client"

import { ChartOfAccountDto, ChartOfAccountFindCommand, ChartOfAccountListDto } from "@/models/chart-of-account-models";
import { chartOfAccountService } from "@/services/chart-of-account-service";
import {
    Combobox,
    Portal,
    useListCollection,
} from "@chakra-ui/react"
import { useEffect, useState, useImperativeHandle, forwardRef } from "react"
import { useAuth } from '@/lib/auth/auth-context';
import { Controller, Control, FieldError } from "react-hook-form";


export class ChartOfAccountComboboxParams
{
    dbKey: any;
    onChange: any;
    control?: Control<any>;
    name?: string;
    error?: FieldError;
    disabled: boolean = false;
    onValidationChange?: (isValid: boolean) => void;
    required?: boolean = true;
    accountType?: number;  // Filter by account type if provided
}

export interface ChartOfAccountComboboxRef {
    isValid: () => boolean;
    getValue: () => any;
    clear: () => void;
}

const ChartOfAccountCombobox = forwardRef<ChartOfAccountComboboxRef, ChartOfAccountComboboxParams>(
    ({dbKey, onChange, control, name, error, disabled, onValidationChange, required, accountType}, ref) => {
        const auth = useAuth();

        const [accounts, setAccounts] = useState<ChartOfAccountListDto[]>([]);
        const [loading, setLoading] = useState(false);
        const [selectedValue, setSelectedValue] = useState<string>();
        const [selectedItem, setSelectedItem] = useState<any[]>([]);
        const [isValid, setIsValid] = useState<boolean>(false);

        const { collection, set } = useListCollection<ChartOfAccountListDto>({
            initialItems: accounts,
            itemToString: (item) => item.id.toString(),
            itemToValue: (item) => item.account_number && item.account_name 
                ? `${item.account_number} - ${item.account_name}` 
                : "-- ERROR --",
        })

        // Expose methods to parent component
        useImperativeHandle(ref, () => ({
            isValid: () => isValid,
            getValue: () => selectedItem[0] || null,
            clear: () => {
                setSelectedValue('');
                setSelectedItem([]);
                onChange(null);
                setIsValid(false);
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
                fetchAccount(dbKey).then( (response) => {
                    if(response != undefined)
                    {
                        setAccounts([response]);
                        set([response]);
                        setSelectedValue(`${response.account_number} - ${response.account_name}`);
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

        const fetchAccount = async (id: number): Promise<ChartOfAccountDto | undefined> => {
            if (auth.authenticated == false) return;

            let command = new ChartOfAccountFindCommand();
            // We'll fetch the specific account by ID if it's a number
            try {
                const response = await chartOfAccountService.get(id, auth.token || "");
                if(response.success && response.data) {
                    return response.data;
                }
            } catch (error) {
                console.error("Error fetching account:", error);
            }
            return undefined;
        };

        const searchAccounts = async (query: string) => {
            if (auth.authenticated == false) return;
            
            setLoading(true);

            let command = new ChartOfAccountFindCommand();
            command.wildcard = query;
            command.is_active = true;
            if (accountType) {
                command.account_type = accountType;
            }

            try {
                const response = await chartOfAccountService.find(command, auth.token || "", 1, 50, "account_number-asc");
                
                if(response.success && response.data) {
                    setAccounts(response.data);
                    set(response.data);
                }
            } catch (error) {
                console.error("Error searching accounts:", error);
            } finally {
                setLoading(false);
            }
        };

        const handleInputChange = (details: any) => {
            const value = details.inputValue || '';
            setSelectedValue(value);
            
            if(value.length >= 2) {
                searchAccounts(value);
            }
        };

        const handleValueChange = (details: any) => {
            const selectedItems = details.items || [];
            setSelectedItem(selectedItems);
            
            if (selectedItems.length > 0) {
                const account = selectedItems[0];
                setSelectedValue(`${account.account_number} - ${account.account_name}`);
                onChange(account.id);
            } else {
                setSelectedValue('');
                onChange(null);
            }
        };

        if (control && name) {
            return (
                <Controller
                    control={control}
                    name={name}
                    rules={{ required: required ? 'Account is required' : false }}
                    render={({ field }) => (
                        <Combobox.Root
                            collection={collection}
                            disabled={disabled}
                            inputValue={selectedValue}
                            onInputValueChange={handleInputChange}
                            onValueChange={handleValueChange}
                            placeholder="Search accounts..."
                        >
                            <Combobox.Control>
                                <Combobox.Input 
                                    disabled={disabled}
                                />
                                <Combobox.IndicatorGroup>
                                    <Combobox.ClearTrigger />
                                    <Combobox.Trigger />
                                </Combobox.IndicatorGroup>
                            </Combobox.Control>
                            <Portal>
                                <Combobox.Positioner>
                                    <Combobox.Content>
                                        {accounts.length === 0 && !loading && (
                                            <Combobox.Empty>No accounts found</Combobox.Empty>
                                        )}
                                        {loading && (
                                            <Combobox.Empty>Loading...</Combobox.Empty>
                                        )}
                                        {accounts.map((account) => (
                                            <Combobox.Item key={account.id} item={account}>
                                                <Combobox.ItemText>
                                                    {account.account_number} - {account.account_name}
                                                </Combobox.ItemText>
                                                <Combobox.ItemIndicator>✓</Combobox.ItemIndicator>
                                            </Combobox.Item>
                                        ))}
                                    </Combobox.Content>
                                </Combobox.Positioner>
                            </Portal>
                        </Combobox.Root>
                    )}
                />
            );
        }

        return (
            <Combobox.Root
                collection={collection}
                disabled={disabled}
                inputValue={selectedValue}
                onInputValueChange={handleInputChange}
                onValueChange={handleValueChange}
                placeholder="Search accounts..."
            >
                <Combobox.Control>
                    <Combobox.Input 
                        disabled={disabled}
                    />
                    <Combobox.IndicatorGroup>
                        <Combobox.ClearTrigger />
                        <Combobox.Trigger />
                    </Combobox.IndicatorGroup>
                </Combobox.Control>
                <Portal>
                    <Combobox.Positioner>
                        <Combobox.Content>
                            {accounts.length === 0 && !loading && (
                                <Combobox.Empty>No accounts found</Combobox.Empty>
                            )}
                            {loading && (
                                <Combobox.Empty>Loading...</Combobox.Empty>
                            )}
                            {accounts.map((account) => (
                                <Combobox.Item key={account.id} item={account}>
                                    <Combobox.ItemText>
                                        {account.account_number} - {account.account_name}
                                    </Combobox.ItemText>
                                    <Combobox.ItemIndicator>✓</Combobox.ItemIndicator>
                                </Combobox.Item>
                            ))}
                        </Combobox.Content>
                    </Combobox.Positioner>
                </Portal>
            </Combobox.Root>
        );
    }
);

ChartOfAccountCombobox.displayName = 'ChartOfAccountCombobox';

export default ChartOfAccountCombobox;
