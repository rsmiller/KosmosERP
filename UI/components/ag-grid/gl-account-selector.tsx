import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";
import { forwardRef, useEffect, useImperativeHandle, useState } from 'react';
import { CustomCellEditorProps } from "ag-grid-react";


interface GLAccountSelectorProps extends CustomCellEditorProps {
    accounts?: any[];
}

const GLAccountSelector = forwardRef<any, GLAccountSelectorProps>((params, ref) => {
    const { contains } = useFilter({ sensitivity: "base" })

    const [selectedValue, setSelectedValue] = useState<string[]>([]);
    const [glAccounts, setGLAccounts] = useState<any[]>([]);
    const [selectedItem, setSelectedItem] = useState<string[]>([]);

    const { collection, filter, set } = useListCollection({
        initialItems: glAccounts,
        filter: contains,
    })

    useEffect(() => {
        if (params.accounts) {
            setGLAccounts(params.accounts);
            set(params.accounts);
        }

        if (params.value) {
            setSelectedValue([params.value]);
        }

    }, [params.accounts, params.value, set]);


    const inputValChange = (inputValue: any) =>
    {
        filter(inputValue);
    }

    // Expose getValue method to ag-grid
    useImperativeHandle(ref, () => ({
        getValue: () => {
            if (selectedValue.length > 0) {
                const matchingStage = glAccounts.find(stage => stage.value === selectedValue[0]);
                return matchingStage ? matchingStage.key : params.value;
            }
            return params.value;
        },
        isPopup: () => {
            return true;
        },
        afterGuiAttached: () => {
            // Focus the input when editor is opened
        }
    }));

    const inputChange = (inputValue: any) => {
        if (inputValue.value !== undefined && inputValue.value.length > 0) {
            const selectedStage = glAccounts.find(item => item.value === inputValue.value[0]);

            if (selectedStage) {
                // keep selectedValue as an array (Combobox expects array for value)
                setSelectedValue([inputValue.value[0]]);
                setSelectedItem([inputValue.value[0]]);
                // Notify ag-grid of the change if provided
                if (params.onValueChange) {
                    params.onValueChange(selectedStage.value);
                }
            }
        }
    };

    return (
        <Combobox.Root
            multiple={false}
            collection={collection}
            onValueChange={(e) => inputChange(e)}
            onInputValueChange={(e) => inputValChange(e.inputValue)}
            value={selectedValue}
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
    )
    
});

GLAccountSelector.displayName = 'GLAccountSelector';

export default GLAccountSelector; 
