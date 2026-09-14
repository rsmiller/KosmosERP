import type { CustomCellRendererProps } from 'ag-grid-react';

export default (params: CustomCellRendererProps) => {
    //console.log(params);

    return (
        <div>
            {params.data.units_shipped} / {params.data.units_to_ship}
        </div>
    );
    
}
