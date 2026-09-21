
import { ModuleObjectDto } from "@/models/key-value-models";
import { keyValueService } from "@/services/keyvalue-service";
import { Combobox, Portal, useFilter, useListCollection } from "@chakra-ui/react";
import { CustomCellEditorProps } from "ag-grid-react";
import { useEffect, useState } from "react";
import { useAuth } from '@/lib/auth/auth-context';

const ModuleComboboxEditor = (
    ({ data, value, onValueChange }: CustomCellEditorProps) => {

    const { contains } = useFilter({ sensitivity: "base" })
    const auth = useAuth();
    const [selectedValue, setSelectedValue] = useState<string[]>([]);
    const [moduleData, setModuleData] = useState<ModuleObjectDto[]>([]);
    //console.log(params);

    const toValueFormater = (dto: any) =>
    {
        if(dto.name != undefined)
        {
            return dto.name;
        }

        if(dto.module_name != undefined)
        {
            return dto.module_name;
        }

        return "-- ERROR --";
    }

    const { collection, filter, set } = useListCollection({
        initialItems: moduleData,
        filter: contains,
        itemToString: (item) => item.module_id ? item.module_id : '-01',
        itemToValue: (item) => toValueFormater(item),
    })

    useEffect(() => {
        //console.log("useEffect:");
        
        if(auth.authenticated == false) return;
        
        keyValueService.getModuleAndKeyValueTypes(auth.token || "").then( (response) => 
        {
            //console.log("getModuleAndKeyValueTypes:", response)
            if(response.success && response.data)
            {
                setModuleData(response.data);
                set(response.data);
            }
        });
        
    }, [auth.authenticated]);

    useEffect(() => {
        if(value)
        {
            setSelectedValue([value]);
        }
        
    }, [value, set]);


    const inputValChange = (inputValue: any) =>
    {
        filter(inputValue);
    }

    const inputChange = (inputValue: any) =>
    {
        setSelectedValue([inputValue.value[0]]);
    }

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
                <Combobox.Positioner style={{ zIndex: 9999 }}>
                    <Combobox.Content style={{ zIndex: 9999 }}>
                    <Combobox.Empty>No items found</Combobox.Empty>
                    {collection.items.map((item) => (
                        <Combobox.Item item={item} key={item.module_id}>
                        {toValueFormater(item)}
                        <Combobox.ItemIndicator />
                        </Combobox.Item>
                    ))}
                    </Combobox.Content>
                </Combobox.Positioner>
            </Portal>
        </Combobox.Root>
    );
});

export default ModuleComboboxEditor;