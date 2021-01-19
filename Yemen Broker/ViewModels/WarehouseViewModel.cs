using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Yemen_Broker.ViewModels
{
    public class WarehouseViewModel
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


        [Display(Name = "Number of doors")]
        public int NumberOfDoors { get; set; }
        [Display(Name = "Height of Wall")]
        public int HeightOfWall { get; set; }
        [Display(Name = "Street area")]
        public string StreetArea { get; set; }


    }
}