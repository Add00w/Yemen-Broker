using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Warehouse
    {
        [Display(Name ="Number of doors")]
        public int NumberOfDoors { get; set; }
        [Display(Name = "Height of Wall")]
        public int HeightOfWall { get; set; }
        [Display(Name = "Street area")]
        public string StreetArea { get; set; }
        [ForeignKey("Ad")]
        [Key]
        public long AdId { set; get; }
        public virtual Ad Ad { set; get; }
    }
}