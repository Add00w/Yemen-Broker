using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Message
    {
        [ForeignKey("User")]
        [Key,Column(Order =1)]
        public string SenderId { get; set; }

        [ForeignKey("Advertiser")]
        [Key, Column(Order = 2)]
        public string RecieverId { get; set; }
        public string MessageContent { get; set; }
        public bool IsMessage { get; set; }
        [Key, Column(Order = 3)]
        public DateTime MessageDateTime { get; set; }
        public virtual User User { get; set; }
        public virtual User Advertiser { get; set; }
    }
}