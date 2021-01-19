using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    public class Computer
    {
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

        [ForeignKey("Ad")]
        [Key]
        public long AdId { set; get; }
        public virtual Ad Ad { set; get; }
    }
}