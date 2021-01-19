using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yemen_Broker.ViewModels
{
    public class ShopModel
    {
        public long Id { get; set; }
        [DisplayName("Number of doors")]
        [Range(minimum:1,maximum:5)]
        public int NumberOfDoors { set; get; }
        [DisplayName("Streat area")]
        public String StreetArea { set; get; }
        [DisplayName("Price")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public double AdPrice { set; get; }
        [DisplayName("Title")]
        [Required]
        public String AdTitle { set; get; }
        [DisplayName("Description")]
        public String AdDescribtion { set; get; }
        [DisplayName("City name")]
        public int CityId { get; set; }
        public String Pictures { get; set; }
    }
}