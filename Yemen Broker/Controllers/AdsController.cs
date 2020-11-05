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
    public class AdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard/Ads
        public ActionResult Index()
        {
            //hello
            return View(db.Ads.Where(ad => ad.Discriminator == DiscriminatorOptions.Ad).ToList());

        }

        // GET: Dashboard/Ads/Details/5
        public ActionResult Details(long? id)
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

        // GET: Dashboard/Ads/Create
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");

            return View();
        }

        // POST: Dashboard/Ads/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ad ad, IEnumerable<HttpPostedFileBase> files)
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
                ad.UserId = User.Identity.GetUserId();
                //var City = db.Cities.Find(ad.City);
                ad.Pictures = pictures;
                db.Ads.Add(ad);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ad);
        }

        // GET: Dashboard/Ads/Edit/5
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
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ad.City.Name);

            return View(ad);
        }

        // POST: Dashboard/Ads/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Ad ad)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ad).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ad);
        }

        // GET: Dashboard/Ads/Delete/5
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

        // POST: Dashboard/Ads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
           

            Ad ad = db.Ads.Find(id);
            var pics = ad.Pictures;
            if (pics != null)
                foreach (var item in pics)
                {
                    db.Ads.Find(ad.AdId).Pictures.Remove(item);
                }
            db.Ads.Remove(ad);
            db.SaveChanges();


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
