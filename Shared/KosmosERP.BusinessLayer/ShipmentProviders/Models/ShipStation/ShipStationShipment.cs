namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation
{
    public class ShipStationShipment
    {
        public string? shipment_id { get; set; }

        public string? carrier_id { get; set; }

        public string? service_code { get; set; }

        public string? requested_shipment_service { get; set; }

        public string? external_order_id { get; set; }

        public DateTime? hold_until_date { get; set; }

        public DateTime? ship_by_date { get; set; }

        public DateTime? deliver_by_date { get; set; }

        public ShipStationMonetaryValue? retail_rate { get; set; }

        public string? store_id { get; set; }

        public List<ShipStationShipmentItem> items { get; set; } = new();

        public string? notes_from_buyer { get; set; }

        public string? notes_to_buyer { get; set; }

        public string? notes_for_gift { get; set; }

        public string? internal_notes { get; set; }

        public bool? is_gift { get; set; }

        public string? assigned_user { get; set; }

        public ShipStationMonetaryValue? amount_paid { get; set; }

        public ShipStationMonetaryValue? shipping_paid { get; set; }

        public ShipStationMonetaryValue? tax_paid { get; set; }

        public int? zone { get; set; }

        public string? display_scheme { get; set; }

        public List<ShipStationTaxIdentifier>? tax_identifiers { get; set; }

        public string? external_shipment_id { get; set; }

        public string? shipment_number { get; set; }

        public DateTimeOffset? ship_date { get; set; }

        public DateTimeOffset? created_at { get; set; }

        public DateTimeOffset? modified_at { get; set; }

        public string? shipment_status { get; set; }

        public ShipStationAddress? ship_to { get; set; }

        public ShipStationAddress? ship_from { get; set; }

        public string? warehouse_id { get; set; }

        public ShipStationAddress? return_to { get; set; }

        public bool? is_return { get; set; }

        public string? confirmation { get; set; }

        public ShipStationCustoms? customs { get; set; }

        public ShipStationAdvancedOptions? advanced_options { get; set; }

        public string? insurance_provider { get; set; }

        public List<ShipStationTag> tags { get; set; } = new();

        public string? order_source_code { get; set; }

        public List<ShipStationPackage> packages { get; set; } = new();

        public ShipStationWeight? total_weight { get; set; }

        public List<string>? batch_ids { get; set; }

        public string? comparison_rate_type { get; set; }

        public List<string>? errors { get; set; }

        public ShipStationAddressValidation? address_validation { get; set; }
    }
}
