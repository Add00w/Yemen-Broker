using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yemen_Broker.Models;

namespace Yemen_Broker.Controllers
{
    public class RatingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Ratings
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public ActionResult Rate(int rateNumber,string advertiserId,string returnUrl)
        {
           
            Rating rate = new Rating()
            {
                RaterId = User.Identity.GetUserId(),
                AdvertiserId=advertiserId,
                RatingNumber=rateNumber
            };
            db.Ratings.Add(rate);
            db.SaveChanges();
            return Redirect(returnUrl);
        }
    }
}