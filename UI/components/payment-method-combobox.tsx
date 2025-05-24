"use client"

import {
  Combobox,
  Portal,
  useFilter,
  useListCollection,
} from "@chakra-ui/react"
import { useEffect } from "react"

function PaymentMethodCombobox({value, onChange}: any) {
    const { contains } = useFilter({ sensitivity: "base" })

    const payment_types = [
        { label: "Cash", value: "1" },
        { label: "Card", value: "2" },
        { label: "PO", value: "3" }
    ]

    useEffect(() => {
        /// FETCH DATA
        // Module id = 83156a35-d140-4442-8fbf-699658bf65e9
    }, []);

    const { collection, filter } = useListCollection({
        initialItems: payment_types,
        filter: contains,
    })

    const inputChange = (inputValue: any) =>
    {
        filter(inputValue);
        onChange(inputValue);
    }

    return (
        <Combobox.Root
            collection={collection}
            onInputValueChange={(e) => inputChange(e.inputValue)}
            width="100%"
        >
            <Combobox.Label>Select Payment Method</Combobox.Label>
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
}

export default PaymentMethodCombobox;