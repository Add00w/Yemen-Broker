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
    public class ApartmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Apartments
        public ActionResult Index()
        {
            string uId = User.Identity.GetUserId();

            var apartments = db.Apartments.OfType<Apartment>().Where(ap => ap.Ad.UserId.Equals(uId)).Include(l => l.Ad).ToList();
            //var apartments = db.Apartments.Include(a => a.Ad);
            return View(apartments);
        }

        // GET: Apartments/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Apartment apartment = db.Apartments.Find(id);
            if (apartment == null)
            {
                return HttpNotFound();
            }
            return View(apartment);
        }

        // GET: Apartments/Create
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");
            return View();
        }

        // POST: Apartments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApartmentViewModel apartmentVM, IEnumerable<HttpPostedFileBase> files)
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



                var City = db.Cities.Find(apartmentVM.CityId);
                Ad Ad = new Ad()
                {
                    AdDescribtion = apartmentVM.AdDescribtion,
                    AdPrice = apartmentVM.AdPrice,
                    City = City,
                    Discriminator = DiscriminatorOptions.Apartment,
                    Pictures = pictures,
                    UserId = User.Identity.GetUserId()
                };
                Apartment Apartment = new Apartment()
                {
                    Ad=Ad,
                    FloorNumber=apartmentVM.FloorNumber,
                    NumberOfBathrooms=apartmentVM.NumberOfBathrooms,
                    NumberOfDoors=apartmentVM.NumberOfDoors,
                    NumberOfKitchens=apartmentVM.NumberOfKitchens,
                    TypeOfFinishing=apartmentVM.TypeOfFinishing,
                    
                };

                db.Apartments.Add(Apartment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", apartmentVM.CityId);
            return View(apartmentVM);
        }

        // GET: Apartments/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Apartment apartment = db.Apartments.Find(id);
            if (apartment == null)
            {
                return HttpNotFound();
            }

            ApartmentViewModel apartmentModel = new ApartmentViewModel
            {
                AdDescribtion = apartment.Ad.AdDescribtion,
                AdPrice = apartment.Ad.AdPrice,
                FloorNumber = apartment.FloorNumber,
                NumberOfBathrooms = apartment.NumberOfBathrooms,
                NumberOfDoors = apartment.NumberOfDoors,
                Id = apartment.AdId,
                CityId = apartment.Ad.City.Id,
                NumberOfKitchens=apartment.NumberOfKitchens,
                TypeOfFinishing=apartment.TypeOfFinishing
            };


            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", apartment.Ad.City.Name);
            return View(apartmentModel);
        }

        // POST: Apartments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApartmentViewModel apartmentVM, IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(apartmentVM.CityId);

            if (ModelState.IsValid)
            {
                var apartment = db.Apartments.Find(apartmentVM.Id);
                var pics = apartment.Ad.Pictures;

                var pictures = new List<Picture>();

                if (files != null && files.Count() > 0)
                {
                    foreach (var file in files)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = apartment.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }



                apartment.Ad.AdDescribtion = apartmentVM.AdDescribtion;
                apartment.Ad.AdPrice = apartmentVM.AdPrice;
                apartment.Ad.City = city;
                apartment.FloorNumber = apartmentVM.FloorNumber;
                apartment.NumberOfBathrooms = apartmentVM.NumberOfBathrooms;
                apartment.NumberOfDoors = apartmentVM.NumberOfDoors;
                apartment.NumberOfKitchens = apartmentVM.NumberOfKitchens;
                apartment.TypeOfFinishing = apartmentVM.TypeOfFinishing;
                if (pictures.Count() > 0)
                    apartment.Ad.Pictures.AddRange(pictures);

                db.Entry(apartment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", apartmentVM.CityId);
            return View(apartmentVM);
        }

        // GET: Apartments/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Apartment apartment = db.Apartments.Find(id);
            if (apartment == null)
            {
                return HttpNotFound();
            }
            return View(apartment);
        }

        // POST: Apartments/Delete/5
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
