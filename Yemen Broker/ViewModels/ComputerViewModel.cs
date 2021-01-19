using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Yemen_Broker.ViewModels
{
    public class ComputerViewModel
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

        [DisplayName("Computer Company")]
        [Required]
        public string ComputerCompany { get; set; }
        [DisplayName("Computer Ram")]
        public string ComputerRam { get; set; }
        [DisplayName("Computer Storage")]
        public string ComputerStorage { get; set; }
        [DisplayName("Computer Screen Size")]
        public string ComputerScreenSize { get; set; }
        [DisplayName("Computer Cpu")]
        public string ComputerCpu { get; set; }
        [DisplayName("Computer OS")]
        public string ComputerOS { get; set; }
        [DisplayName("Computer Color")]
        public string ComputerColor { get; set; }
        [DisplayName("Computer Battery")]
        public string ComputerBattery { get; set; }
        [DisplayName("Computer Status")]
        public string ComputerStatus { get; set; }
        [DisplayName("CD Drive")]
        public Boolean CD_Drive { get; set; }
        [DisplayName("Computer Screen Card")]
        public string ComputerScreenCard { get; set; }

    }
}