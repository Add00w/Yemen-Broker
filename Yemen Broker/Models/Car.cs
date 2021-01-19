using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Car
    {
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

        [ForeignKey("Ad")]
        [Key]
        public long AdId { set; get; }
        public virtual Ad Ad { set; get; }

    }
}