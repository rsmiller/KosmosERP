using System;
using System.Collections.Generic;
using System.Text;

namespace KosmosERP.BusinessLayer.ShipmentProviders.Models
{
    public class ShipStationDimensions
    {
        public string unit { get; set; }
        public decimal length { get; set; }
        public decimal width { get; set; }
        public decimal height { get; set; }
    }
}
