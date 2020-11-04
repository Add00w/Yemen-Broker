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
        public ActionResult Index()
        {
            var homes = db.Homes.Include(h => h.Ad);
            return View(homes.ToList());
        }

        // GET: Homes/Details/5
        public ActionResult Details(long? id)
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

        // GET: Homes/Create
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
        public ActionResult Create(HomeViewModel homeVM,IEnumerable<HttpPostedFileBase> files)
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



                var City = db.Cities.Find(homeVM.CityId);
                Ad Ad = new Ad()
                {
                    AdDescribtion = homeVM.AdDescribtion,
                    AdPrice = homeVM.AdPrice,
                    City = City,
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
                DetailSystem=home.DetailSystem,
                NumberOfFloors=home.NumberOfFloors,
                NumberOfLand=home.NumberOfLand,
                PlateNumber=home.PlateNumber,
                StreetsArea=home.StreetsArea,
                Id=home.AdId,
                CityId=home.Ad.CityId
            };
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", homeVM.CityId);

            return View(homeVM);
        }

        // POST: Homes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    if (file != null && file.ContentLength > 0)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = home.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }
                



                home.Ad.AdDescribtion = homeVM.AdDescribtion;
                home.Ad.AdPrice = homeVM.AdPrice;
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
                return RedirectToAction("Index");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", homeVM.CityId);
            return View(homeVM);
        }

        // GET: Homes/Delete/5
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
