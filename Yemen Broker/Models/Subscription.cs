using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Subscription
    {
        public Subscription()
        {
            Users = new HashSet<User>();
        }
        [Key]
        public int SubscriptionId { get; set; }

        [Required(ErrorMessage = "Please enter the  subscription type")]
        [StringLength(10, ErrorMessage = "Maximum length allowed is 10")]
        [DisplayName("Subscription Type")]
        public string SubscriptionType { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        [Column(TypeName = "money")]
        [DisplayName("Subscription Price")]
        public decimal SubscriptionPrice { get; set; }

        [StringLength(100, ErrorMessage = "Maximum length allowed is 100")]
        [DisplayName("Description")]
        public string SubscriptionDescription { get; set; }

        [DisplayName("Size")]
        public int SubscripsionSize { get; set; }


        [DisplayName("Period")]
        public int SubscriptionPeriod { get; set; }

        public virtual ICollection<User> Users { get; set; }

    }
}