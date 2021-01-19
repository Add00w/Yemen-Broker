using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Yemen_Broker.Models;
using Yemen_Broker.ViewModels;

namespace Yemen_Broker.Controllers
{
    public class HomesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Homes
        public ActionResult Index(string SearchString, String city,string detailssystem, double from = 0.0, double to = 0.0, int page = 1)
        {
            var homes = db.Homes.OfType<Home>()
                .Where(w => w.Ad.User.Confirmed)
                .Include(l => l.Ad);
            ViewBag.Views = db.Views;
            ViewBag.Wishlists = db.Wishlists;
            if (!String.IsNullOrEmpty(SearchString))
            {
                homes = homes.Where(a => a.Ad.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdTitle.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (!String.IsNullOrEmpty(city))
            {
                homes = homes.Where(a => a.Ad.City.Name.ToUpper().Equals(city));
            }

            if (to >= from && to > 1 && from >= 0.0)
            {
                homes = homes.Where(a => a.Ad.AdPrice >= from && a.Ad.AdPrice <= to);

            }
            var cities = homes.Select(c => c.Ad.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(homes.Count(), page, pageSize);
            HomeIndexViewModel vModel = new HomeIndexViewModel()
            {
                Homes = homes.OrderBy(a => a.Ad.AdPrice).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City = city,
                DetailsSystem = detailssystem,
                From = from,
                To = to

            };
            return View(vModel);
        }

        // GET: Homes/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Home home = db.Homes.Find(id);
            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(), home.Ad.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(home.AdId, User.Identity.GetUserId()) != null;
            var adverRate = db.Ratings.Where(r => r.AdvertiserId.Equals(home.Ad.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(home.Ad.UserId))
                                .Select(r => r.RatingNumber)
                                .Average() : 0;
            ViewBag.ratersCount = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(home.Ad.UserId))
                                 .Count() : 0;
            ViewBag.Rated = rate != null;
            //end of rating
            if (home == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                String userId = User.Identity.GetUserId();
                bool isOwner = home.Ad.UserId.Equals(userId);
                int view = db.Views.Where(v => v.UserId.Equals(userId) && v.AdId == home.AdId).Count();

                if (!isOwner && view == 0)
                {
                    db.Views.Add(new View()
                    {
                        AdId = home.AdId,
                        UserId = userId,
                    });
                    db.SaveChanges();
                }
            }
            var views = db.Views.Where(h => h.AdId.Equals(home.AdId)).Count();

            ViewBag.views = views;
            //similar ads
            var similarHomes = db.Homes
                            .Where(c => c.Ad.Confirmed)
                            .OrderBy(c => home.NumberOfFloors)
                            .ThenBy(c => home.Ad.AdTitle)
                            .ThenBy(c => home.DetailSystem)
                            .ThenBy(c => c.Ad.User.DatePayingStarted)
                            .Take(4);
            ViewBag.similarHomes = similarHomes;
            return View(home);
        }

        // GET: Homes/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");

            return View();
        }

        // POST: Homes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(HomeViewModel homeVM,IEnumerable<HttpPostedFileBase> files)
        {
            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            if (!user.Confirmed)
            {
                ViewBag.NotConfirmed = "Sory! wait until the confirmation, thanks!.";
                ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", homeVM.CityId);

                return View(homeVM);
            }

            if (ModelState.IsValid)
            {
                if (!User.IsInRole("Admin"))
                {
                    //find user's listings and check if he advertised more than allowed size
                    int uLiveAdsCount = db.Ads.Where(ap => ap.UserId.Equals(uid)).Count();
                    ////find user's subscription size
                    int usubsId = db.Users.Where(u => u.Id.Equals(uid)).Select(usub => usub.SubscriptionId).FirstOrDefault();
                    int subsSize = db.Subscriptions.Where(s => s.SubscriptionId == usubsId).Select(sz => sz.SubscripsionSize).FirstOrDefault();

                    if (uLiveAdsCount >= subsSize)
                    {
                        ViewBag.LimitReached = "Sory! you have reached the limit of this subscription please remove some ads or upgrade your subscription, thanks!.";
                        ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", homeVM.CityId);

                        return View(homeVM);

                    }
                }
                //if user chooses no image show him error
                if (files == null || files.Count() <= 0 || files.FirstOrDefault() == null)
                {
                    ViewBag.chooseImage = "Please choose image";
                    ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", homeVM.CityId);
                    return View(homeVM);
                }
                //end
                var pictures = new List<Picture>();

                foreach (var file in files)
                {

                    if (file != null && file.ContentLength > 0)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));
                    }
                }



                var City = db.Cities.Find(homeVM.CityId);
                Ad Ad = new Ad()
                {
                    AdDescribtion = homeVM.AdDescribtion,
                    AdPrice = homeVM.AdPrice,
                    City = City,
                    AdTitle = homeVM.AdTitle,
                    Date = DateTime.Now.Date,
                    Discriminator = DiscriminatorOptions.Home,
                    Pictures = pictures,
                    UserId = User.Identity.GetUserId()
                };

                Home home = new Home()
                {
                   DetailSystem=homeVM.DetailSystem,
                   NumberOfFloors=homeVM.NumberOfFloors,
                   NumberOfLand=homeVM.NumberOfLand,
                   PlateNumber=homeVM.PlateNumber,
                   StreetsArea=homeVM.StreetsArea,
                    Ad = Ad
                };


                db.Homes.Add(home);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine("error:"+e.Message);
                    Console.WriteLine("trace:" + e.StackTrace);

                }


                return RedirectToAction("Index");
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name",homeVM.CityId);

            return View(homeVM);
        }

        // GET: Homes/Edit/5
        [Authorize]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Home home = db.Homes.Find(id);
            if (home == null)
            {
                return HttpNotFound();
            }
            HomeViewModel homeVM = new HomeViewModel
            {
                AdDescribtion=home.Ad.AdDescribtion,
                AdPrice=home.Ad.AdPrice,
                AdTitle=home.Ad.AdTitle,
                DetailSystem =home.DetailSystem,
                NumberOfFloors=home.NumberOfFloors,
                NumberOfLand=home.NumberOfLand,
                PlateNumber=home.PlateNumber,
                StreetsArea=home.StreetsArea,
                Id=home.AdId,
                CityId=home.Ad.CityId
            };
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", home.Ad.City.Name);

            return View(homeVM);
        }

        // POST: Homes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(HomeViewModel homeVM, IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(homeVM.CityId);

            if (ModelState.IsValid)
            {
                var home = db.Homes.Find(homeVM.Id);
                var pics = home.Ad.Pictures;

                var pictures = new List<Picture>();

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0 && files.FirstOrDefault() != null)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = home.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }
                



                home.Ad.AdDescribtion = homeVM.AdDescribtion;
                home.Ad.AdPrice = homeVM.AdPrice;
                home.Ad.AdTitle = homeVM.AdTitle;
                home.Ad.City = city;
                home.NumberOfFloors = homeVM.NumberOfFloors;
                home.StreetsArea = homeVM.StreetsArea;
                home.DetailSystem = homeVM.DetailSystem;
                home.NumberOfLand = homeVM.NumberOfLand;
                home.PlateNumber = homeVM.PlateNumber;
                home.AdId = homeVM.Id;
                if (pictures.Count() > 0)
                    home.Ad.Pictures.AddRange(pictures);




                db.Entry(home).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyAds", "Ads");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", homeVM.CityId);
            return View(homeVM);
        }

        // GET: Homes/Delete/5
        [Authorize]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Home home = db.Homes.Find(id);
            if (home == null)
            {
                return HttpNotFound();
            }
            return View(home);
        }

        // POST: Homes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(long id)
        {
            Home home = db.Homes.Find(id);
            Ad ad = home.Ad;
            var pics = ad.Pictures;
            db.Homes.Remove(home);
            if (pics != null)
            {
                foreach (Picture file in pics.ToList())
                {
                    //find old image's path
                    string OldPath = Path.Combine(Server.MapPath("/Uploads"), file.PictureURL);

                    //delete the old photo from our server
                    System.IO.File.Delete(OldPath);

                }
            }
            db.Ads.Remove(ad);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
