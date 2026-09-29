namespace KosmosERP.BusinessLayer.ShipmentProviders.Models.ShipStation
{
    public class ShipStationAddress
    {
        public string? name { get; set; }

        public string? phone { get; set; }

        public string? email { get; set; }

        public string? company_name { get; set; }

        public string? address_line1 { get; set; }

        public string? address_line2 { get; set; }

        public string? address_line3 { get; set; }

        public string? city_locality { get; set; }

        public string? state_province { get; set; }

        public string? postal_code { get; set; }

        public string? country_code { get; set; }

        /// <summary>"yes", "no", or "unknown".</summary>
        public string? address_residential_indicator { get; set; }
    }
}
