using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Home
    {
        public int NumberOfFloors { get; set; }
        [Required]
        public string DetailSystem { get; set; }

        public int NumberOfLand { get; set; }

        public int PlateNumber { get; set; }
        [Required]
        public string StreetsArea { get; set; }

        [ForeignKey("Ad")]
        [Key]
        public long AdId { set; get; }
        public virtual Ad Ad { set; get; }
    }
}