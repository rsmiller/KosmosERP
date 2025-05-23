"use client"

import {
  Combobox,
  Portal,
  useFilter,
  useListCollection,
} from "@chakra-ui/react"
import { useEffect } from "react"

function ShipmentMethodCombobox({value, onChange}: any) {
    const { contains } = useFilter({ sensitivity: "base" })

    const payment_types = [
        { label: "Pickup", value: "1" },
        { label: "Carrier", value: "2" },
        { label: "Dispatch", value: "3" },
    ]

    useEffect(() => {
        /// FETCH DATA
        // Module id = 9da95117-2792-44e5-996a-e91a244b0384
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
            <Combobox.Label>Select Shipment Method</Combobox.Label>
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

export default ShipmentMethodCombobox;