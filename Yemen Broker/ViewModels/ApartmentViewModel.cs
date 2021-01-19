using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Yemen_Broker.ViewModels
{
    public class ApartmentViewModel
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

        [DisplayName("Floor number")]
        [Required]
        public int FloorNumber { get; set; }
        [DisplayName("Number of rooms")]
        [Required]
        public int NumberOfRooms { get; set; }
        [DisplayName("Number of doors")]
        [Required]
        public int NumberOfDoors { get; set; }
        [DisplayName("Number of bathrooms")]
        [Required]
        public int NumberOfBathrooms { get; set; }
        [DisplayName("Number of kitchens")]
        [Required]
        public int NumberOfKitchens { get; set; }
        [DisplayName("Type of finishing")]
        [Required]
        public string TypeOfFinishing { get; set; }
    }
}