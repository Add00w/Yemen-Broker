using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yemen_Broker.ViewModels;

namespace Yemen_Broker.Models
{
    public class Ad
    {
        public Ad()
        {
            Pictures = new List<Picture>();
        }
        [Key]
        public long AdId { set; get; }
        [DisplayName("Price")]
        [Required]
        public double AdPrice { set; get; }
        [DisplayName("Description")]
        [Required]
        public String AdDescribtion { set; get; }
        [DisplayName("Title")]
        [Required]
        public String AdTitle { set; get; }
        public DateTime Date { get; set; }
        public DiscriminatorOptions Discriminator { get; set; }
        [ForeignKey("City")]
        public int CityId { get; set; }
        public virtual CityModel City { get; set; }
        public virtual List<Picture> Pictures { get; set; }
        [DefaultValue(false)]
        public bool Confirmed { get; set; }
        [DefaultValue(false)]
        public bool Rejected { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }

    }
}