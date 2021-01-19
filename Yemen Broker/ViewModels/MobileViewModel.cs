using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Yemen_Broker.ViewModels
{
    public class MobileViewModel
    {
        public long Id { get; set; }
        [DisplayName("Price")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        [Required]
        public double AdPrice { set; get; }
        [DisplayName("Title")]
        [Required]
        public String AdTitle { set; get; }
        [DisplayName("Description")]
        [Required]
        public String AdDescribtion { set; get; }
        [DisplayName("City name")]
        public int CityId { get; set; }
        public String Pictures { get; set; }
        public DateTime Date { get; set; }

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
    }

}