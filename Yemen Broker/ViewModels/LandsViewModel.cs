using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Yemen_Broker.Models;

namespace Yemen_Broker.ViewModels
{
    public class LandsViewModel
    {
        public long Id { get; set; }


        [DisplayName("Number of land")]
        [Required]
        public int NumberOfLand { get; set; }
        [DisplayName("Plate number")]
        [Required]
        public int PlateNumber { get; set; }
        [DisplayName("Streat area")]
        [Required]
        public string StreetsArea { get; set; }

        [DisplayName("Price")]
        [Required]
        public double AdPrice { set; get; }
        [DisplayName("Description")]
        [Required]
        public String AdDescribtion { set; get; }
        [DisplayName("City name")]
        public int CityId { get; set; }
        public String Pictures { get; set; }
    }
}