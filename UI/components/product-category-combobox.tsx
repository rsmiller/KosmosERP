"use client"

import {
  Combobox,
  Portal,
  useFilter,
  useListCollection,
} from "@chakra-ui/react"
import { useEffect } from "react"

function PrpductCategoryCombobox({value, onChange}: any) {
    const { contains } = useFilter({ sensitivity: "base" })

    const payment_types = [
        { label: "Welding Equipment", value: "Welding Equipment" },
        { label: "Frame", value: "Frame" },
        { label: "Electrical", value: "Electrical" },
        { label: "Accessories", value: "Accessories" },
        { label: "Mechanical", value: "Mechanical" },
        { label: "Packaging", value: "Packaging" },
        { label: "Hardware", value: "Hardware" },
    ]

    useEffect(() => {
        /// FETCH DATA
        // Module id = f6e28b05-265d-4416-b5fd-48399036493a
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
            <Combobox.Label>Select Product Category</Combobox.Label>
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

export default PrpductCategoryCombobox;