using System;
using System.Collections.Generic;
using System.Text;

namespace KosmosERP.BusinessLayer.ShipmentProviders.Models
{
    public class ShipStationPackage : IShippingPackage
    {
        public string package_id { get; set; }
        public string package_code { get; set; }
        public string package_name { get; set; }
        public ShipStationWeight weight { get; set; }
        public ShipStationDimensions dimensions { get; set; }
        public List<ShipStationInsuredValue> insured_value { get; set; } = new();
        public ShipStationLabelMessages label_messages { get; set; }
        public string external_package_id { get; set; }
        public string content_description { get; set; }
    }
}
