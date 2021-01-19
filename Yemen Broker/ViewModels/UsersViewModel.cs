using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Yemen_Broker.Models;

namespace Yemen_Broker.ViewModels
{
    public class UsersViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public string SearchString { get; set; }
        public bool AddressSortParm { get; set; }
        public Pager Pager { get; set; }
    }
}