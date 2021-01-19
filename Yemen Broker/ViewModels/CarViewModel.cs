using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Yemen_Broker.ViewModels
{
    public class CarViewModel
    {
        public long Id { get; set; }
        [DisplayName("Price")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        [Required]
        public double AdPrice { set; get; }
        [DisplayName("Title")]
        [Required]
        public String AdTitle { set; get; }
        [DisplayName("Description")]
        [Required]
        public String AdDescribtion { set; get; }
        [DisplayName("City name")]
        public int CityId { get; set; }
        public String Pictures { get; set; }

        [DisplayName("Car Company")]
        [Required]
        public string CarCompany { get; set; }
        [DisplayName("Car Name")]
        [Required]
        public string CarName { get; set; }
        [DisplayName("Car Type")]
        [Required]
        public string CarType { get; set; }
        [DisplayName("Type of Gas")]
        [Required]
        public string TypeofGas { get; set; }
        [DisplayName("Type of Gear")]
        [Required]
        public string TypeofGear { get; set; }
        [DisplayName("Car Model")]
        [Required]
        public string CarModel { get; set; }
        [DisplayName("Engine Type")]
        [Required]
        public string EngineType { get; set; }
        [DisplayName("Car Color")]
        [Required]
        public string CarColor { get; set; }
        [DisplayName("Car Status")]
        [Required]
        public string Status { get; set; }
    }
}