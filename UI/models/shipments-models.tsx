import { AddressDto } from "./address-models";
import { BaseDto } from "./base-models";

export interface ShipmentLineDto extends BaseDto {
    shipment_header_id?: number;
    order_line_id?: number;
    units_to_ship?: number;
    units_shipped?: number;
    is_complete?: boolean;
    is_canceled?: boolean;
    completed_on?: Date | null;
    completed_by?: number | null;
    canceled_on?: Date | null;
    canceled_by?: number | null;
    canceled_reason?: string | null;
}

export interface ShipmentHeaderDto extends BaseDto {
    order_header_id?: number;
    shipment_number?: number;
    address_id?: number;
    units_to_ship?: number;
    units_shipped?: number;
    is_complete?: boolean;
    is_canceled?: boolean;
    ship_via?: string;
    ship_attn?: string | null;
    freight_carrier?: string | null;
    freight_charge_amount?: number;
    tax?: number;
    completed_on?: Date | null;
    completed_by?: number | null;
    canceled_on?: Date | null;
    canceled_by?: number | null;
    canceled_reason?: string | null;

    shipment_lines?: ShipmentLineDto[];
    address?: AddressDto;
}

export interface ShipmentHeaderListDto extends BaseDto {
    order_header_id?: number;
    shipment_number?: number;
    address_id?: number;
    units_to_ship?: number;
    units_shipped?: number;
    is_complete?: boolean;
    is_canceled?: boolean;
    ship_via?: string;
    ship_attn?: string | null;
    freight_carrier?: string | null;
    freight_charge_amount?: number;
    tax?: number;
    completed_on?: Date | null;
    completed_by?: number | null;
    canceled_on?: Date | null;
    canceled_by?: number | null;
    canceled_reason?: string | null;

    address?: AddressDto;
}
