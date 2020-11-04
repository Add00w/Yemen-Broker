using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yemen_Broker.Models
{
    public class Shop
    {
        [DisplayName("Number of doors")]
        public int NumberOfDoors { set; get; }

        [DisplayName("Streat area")]
        [Required]
        public String StreetArea { set; get; }

        [ForeignKey("Ad")]
        [Key]
        public long AdId { set; get; }
        public virtual Ad Ad { set; get; }
    }
}