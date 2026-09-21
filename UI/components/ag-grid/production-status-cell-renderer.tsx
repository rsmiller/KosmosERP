import { useState, useEffect } from "react";
import { CustomCellRendererProps } from "ag-grid-react";
import { KeyValueDto } from "@/models/key-value-models";
import { keyValueService } from "@/services/keyvalue-service";
import { useAuth } from '@/lib/auth/auth-context';

const ProductionStatusCellRenderer = (params: CustomCellRendererProps) => {
    const [stages, setStages] = useState<KeyValueDto[]>([]);
    const [displayValue, setDisplayValue] = useState<string>("");
    const [loading, setLoading] = useState(true);
    const auth = useAuth();

    const fetchData = async () => {
        try {
            const response = await keyValueService.GetDtoByModule("f157469e-5e5c-4a5b-b071-89a28b2a0310", auth.token || "");
            if (response.success && response.data !== undefined) {
                setStages(response.data);
                
                // Find the matching value for the current key
                if (params.value && response.data.length > 0) {
                    const matchingItem = response.data.find((item: KeyValueDto) => item.key === params.value);
                    if (matchingItem) {
                        setDisplayValue(matchingItem.value);
                    } else {
                        setDisplayValue(params.value || "");
                    }
                } else {
                    setDisplayValue("");
                }
                setLoading(false);
            }
        } catch (error) {
            console.error('Error loading production status options:', error);
            setDisplayValue(params.value || "");
            setLoading(false);
        }
    };

    useEffect(() => {
        if(auth.authenticated == false) return;

        fetchData();
    }, [params.value, auth.authenticated]);

    return <span>{displayValue}</span>;
};

export default ProductionStatusCellRenderer; 