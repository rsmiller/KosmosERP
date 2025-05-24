"use client"

import {
  Combobox,
  Portal,
  useFilter,
  useListCollection,
  HStack,
  Button
} from "@chakra-ui/react"
import { useEffect } from "react"

function HeaderTypeSelectorCombobox({value, title, onChange}: any) {
    const { contains } = useFilter({ sensitivity: "base" })

    const addresses = [
        { label: "Quote", value: "Q" },
        { label: "Release", value: "R" },
    ]

    useEffect(() => {
        /// FETCH DATA

    }, []);

    const { collection, filter } = useListCollection({
        initialItems: addresses,
        filter: contains,
    })

    const inputChange = (inputValue: any) =>
    {
        filter(inputValue);
        onChange(inputValue);
    }


    return (
        <HStack direction="row" gap="4">
            <Combobox.Root
                collection={collection}
                onInputValueChange={(e) => inputChange(e.inputValue)}
                width="100%"
            >
                <Combobox.Label>{title}</Combobox.Label>
                <Combobox.Control>
                <Combobox.Input placeholder="Choose Type" />
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
        </HStack>
    )
}

export default HeaderTypeSelectorCombobox;