import { useImperativeHandle, forwardRef, useState, useEffect } from "react";
import { CustomCellEditorProps } from "ag-grid-react";
import { KeyValueDto } from "@/models/key-value-models";
import { keyValueService } from "@/services/keyvalue-service";
import {
    Combobox,
    Portal,
    useFilter,
    useListCollection,
} from "@chakra-ui/react";
import { useKeycloak } from '@react-keycloak/web';

const ProductionStatusCellEditor = forwardRef<any, CustomCellEditorProps>((props, ref) => {
    const { contains } = useFilter({ sensitivity: "base" });
    const [stages, setStages] = useState<KeyValueDto[]>([]);
    const [selectedValue, setSelectedValue] = useState<string>("");
    const [selectedItem, setSelectedItem] = useState<string[]>([]);
    const [initialized, setInitialized] = useState(false);
    const { keycloak } = useKeycloak();

    const { collection, filter, set } = useListCollection<KeyValueDto>({
        initialItems: stages,
        filter: contains,
        itemToString: (item) => item.key,
        itemToValue: (item) => item.value,
    });

    const fetchData = async () => {
        try {
            const response = await keyValueService.GetDtoByModule("f157469e-5e5c-4a5b-b071-89a28b2a0310", keycloak?.token || "");
            if (response.success && response.data !== undefined) {
                set(response.data);
                setStages(response.data);
                
                // Set initial value if provided
                if (props.value && response.data.length > 0) {
                    const matchingItem = response.data.find((item: KeyValueDto) => item.key === props.value);
                    if (matchingItem) {
                        setSelectedValue(matchingItem.value);
                        setSelectedItem([matchingItem.value]);
                    }
                }
                setInitialized(true);
            }
        } catch (error) {
            console.error('Error loading production status options:', error);
            setInitialized(true);
        }
    };

    useEffect(() => {
        if(keycloak.authenticated == false) return;

        fetchData();
    }, [props.value, set, keycloak.authenticated]);

    // Expose getValue method to ag-grid
    useImperativeHandle(ref, () => ({
        getValue: () => {
            if (selectedItem.length > 0) {
                const matchingStage = stages.find(stage => stage.value === selectedItem[0]);
                return matchingStage ? matchingStage.key : props.value;
            }
            return props.value;
        },
        isPopup: () => {
            return true;
        },
        afterGuiAttached: () => {
            // Focus the input when editor is opened
        }
    }));

    const inputValChange = (inputValue: any) => {
        filter(inputValue);
    };

    const inputChange = (inputValue: any) => {
        if (inputValue.value !== undefined && inputValue.value.length > 0) {
            const selectedStage = stages.find(item => item.value === inputValue.value[0]);
            if (selectedStage) {
                setSelectedValue(inputValue.value[0]);
                setSelectedItem([selectedStage.value]);
                // Notify ag-grid of the change
                if (props.onValueChange) {
                    props.onValueChange(selectedStage.key);
                }
            }
        } else {
            //setSelectedValue('');
            //setSelectedItem([]);
            // Notify ag-grid of the change
            //if (props.onValueChange) {
            //    props.onValueChange('');
            //}
        }
    };

    if (!initialized) {
        return <div>Loading...</div>;
    }

    return (
        <Combobox.Root
            collection={collection}
            inputValue={selectedValue}
            value={selectedItem}
            onValueChange={(e) => inputChange(e)}
            onInputValueChange={(e) => inputValChange(e.inputValue)}
            allowCustomValue={false}
            width="100%"
        >
            <Combobox.Control>
                <Combobox.Input placeholder="Select status" />
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
                            <Combobox.Item item={item} key={item.key}>
                                {item.value}
                                <Combobox.ItemIndicator />
                            </Combobox.Item>
                        ))}
                    </Combobox.Content>
                </Combobox.Positioner>
            </Portal>
        </Combobox.Root>
    );
});

ProductionStatusCellEditor.displayName = 'ProductionStatusCellEditor';

export default ProductionStatusCellEditor; 