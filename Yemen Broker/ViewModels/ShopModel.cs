using System;
using System.ComponentModel;

namespace Yemen_Broker.ViewModels
{
    public class ShopModel
    {
        public long Id { get; set; }
        [DisplayName("Number of doors")]
        public int NumberOfDoors { set; get; }
        [DisplayName("Streat area")]
        public String StreetArea { set; get; }
        [DisplayName("Price")]
        public double AdPrice { set; get; }
        [DisplayName("Description")]
        public String AdDescribtion { set; get; }
        [DisplayName("City name")]
        public int CityId { get; set; }
        public String Pictures { get; set; }
    }
}