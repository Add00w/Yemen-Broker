using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Yemen_Broker.Models
{
    
        [Table("Picture")]
        public class Picture
        {
            [Key]
            [DisplayName("Pictures")]
            public int PictureId { get; set; }

            [Column("Picture")]
            public string PictureURL { get; set; }

            [ForeignKey("Ad")]
            public long AdId { get; set; }
        public virtual Ad Ad { get; set; }
    }
}