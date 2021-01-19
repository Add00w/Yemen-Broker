using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Yemen_Broker.Models;

namespace Yemen_Broker.ViewModels
{
    public class MyOrdersViewModel
    {
        public List<Order> Orders { get; set; }
        public string SearchString { get; set; }
        public Pager Pager { get; set; }


        public string SortBy { get; set; }
        public Dictionary<string, string> Sorts { get; set; }
    }
}