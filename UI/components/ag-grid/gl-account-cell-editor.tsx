import { useImperativeHandle, forwardRef, useState, useEffect } from "react";
import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";

const GLAccountCellEditor = forwardRef((props, ref) => {
    const { contains } = useFilter({ sensitivity: "base" })
    const [value, setValue] = useState<number | null>(null);
    const [selectedValue, setSelectedValue] = useState<string[]>([]);

    console.log(props);

    const gl_accounts = [    
        { label: "Cash - 1000001", value: 1 },
        { label: "Checking - 1000002", value: 2 },
        { label: "Payroll - 1005001", value: 3 },
        { label: "Sales - 1003001", value: 4 }
    ]

    const { collection, filter } = useListCollection({
            initialItems: gl_accounts,
            filter: contains,
    })

    useImperativeHandle(ref, () => ({
        getValue: () => {
        return value;
        }
    }));



    return (
        <Combobox.Root
            multiple={false}
            collection={collection}
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
    );
});

export default GLAccountCellEditor;