using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Yemen_Broker.Models;

namespace Yemen_Broker.ViewModels
{
    public class IndividualUserReportViewModel
    {
        public int AdsCount { get; set; }
        public int OrdersCount { get; set; }
        public User User { get; set; }
    }
}