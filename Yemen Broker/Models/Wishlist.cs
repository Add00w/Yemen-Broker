using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Wishlist
    {
        [ForeignKey("Ad")]
        [Key, Column(Order = 1)]
        public long AdId { get; set; }
        public string Note { get; set; }
        [ForeignKey("User")]
        [Key, Column(Order = 2)]
        public String UserId { get; set; }

        public virtual User User { get; set; }
        public virtual Ad Ad { get; set; }

    }
}