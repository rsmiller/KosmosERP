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

function AddressSelectorCombobox({value, title, onChange}: any) {
    const { contains } = useFilter({ sensitivity: "base" })

    const addresses = [
        { label: "86261 Freeman Squares, Lake Jennifer, TX 76523", value: "1" },
        { label: "60353 Norman Views, Robertshire, OK 98762", value: "2" },
        { label: "36206 Nichols Extension Suite 312, Charlesborough, TX 76092", value: "3" }
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

    const handleNewAddressClick = () => {

    };

    return (
        <HStack direction="row" gap="4">
            <Combobox.Root
                collection={collection}
                onInputValueChange={(e) => inputChange(e.inputValue)}
                width="100%"
            >
                <Combobox.Label>{title}</Combobox.Label>
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
            <div style={{ marginTop: "22px"}}>
                <Button type="button" colorPalette="blue" onClick={() => handleNewAddressClick}>New</Button>
            </div>
        </HStack>
    )
}

export default AddressSelectorCombobox;