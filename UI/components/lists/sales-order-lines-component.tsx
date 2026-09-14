import { Grid, GridItem } from "@chakra-ui/react";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import { useEffect, useState } from "react";
import { CurrencyFormatter } from "../ag-grid/currency-formatter";
import { OrderLineDto } from "@/models/sales-order-models";
import { orderService } from "@/services/order-service";
import { useKeycloak } from '@react-keycloak/web';

ModuleRegistry.registerModules([AllCommunityModule]);

export class SalesOrdersLinesComponentParams
{
    order_header_id: any;
    onSelect: any;
}

export class SalesOrderSelectorForm
{
    order_search: any;
}

function SalesOrdersLinesComponent({order_header_id, onSelect}: SalesOrdersLinesComponentParams) {
    
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [rowData, setRowData] = useState<OrderLineDto[]>([]);
    const { keycloak } = useKeycloak();
    
    const [colDefs, setColDefs] = useState<ColDef<OrderLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "line_description", headerName: "Description"},
    { field: "quantity", headerName: "Quantity"},
    { field: "unit_price", headerName: "Unit Price", 
        cellRenderer: CurrencyFormatter 
    },
    { headerName: "Total", 
        cellRenderer: (props: any) => 
        {
        const total = props.data.quantity * props.data.unit_price;
        return "$" + total.toFixed(2);
        }
    }
    ]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: true,
        sortable: true,
    };
    
    const loadSalesOrder = async () => {
        try {
            const response = await orderService.get(order_header_id, keycloak?.token || "");
            //console.log(response)

            if (response.success && response.data) {

                if(response.data.order_lines) {
                setRowData(response.data.order_lines);
                }
            } else {
                setError('Failed to load sales order');
            }
        } catch (err) {
            console.error('Error loading sales order:', err);
            setError('Error loading sales order');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if(keycloak.authenticated == false) return;
    
        loadSalesOrder();
    }, [order_header_id, keycloak.authenticated]);

    return (
        <Grid
            templateColumns="repeat(5, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
        >
            <GridItem colSpan={5} >
                <div style={{ width: "100%", height: "500px" }}>
                    <AgGridReact
                        rowData={rowData}
                        columnDefs={colDefs}
                        defaultColDef={defaultColDef}
                    />
                </div>
            </GridItem>
        </Grid>
    );
}

export default SalesOrdersLinesComponent;