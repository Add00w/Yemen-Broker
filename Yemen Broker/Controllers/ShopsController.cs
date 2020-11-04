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
    [Authorize()]
    public class ShopsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard/Shops
        public ActionResult Index()
        {
            var shops = db.Shops.Include(s => s.Ad);
            return View(shops.ToList());
        }

        // GET: Dashboard/Shops/Details/5
        public ActionResult Details(long? id)
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

        // GET: Dashboard/Shops/Create
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
        public ActionResult Create( ShopModel shopVM, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                //find user's listings and check if he advertised more than allowed size
                //var uid = User.Identity.GetUserId();
                //int uAppartmentsCount = db.Appartments.Where(ap => ap.AdvertiserId.Equals(uid)).Count();
                //int uShopsCount = db.Shops.Where(sh => sh.AdvertiserId.Equals(uid)).Count();
                //int uHomesCount = db.Homes.Where(ap => ap.AdvertiserId.Equals(uid)).Count();
                //int uWarehousesCount = db.Warehouses.Where(ap => ap.AdvertiserId.Equals(uid)).Count();
                //int uLandsCount = db.Lands.Where(ap => ap.AdvertiserId.Equals(uid)).Count();
                //int totalAdvertised = uAppartmentsCount + uShopsCount + uHomesCount + uWarehousesCount + uLandsCount;
                ////find user's subscription size
                //int usubsId = db.Users.Where(u => u.Id.Equals(uid)).Select(usub => usub.SubscriptionId).FirstOrDefault();
                //int subsSize = db.Subscriptions.Where(s => s.SubscriptionId == usubsId).Select(sz => sz.SubscripsionSize).FirstOrDefault();


                //if (totalAdvertised >= subsSize)
                //{
                //    ViewBag.LimitReached = "Sory! you have reached the limit of this subscription please remove some listings or upgrade your subscription, thanks!.";
                //    return View(shop);

                //}


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

                if (files != null && files.Count()>0)
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
                shop.Ad.City= city;
                shop.NumberOfDoors = shopVM.NumberOfDoors;
                shop.StreetArea = shopVM.StreetArea;
                if(pictures.Count()>0)
                shop.Ad.Pictures.AddRange(pictures);
                db.Entry(shop).State = EntityState.Modified;
                db.SaveChanges();


                return RedirectToAction("Index");
            }
           
            ViewBag.CityId = new SelectList(db.Cities, "Id","Name", city.Name);
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
