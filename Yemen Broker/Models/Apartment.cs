using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Apartment
    {
        [DisplayName("Floor number")]
        [Required]
        public int FloorNumber { get; set; }
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
        [DisplayName("Number of rooms")]
        [Required]
        public int NumberOfRooms { get; set; }
        [ForeignKey("Ad")]
        [Key]
        public long AdId { set; get; }
        public virtual Ad Ad { set; get; }

    }
}