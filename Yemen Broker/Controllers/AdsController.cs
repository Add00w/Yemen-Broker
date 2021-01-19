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

namespace Yemen_Broker.Areas.Dashboard.Controllers
{
   
    public class AdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Ads
        public ActionResult Index(string SearchString, String city, double from = 0.0, double to = 0.0, int page = 1)
        {
            var ads = db.Ads.OfType<Ad>().Where(w =>w.Discriminator==DiscriminatorOptions.Ad && w.User.Confirmed);
          
            ViewBag.Views = db.Views;
            ViewBag.Wishlists = db.Wishlists;
            if (!String.IsNullOrEmpty(SearchString))
            {
                ads = ads.Where(a => a.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.AdTitle.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (!String.IsNullOrEmpty(city))
            {
                ads = ads.Where(a => a.City.Name.ToUpper().Equals(city));
            }

            if (to >= from && to > 1 && from >= 0.0)
            {
                ads = ads.Where(a => a.AdPrice >= from && a.AdPrice <= to);

            }
            var cities = ads.Select(c => c.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(ads.Count(), page, pageSize);
            AdIndexViewModel vModel = new AdIndexViewModel()
            {
                Ads = ads.OrderBy(a => a.AdPrice).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City = city,
                From = from,
                To = to

            };
           
            return View(vModel);

        }
        // Get: Ads/MyAds
        [Authorize()]
        public ActionResult MyAds(string SearchString, string sortBy, int page = 1)
        {
            string uId = User.Identity.GetUserId();
            string adminId = db.Users.Where(u => u.UserType.Equals("Admin")).Select(u => u.Id).FirstOrDefault();
            ViewBag.adminId = adminId;
            var Ads = db.Ads.OfType<Ad>().Where(ad => ad.UserId.Equals(uId));
            if (!String.IsNullOrEmpty(SearchString))
            {
                Ads = Ads.Where(a => a.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.AdTitle.ToUpper().Contains(SearchString.ToUpper()));
            }

            switch (sortBy)
            {
                case "Active":
                    Ads = Ads.Where(a=>a.Confirmed);
                    break;
                case "Inactive":
                    Ads = Ads.Where(a => !a.Confirmed);
                    break;
                default:
                    break;
            }

            //pagination steps
            int pageSize = 6;
            var pager = new Pager(Ads.Count(), page, pageSize);
            MyAdsViewModel MyAds = new MyAdsViewModel()
            {
                Ads = Ads.OrderBy(a=>a.Date).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize).ToList(),
                Pager=pager,
                SearchString=SearchString,
                

            };
            MyAds.Sorts = new Dictionary<string, string>
            {
                {"Active", "Active" },
                {"Inactive", "Inactive" }
            };
            return View(MyAds);
        }

        // Get: Ads/MyAds
        [Authorize(Roles ="Admin")]
        public ActionResult ManageAds(string SearchString, int page = 1)
        {
            //string uId = User.Identity.GetUserId();
            ViewBag.Views = db.Views;
            var Ads = db.Ads.OfType<Ad>().Where(ad => !ad.Confirmed);
            if (!String.IsNullOrEmpty(SearchString))
            {
                Ads = Ads.Where(a => a.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.AdTitle.ToUpper().Contains(SearchString.ToUpper()));
            }

            //pagination steps
            int pageSize = 6;
            var pager = new Pager(Ads.Count(), page, pageSize);
            MyAdsViewModel MyAds = new MyAdsViewModel()
            {
                Ads = Ads.OrderBy(a => a.Date).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize).ToList(),
                Pager = pager,
                SearchString = SearchString,


            };
         
            return View(MyAds);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ConfirmOrReject(long id=0, bool isReject=false,bool isConfirm=false)
        {
            if(id <= 0 ) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Ad ad = db.Ads.Find(id);
            if (isConfirm)
            {
               ad.Confirmed = true;
                ad.Rejected = false;

                db.Entry(ad).State = EntityState.Modified;
                db.SaveChanges();
            }
            else if (isReject)
            {
                //Session.Add("rejectedAdId", id);
                ViewBag.rejectedAdId = id;
                return View("ConfirmReject", ad);
            }
            return RedirectToAction("ManageAds");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ConfirmOrReject(String message,long id)
        {
            //var adId =long.Parse(Session.Contents["rejectedAdId"].ToString());
            Ad ad = db.Ads.Find(id);
            //here send message to the user and turn rejected to true
            Message Message = new Message()
            {
                SenderId = User.Identity.GetUserId(),
                IsMessage = false,
                MessageDateTime = DateTime.Now,
                MessageContent = message,
                RecieverId =ad.UserId,
            };
            ad.Rejected = true;
            db.Entry(ad).State = EntityState.Modified;
            db.Messages.Add(Message);
            db.SaveChanges();
            return RedirectToAction("ManageAds");

        }




        public ActionResult Properties()
        {
            return View(db.Ads.Where(w => w.User.Confirmed).ToList());
           
        }

        // GET: Dashboard/Ads/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ad ad = db.Ads.Find(id);
            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(), ad.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(ad.AdId, User.Identity.GetUserId()) != null;
            var adverRate =  db.Ratings.Where(r => r.AdvertiserId.Equals(ad.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() >0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(ad.UserId))
                                .Select(r => r.RatingNumber)
                                .Average():0;
            ViewBag.ratersCount = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(ad.UserId))
                                 .Count() : 0;
            ViewBag.Rated = rate != null;
            //end of rating
            if (ad == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                String userId = User.Identity.GetUserId();
                bool isOwner = ad.UserId.Equals(userId);
                int view = db.Views.Where(v => v.UserId.Equals(userId) && v.AdId == ad.AdId).Count();

                if (!isOwner && view == 0)
                {
                    db.Views.Add(new View()
                    {
                        AdId = ad.AdId,
                        UserId = userId,
                    });
                    db.SaveChanges();
                }
            }
            var views = db.Views.Where(w => w.AdId.Equals(ad.AdId)).Count();

            ViewBag.views = views;
            //similar ads
            var similarAds = db.Ads
                            .Where(c => c.Confirmed)
                            .OrderBy(c => ad.City.Name)
                            .ThenBy(c => ad.AdTitle)
                            .ThenBy(c => c.User.DatePayingStarted)
                            .Take(4);
            ViewBag.similarAds = similarAds;
            return View(ad);
        }

        // GET: Ads/Create
        [Authorize()]
        public ActionResult Create()
        {
            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            //assign user.confirmed value to viewbag
                ViewBag.Confirmed = user.Confirmed;

            
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");

            return View();
        }

        // POST: Ads/Create
        [Authorize()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ad ad, IEnumerable<HttpPostedFileBase> files)
        {
            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            ViewBag.Confirmed = user.Confirmed;

            if (!user.Confirmed)
            {
                ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ad.CityId);

                return View(ad);
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
                        ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ad.CityId);

                        return View(ad);

                    }
                }
                //if user chooses no image show him error
                if (files == null || files.Count() <= 0 || files.FirstOrDefault() == null)
                {
                    ViewBag.chooseImage = "Please choose image";
                    ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ad.CityId);
                    return View(ad);
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
                ad.UserId = User.Identity.GetUserId();
                ad.Date = DateTime.Now;
                //var City = db.Cities.Find(ad.City);
                ad.Pictures = pictures;
                db.Ads.Add(ad);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ad);
        }

        // GET: Ads/Edit/5
        [Authorize()]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ad ad = db.Ads.Find(id);
            if (ad == null)
            {
                return HttpNotFound();
            }

            switch (ad.Discriminator)
            {
                case DiscriminatorOptions.Shop:
                    return RedirectToAction("Edit", "Shops", new { id });
                case DiscriminatorOptions.Home:
                    return RedirectToAction("Edit", "Homes", new { id });
                case DiscriminatorOptions.Land:
                    return RedirectToAction("Edit", "Lands", new { id });
                case DiscriminatorOptions.Apartment:
                    return RedirectToAction("Edit", "Apartments", new { id });

                case DiscriminatorOptions.Car:
                    return RedirectToAction("Edit", "Cars", new { id });
                case DiscriminatorOptions.Computer:
                    return RedirectToAction("Edit", "Computers", new { id });
                case DiscriminatorOptions.Mobile:
                    return RedirectToAction("Edit", "Mobiles", new { id });
                case DiscriminatorOptions.Warehouse:
                    return RedirectToAction("Edit", "Warehouses", new { id });
                case DiscriminatorOptions.Ad:
                default:
                    break;
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ad.City.Name);

            return View(ad);
        }

        // POST: Ads/Edit/5
        [Authorize()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Ad ad,IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(ad.CityId);
            if (ModelState.IsValid)
            {
                var Ad = db.Ads.Find(ad.AdId);
                var pics = Ad.Pictures;

                var pictures = new List<Picture>();

                if (files != null && files.Count() > 0 && files.FirstOrDefault() != null)
                {
                    foreach (var file in files)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = Ad.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }
                Ad.AdDescribtion = ad.AdDescribtion;
                Ad.AdPrice = ad.AdPrice;
                Ad.AdTitle = ad.AdTitle;
                Ad.City = city;
                if (pictures.Count() > 0)
                    Ad.Pictures.AddRange(pictures);
                db.Entry(ad).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("MyAds");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ad.CityId);
            return View(ad);
        }

        // GET: Ads/Delete/5
        [Authorize()]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ad ad = db.Ads.Find(id);
            if (ad == null)
            {
                return HttpNotFound();
            }
            return View(ad);
        }

        // POST: Ads/Delete/5
        [Authorize()]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
           

            Ad ad = db.Ads.Find(id);
            var pics = ad.Pictures;
            if (ad.Discriminator == DiscriminatorOptions.Home)
            {
                Home home = db.Homes.Find(id);
                db.Homes.Remove(home);
            }
            else if(ad.Discriminator == DiscriminatorOptions.Car) {
                Car car = db.Cars.Find(id);
                db.Cars.Remove(car);
            }
            else if (ad.Discriminator == DiscriminatorOptions.Warehouse)
            {
                Warehouse warehouse = db.Warehouses.Find(id);
                db.Warehouses.Remove(warehouse);
            }
            else if (ad.Discriminator == DiscriminatorOptions.Shop)
            {
                Shop shop = db.Shops.Find(id);
                db.Shops.Remove(shop);
            }




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


           

            return RedirectToAction("MyAds");
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
