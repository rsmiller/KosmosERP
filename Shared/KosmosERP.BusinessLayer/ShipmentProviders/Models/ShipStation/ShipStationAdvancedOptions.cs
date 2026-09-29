using System;
using System.Collections.Generic;
using System.Text;

namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation
{
    public class ShipStationAdvancedOptions
    {
        public string? bill_to_account { get; set; }

        public string? bill_to_country_code { get; set; }

        public string? bill_to_party { get; set; }

        public string? bill_to_postal_code { get; set; }

        public bool? contains_alcohol { get; set; }

        public bool? delivered_duty_paid { get; set; }

        public bool? dry_ice { get; set; }

        public ShipStationWeight? dry_ice_weight { get; set; }

        public bool? non_machinable { get; set; }

        public bool? saturday_delivery { get; set; }

        public ShipStationFedexFreight? fedex_freight { get; set; }

        public bool? use_ups_ground_freight_pricing { get; set; }

        public string? freight_class { get; set; }

        public string? custom_field1 { get; set; }

        public string? custom_field2 { get; set; }

        public string? custom_field3 { get; set; }

        public string? origin_type { get; set; }

        public bool? additional_handling { get; set; }

        public bool? shipper_release { get; set; }

        public ShipStationCollectOnDelivery? collect_on_delivery { get; set; }

        public bool? third_party_consignee { get; set; }

        public bool? dangerous_goods { get; set; }

        public ShipStationDangerousGoodsContact? dangerous_goods_contact { get; set; }

        public string? movement_indicator { get; set; }

        public ShipStationWindsorFrameworkDetails? windsor_framework_details { get; set; }

        public string? ancillary_endorsements_option { get; set; }

        public int? return_pickup_attempts { get; set; }

        public bool? own_document_upload { get; set; }

        public bool? limited_quantity { get; set; }

        public bool? event_notification { get; set; }

        public bool? fragile { get; set; }

        public bool? delivery_as_addressed { get; set; }

        public bool? return_after_first_attempt { get; set; }

        public string? regulated_content_type { get; set; }

        public ShipStationNetstampsOptions? netstamps_options { get; set; }

        public List<string>? suppress_carrier_generated_documents { get; set; }

        public List<ShipStationTransparencyCode>? transparency_codes { get; set; }
    }
}
