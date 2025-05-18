"use client"

import {
  Combobox,
  Portal,
  useFilter,
  useListCollection,
} from "@chakra-ui/react"
import { useEffect } from "react"

function CustomerCombobox({value, onChange}: any) {
    const { contains } = useFilter({ sensitivity: "base" })

    const customers = [
        { label: "React", value: "react" },
        { label: "Solid", value: "solid" },
        { label: "Vue", value: "vue" },
        { label: "Angular", value: "angular" },
        { label: "Svelte", value: "svelte" },
        { label: "Preact", value: "preact" },
        { label: "Qwik", value: "qwik" },
        { label: "Lit", value: "lit" },
        { label: "Alpine.js", value: "alpinejs" },
        { label: "Ember", value: "ember" },
        { label: "Next.js", value: "nextjs" },
    ]

    useEffect(() => {
        /// FETCH DATA
    }, []);

    const { collection, filter } = useListCollection({
        initialItems: customers,
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
            <Combobox.Label>Select Customer</Combobox.Label>
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

export default CustomerCombobox;