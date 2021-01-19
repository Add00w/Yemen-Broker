using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Yemen_Broker.ViewModels
{
    public class HomeViewModel
    {

        public long Id { get; set; }
        [DisplayName("Title")]
        [Required]
        public String AdTitle { set; get; }
        [DisplayName("Price")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public double AdPrice { set; get; }
        [DisplayName("Description")]
        public String AdDescribtion { set; get; }

        [Display(Name = "Number of floors")]

        public int NumberOfFloors { get; set; }
        [Display(Name = "Detail system")]

        public string DetailSystem { get; set; }
        [Display(Name = "Number of land")]

        public int NumberOfLand { get; set; }
        [Display(Name = "Plate number")]

        public int PlateNumber { get; set; }
        [Required]
        [Display(Name = "Streets area")]
        public string StreetsArea { get; set; }

        [DisplayName("City")]
        public int CityId { get; set; }
        public String Pictures { get; set; }

    }
}