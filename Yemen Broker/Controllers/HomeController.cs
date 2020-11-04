using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yemen_Broker.Models;
using Yemen_Broker.ViewModels;

namespace Yemen_Broker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var ads = db.Ads.Where(ad => ad.Discriminator == DiscriminatorOptions.Apartment).Take(4).ToList();

            return View(ads);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}