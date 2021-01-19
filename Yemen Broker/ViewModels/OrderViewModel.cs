using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Yemen_Broker.ViewModels
{
    public class OrderViewModel
    {
        
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

        [DisplayName("City name")]
        public int CityId { get; set; }
    }
}