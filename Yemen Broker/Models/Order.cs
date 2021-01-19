using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Yemen_Broker.ViewModels;

namespace Yemen_Broker.Models
{
    public class Order
    {
        [Key]
        public long Order_id { get; set; }
        [DisplayName("Order Description")]
        [Required]
        public string Order_description { get; set; }
        [DisplayName("Order Title")]
        [Required]
        public string Order_title { get; set; }
        [DisplayName("Order Date")]
        [Required]
        public DateTime Order_date { get; set; }

        [ForeignKey("City")]
        public int CityId { get; set; }
        public virtual CityModel City { get; set; }

        [DefaultValue(false)]
        public bool Confirmed { get; set; }
        [DefaultValue(false)]
        public bool Rejected { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}