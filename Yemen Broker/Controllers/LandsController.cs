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
    [Authorize]
    public class LandsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Lands
        public ActionResult Index()
        {
            string uId = User.Identity.GetUserId();

            var lands=db.Lands.OfType<Land>().Where(land => land.Ad.UserId.Equals(uId)).Include(l=>l.Ad).ToList();
            //var lands = db.Lands.Include(l => l.Ad);
            return View(lands);
        }

        // GET: Lands/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Land land = db.Lands.Find(id);
            if (land == null)
            {
                return HttpNotFound();
            }
            return View(land);
        }

        // GET: Lands/Create
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");
            return View();
        }

        // POST: Lands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LandsViewModel landsVM, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
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



                var City = db.Cities.Find(landsVM.CityId);
                Ad Ad = new Ad()
                {
                    AdDescribtion = landsVM.AdDescribtion,
                    AdPrice = landsVM.AdPrice,
                    City = City,
                    Discriminator = DiscriminatorOptions.Land,
                    Pictures = pictures,
                    UserId = User.Identity.GetUserId()
                };

                Land land = new Land()
                {
                   NumberOfLand=landsVM.NumberOfLand,
                   PlateNumber=landsVM.PlateNumber,
                   StreetsArea=landsVM.StreetsArea,
                    Ad = Ad
                };
                db.Lands.Add(land);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", landsVM.CityId);
            return View(landsVM);
        }

        // GET: Lands/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Land land = db.Lands.Find(id);
            if (land == null)
            {
                return HttpNotFound();
            }

            LandsViewModel shopModel = new LandsViewModel
            {
                AdDescribtion = land.Ad.AdDescribtion,
                AdPrice = land.Ad.AdPrice,
               NumberOfLand=land.NumberOfLand,
               PlateNumber=land.PlateNumber,
               StreetsArea=land.StreetsArea,
                Id = land.AdId,
                CityId = land.Ad.City.Id
            };


            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", land.Ad.City.Name);
            return View(shopModel);
        }

        // POST: Lands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LandsViewModel landsVM, IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(landsVM.CityId);

            if (ModelState.IsValid)
            {
                var land = db.Lands.Find(landsVM.Id);
                var pics = land.Ad.Pictures;

                var pictures = new List<Picture>();

                if (files != null && files.Count() > 0)
                {
                    foreach (var file in files)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = land.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }



                land.Ad.AdDescribtion = landsVM.AdDescribtion;
                land.Ad.AdPrice = landsVM.AdPrice;
                land.Ad.City = city;
                land.NumberOfLand = landsVM.NumberOfLand;
                land.PlateNumber = landsVM.PlateNumber;
                if (pictures.Count() > 0)
                    land.Ad.Pictures.AddRange(pictures);


                db.Entry(land).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", landsVM.CityId);
            return View(landsVM);
        }

        // GET: Lands/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Land land = db.Lands.Find(id);
            if (land == null)
            {
                return HttpNotFound();
            }
            return View(land);
        }

        // POST: Lands/Delete/5
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
