import type { CustomCellRendererProps } from 'ag-grid-react';

export default (params: CustomCellRendererProps) => {
    //console.log(params);

    if(params.data != undefined && params.data.address != undefined)
    {

        if(params.data.address.street_address2 != '' && params.data.address.street_address2 != undefined)
        {
            return (
                <div>
                    <div className="address_line">{params.data.address.street_address1}</div>
                    <div className="address_line">{params.data.address.street_address2}</div>
                    <div className="address_line">{params.data.address.city}, {params.data.address.state} {params.data.address.postal_code} {params.data.address.country}</div>
                </div>
            );
        }
        else
        {
            return (
                <div>
                    <div className="address_line">{params.data.address.street_address1}</div>
                    <div className="address_line">{params.data.address.city}, {params.data.address.state} {params.data.address.postal_code} {params.data.address.country}</div>
                </div>
            );
        }
    }
    else
    {
        return;
    }
    
}
