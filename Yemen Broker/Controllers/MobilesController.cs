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
    public class MobilesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Mobiles
        public ActionResult Index(string SearchString, String city, string status, double from = 0.0, double to = 0.0, int page = 1)
        {
            var mobiles = db.Mobiles.OfType<Mobile>()
                .Where(w => w.Ad.User.Confirmed)
                .Include(l => l.Ad);
            ViewBag.Views = db.Views;
            ViewBag.Wishlists = db.Wishlists;
            if (!String.IsNullOrEmpty(SearchString))
            {
                mobiles = mobiles.Where(a => a.Ad.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.MobileCompany.ToUpper().Contains(SearchString.ToUpper())
                || a.MobileOS.ToUpper().Contains(SearchString.ToUpper())
                || a.MobileColor.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (!String.IsNullOrEmpty(city))
            {
                mobiles = mobiles.Where(a => a.Ad.City.Name.ToUpper().Equals(city));
            }

            if (to >= from && to > 1 && from >= 0.0)
            {
                mobiles = mobiles.Where(a => a.Ad.AdPrice >= from && a.Ad.AdPrice <= to);

            }
            var cities = mobiles.Select(c => c.Ad.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(mobiles.Count(), page, pageSize);
            MobileIndexViewModel vModel = new MobileIndexViewModel()
            {
                Mobiles = mobiles.OrderBy(a => a.Ad.AdPrice).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City = city,
                Status = status,
                From = from,
                To = to

            };
            return View(vModel);
        }

        // GET: Mobiles/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mobile mobile = db.Mobiles.Find(id);
            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(), mobile.Ad.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(mobile.AdId, User.Identity.GetUserId()) != null;
            var adverRate = db.Ratings.Where(r => r.AdvertiserId.Equals(mobile.Ad.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(mobile.Ad.UserId))
                                .Select(r => r.RatingNumber)
                                .Average() : 0;
            ViewBag.ratersCount = adverRate.Count() > 0 ?
                               db.Ratings.Where(r => r.AdvertiserId.Equals(mobile.Ad.UserId))
                               .Count() : 0;
            ViewBag.Rated = rate != null;
            //end of rating
            if (mobile == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                String userId = User.Identity.GetUserId();
                bool isOwner = mobile.Ad.UserId.Equals(userId);
                int view = db.Views.Where(v => v.UserId.Equals(userId) && v.AdId == mobile.AdId).Count();

                if (!isOwner && view == 0)
                {
                    db.Views.Add(new View()
                    {
                        AdId = mobile.AdId,
                        UserId = userId,
                    });
                    db.SaveChanges();
                }
            }
            var views = db.Views.Where(m => m.AdId.Equals(mobile.AdId)).Count();

            ViewBag.views = views;
            //similar ads
            var similarMobiles = db.Mobiles
                            .Where(c => c.Ad.Confirmed)
                            .OrderBy(c => mobile.MobileCompany)
                            .ThenBy(c => mobile.Ad.AdTitle)
                            .ThenBy(c => mobile.MobileOS)
                            .ThenBy(c => c.Ad.User.DatePayingStarted)
                            .Take(4);
            ViewBag.similarMobiles = similarMobiles;
            return View(mobile);
        }

        // GET: Mobiles/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");
            return View();
        }

        // POST: Mobiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(MobileViewModel MobileVM, IEnumerable<HttpPostedFileBase> files)
        {
            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            if (!user.Confirmed)
            {
                ViewBag.NotConfirmed = "Sory! wait until the confirmation, thanks!.";
                ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", MobileVM.CityId);

                return View(MobileVM);
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
                        ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", MobileVM.CityId);

                        return View(MobileVM);

                    }
                }
                //if user chooses no image show him error
                if (files == null || files.Count() <= 0 || files.FirstOrDefault() == null)
                {
                    ViewBag.chooseImage = "Please choose image";
                    ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", MobileVM.CityId);
                    return View(MobileVM);
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



                var City = db.Cities.Find(MobileVM.CityId);
                Ad Ad = new Ad()
                {
                    AdTitle = MobileVM.AdTitle,
                    AdDescribtion = MobileVM.AdDescribtion,
                    AdPrice = MobileVM.AdPrice,
                    City = City,
                    Discriminator = DiscriminatorOptions.Mobile,
                    Pictures = pictures,
                    UserId = User.Identity.GetUserId(),
                    Date = DateTime.Now.Date,
                };
                Mobile Mobile = new Mobile()
                {
                    Ad = Ad,
                    MobileCompany = MobileVM.MobileCompany,
                    MobileRam = MobileVM.MobileRam,
                    MobileStorage = MobileVM.MobileStorage,
                    MobileScreenSize = MobileVM.MobileScreenSize,
                    MobileCpu = MobileVM.MobileCpu,
                    MobileOS = MobileVM.MobileOS,
                    MobileColor = MobileVM.MobileColor,
                    MobileBattery = MobileVM.MobileBattery,
                    MobileStatus = MobileVM.MobileStatus,
                    CDMA_GSM = MobileVM.CDMA_GSM,
                    MobileCamera = MobileVM.MobileCamera,
                    OTG = MobileVM.OTG,
                };

                db.Mobiles.Add(Mobile);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", MobileVM.CityId);
            return View(MobileVM);
        }

        // GET: Mobiles/Edit/5
        [Authorize]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mobile mobile = db.Mobiles.Find(id);
            if (mobile == null)
            {
                return HttpNotFound();
            }
            MobileViewModel MobileVM = new MobileViewModel
            {
                AdDescribtion = mobile.Ad.AdDescribtion,
                AdPrice = mobile.Ad.AdPrice,
                Id = mobile.AdId,
                CityId = mobile.Ad.CityId,

                MobileCompany = mobile.MobileCompany,
                MobileRam = mobile.MobileRam,
                MobileStorage = mobile.MobileStorage,
                MobileScreenSize = mobile.MobileScreenSize,
                MobileCpu = mobile.MobileCpu,
                MobileOS = mobile.MobileOS,
                MobileColor = mobile.MobileColor,
                MobileBattery = mobile.MobileBattery,
                MobileStatus = mobile.MobileStatus,
                CDMA_GSM = mobile.CDMA_GSM,
                MobileCamera = mobile.MobileCamera,
                OTG = mobile.OTG,
                AdTitle=mobile.Ad.AdTitle,
                Date=mobile.Ad.Date
            };
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", mobile.Ad.City.Name);

            return View(MobileVM);
        }

        // POST: Mobiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(MobileViewModel MobileVM, IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(MobileVM.CityId);

            if (ModelState.IsValid)
            {
                var Mobile = db.Mobiles.Find(MobileVM.Id);
                var pics = Mobile.Ad.Pictures;

                var pictures = new List<Picture>();

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0 && files.FirstOrDefault() != null)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = Mobile.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }




                Mobile.Ad.AdDescribtion = MobileVM.AdDescribtion;
                Mobile.Ad.AdTitle = MobileVM.AdTitle;
                Mobile.Ad.AdPrice = MobileVM.AdPrice;
                Mobile.Ad.City = city;
                Mobile.AdId = MobileVM.Id;

                Mobile.MobileCompany = MobileVM.MobileCompany;
                Mobile.MobileRam = MobileVM.MobileRam;
                Mobile.MobileStorage = MobileVM.MobileStorage;
                Mobile.MobileScreenSize = MobileVM.MobileScreenSize;
                Mobile.MobileCpu = MobileVM.MobileCpu;
                Mobile.MobileOS = MobileVM.MobileOS;
                Mobile.MobileColor = MobileVM.MobileColor;
                Mobile.MobileBattery = MobileVM.MobileBattery;
                Mobile.MobileStatus = MobileVM.MobileStatus;
                Mobile.CDMA_GSM = MobileVM.CDMA_GSM;
                Mobile.MobileCamera = MobileVM.MobileCamera;
                Mobile.OTG = MobileVM.OTG;

                if (pictures.Count() > 0)
                    Mobile.Ad.Pictures.AddRange(pictures);




                db.Entry(Mobile).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyAds", "Ads");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", MobileVM.CityId);
            return View(MobileVM);
        }

        // GET: Mobiles/Delete/5
        [Authorize]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mobile mobile = db.Mobiles.Find(id);
            if (mobile == null)
            {
                return HttpNotFound();
            }
            return View(mobile);
        }

        // POST: Mobiles/Delete/5
        [Authorize()]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Ad ad = db.Ads.Find(id);
            var pics = ad.Pictures;

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
