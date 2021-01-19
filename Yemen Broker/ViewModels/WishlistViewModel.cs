using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Yemen_Broker.Models;

namespace Yemen_Broker.ViewModels
{
    public class WishlistViewModel
    {
        public List<Wishlist> Wishlists { get; set; }
        public string SearchString { get; set; }
        public Pager Pager { get; set; }
    }
}