using System;
using System.ComponentModel;

namespace Yemen_Broker.ViewModels
{
    public class CityModel
    {
        public int Id { get; set; }
        [DisplayName("Ad Location")]
        public String Name { get; set; }

    }
}