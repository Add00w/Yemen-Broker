using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Rating
    {
        public int RatingNumber { get; set; }
        [ForeignKey("Rater")]
        [Key,Column(Order=1)]
        public string RaterId { get; set; }
        public virtual User Rater { get; set; }

        [ForeignKey("Advertiser")]
        [Key, Column(Order =2)]
        public string AdvertiserId { get; set; }
        public virtual User Advertiser { get; set; }
    }
}