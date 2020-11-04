using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Land
    {
        [DisplayName("Number of land")]
        [Required]
        public int NumberOfLand { get; set; }
        [DisplayName("Plate number")]
        [Required]
        public int PlateNumber { get; set; }
        [DisplayName("Streat area")]
        [Required]
        public string StreetsArea { get; set; }

        [ForeignKey("Ad")]
        [Key]
        public long AdId { set; get; }
        public virtual Ad Ad { set; get; }
    }
}
