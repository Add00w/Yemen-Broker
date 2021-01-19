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
    public class ShopsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard/Shops
        public ActionResult Index(string SearchString, String city, double from = 0.0, double to = 0.0, int page = 1,int numberofDoor=1)
        {
            var shops = db.Shops.OfType<Shop>()
                .Where(w => w.Ad.User.Confirmed)
                .Include(l => l.Ad);
            ViewBag.Views = db.Views;
            ViewBag.Wishlists = db.Wishlists;
            if (!String.IsNullOrEmpty(SearchString))
            {
                shops = shops.Where(a => a.Ad.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdTitle.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (!String.IsNullOrEmpty(city))
            {
                shops = shops.Where(a => a.Ad.City.Name.ToUpper().Equals(city));
            }

            if (to >= from && to > 1 && from >= 0.0)
            {
                shops = shops.Where(a => a.Ad.AdPrice >= from && a.Ad.AdPrice <= to);

            }
            var cities = shops.Select(c => c.Ad.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(shops.Count(), page, pageSize);
            ShopIndexViewModel vModel = new ShopIndexViewModel()
            {
                Shops = shops.OrderBy(a => a.Ad.AdPrice).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City = city,
                NumberofDoor=numberofDoor,
                From = from,
                To = to

            };
            return View(vModel);
        }

        // GET: Dashboard/Shops/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Shop shop = db.Shops.Find(id);
            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(), shop.Ad.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(shop.AdId, User.Identity.GetUserId()) != null;
            var adverRate = db.Ratings.Where(r => r.AdvertiserId.Equals(shop.Ad.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(shop.Ad.UserId))
                                .Select(r => r.RatingNumber)
                                .Average() : 0;
            ViewBag.ratersCount = adverRate.Count() > 0 ?
                               db.Ratings.Where(r => r.AdvertiserId.Equals(shop.Ad.UserId))
                               .Count() : 0;
            ViewBag.Rated = rate != null;
            //end of rating
            if (shop == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                String userId = User.Identity.GetUserId();
                bool isOwner = shop.Ad.UserId.Equals(userId);
                int view = db.Views.Where(v => v.UserId.Equals(userId) && v.AdId == shop.AdId).Count();

                if (!isOwner && view == 0)
                {
                    db.Views.Add(new View()
                    {
                        AdId = shop.AdId,
                        UserId = userId,
                    });
                    db.SaveChanges();
                }
            }
            var views = db.Views.Where(s => s.AdId.Equals(shop.AdId)).Count();

            ViewBag.views = views;     
            //similar ads
            var similarShops = db.Shops
                            .Where(c => c.Ad.Confirmed)
                            .OrderBy(c => shop.Ad.City.Name)
                            .ThenBy(c => shop.Ad.AdTitle)
                            .ThenBy(c => shop.NumberOfDoors)
                            .ThenBy(c => c.Ad.User.DatePayingStarted)
                            .Take(4);
            ViewBag.similarShops = similarShops;
            return View(shop);
        }

        // GET: Dashboard/Shops/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id","Name");
            return View();
        }

        // POST: Dashboard/Shops/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create( ShopModel shopVM, IEnumerable<HttpPostedFileBase> files)
        {
            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            if (!user.Confirmed)
            {
                ViewBag.NotConfirmed = "Sory! wait until the confirmation, thanks!.";
                ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", shopVM.CityId);

                return View(shopVM);
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
                        ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", shopVM.CityId);

                        return View(shopVM);

                    }
                }
               
                //if user chooses no image show him error
                if (files == null || files.Count() <= 0 || files.FirstOrDefault() == null)
                {
                    ViewBag.chooseImage = "Please choose image";
                    ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", shopVM.CityId);
                    return View(shopVM);
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

                

                var City = db.Cities.Find(shopVM.CityId);
                Ad Ad = new Ad()
                {
                    AdDescribtion = shopVM.AdDescribtion,
                    AdPrice = shopVM.AdPrice,
                    AdTitle = shopVM.AdTitle,
                    Date = DateTime.Now.Date,
                    City=City,
                    Discriminator = DiscriminatorOptions.Shop,
                    Pictures=pictures,
                    UserId= User.Identity.GetUserId()
            };

                Shop shop = new Shop()
                {
                    NumberOfDoors = shopVM.NumberOfDoors,
                    StreetArea = shopVM.StreetArea,
                    Ad = Ad
                };
                db.Shops.Add(shop);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id","Name", shopVM.CityId);
            
            return View(shopVM);
        }

        // GET: Dashboard/Shops/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Shop shop = db.Shops.Find(id);
            if (shop == null)
            {
                return HttpNotFound();
            }
            ShopModel shopModel = new ShopModel
            {
                AdDescribtion = shop.Ad.AdDescribtion,
                AdPrice = shop.Ad.AdPrice,
                AdTitle=shop.Ad.AdTitle,
                NumberOfDoors = shop.NumberOfDoors,
                StreetArea=shop.StreetArea,
                Id=shop.AdId,
                CityId=shop.Ad.City.Id
            };
            
            ViewBag.CityId = new SelectList(db.Cities, "Id","Name", shop.Ad.City.Name);
           
            return View(shopModel);
        }

        // POST: Dashboard/Shops/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( ShopModel shopVM , IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(shopVM.CityId);
            if (ModelState.IsValid)
            {
                var shop = db.Shops.Find(shopVM.Id);
                var pics =shop.Ad.Pictures;

                var pictures = new List<Picture>();

                if (files != null && files.Count()> 0 && files.FirstOrDefault() != null)
                {
                    foreach (var file in files)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = shop.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }


                
                shop.Ad.AdDescribtion = shopVM.AdDescribtion;
                shop.Ad.AdPrice = shopVM.AdPrice;
                shop.Ad.AdTitle = shopVM.AdTitle;
                shop.Ad.City= city;
                shop.NumberOfDoors = shopVM.NumberOfDoors;
                shop.StreetArea = shopVM.StreetArea;
                if(pictures.Count()>0)
                shop.Ad.Pictures.AddRange(pictures);
                db.Entry(shop).State = EntityState.Modified;
                db.SaveChanges();


                return RedirectToAction("MyAds", "Ads");
            }
           
            ViewBag.CityId = new SelectList(db.Cities, "Id","Name", shopVM.CityId);
            return View(shopVM);
        }

        // GET: Dashboard/Shops/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Shop shop = db.Shops.Find(id);
            if (shop == null)
            {
                return HttpNotFound();
            }
            return View(shop);
        }

        // POST: Dashboard/Shops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Shop shop = db.Shops.Find(id);
            Ad ad = shop.Ad;
            db.Shops.Remove(shop);
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
