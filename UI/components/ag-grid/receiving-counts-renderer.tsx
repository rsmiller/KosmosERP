import type { CustomCellRendererProps } from 'ag-grid-react';

export default (params: CustomCellRendererProps) => {
    //console.log(params);

    return (
        <div>
            {params.data.units_received} / {params.data.units_ordered}
        </div>
    );
    
}
