using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Mobile
    {
        [DisplayName("Mobile Company")]
        [Required]
        public string MobileCompany { get; set; }
        [DisplayName("Mobile Ram")]
        public string MobileRam { get; set; }
        [DisplayName("Mobile Storage")]
        public string MobileStorage { get; set; }
        [DisplayName("Mobile Screen Size")]
        public string MobileScreenSize { get; set; }
        [DisplayName("Mobile Cpu")]
        public string MobileCpu { get; set; }
        [DisplayName("Mobile OS")]
        public string MobileOS { get; set; }
        [DisplayName("Mobile Color")]
        public string MobileColor { get; set; }
        [DisplayName("Mobile Battery")]
        public string MobileBattery { get; set; }
        [DisplayName("Mobile Status")]
        public string MobileStatus { get; set; }
        [DisplayName("CDMA or GSM")]
        public string CDMA_GSM { get; set; }
        [DisplayName("Mobile Camera")]
        public string MobileCamera { get; set; }
        [DisplayName("OTG")]
        public bool OTG { get; set; }

        [ForeignKey("Ad")]
        [Key]
        public long AdId { set; get; }
        public virtual Ad Ad { set; get; }
    }
}