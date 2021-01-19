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
    public class CarsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Cars
        public ActionResult Index( string SearchString, String city, String status, double from = 0.0, double to = 0.0, int page = 1)
        {
            var cars = db.Cars.OfType<Car>()
                .Where(w => w.Ad.User.Confirmed)
                .Include(l => l.Ad);
            ViewBag.Views = db.Views;
            ViewBag.Wishlists = db.Wishlists;
            if (!String.IsNullOrEmpty(SearchString))
            {
                cars = cars.Where(a => a.Ad.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.CarCompany.ToUpper().Contains(SearchString.ToUpper())
                || a.CarName.ToUpper().Contains(SearchString.ToUpper())
                || a.CarModel.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (!String.IsNullOrEmpty(city))
            {
                cars = cars.Where(a => a.Ad.City.Name.ToUpper().Equals(city));
            }
            if (!String.IsNullOrEmpty(status))
            {
                cars = cars.Where(a => a.Status.ToUpper().Equals(status));
            }
            var statu = cars.Select(c => c.Status).Distinct();
            ViewBag.status = new SelectList(statu);
            if (to >= from && to > 1 && from >= 0.0)
            {
                cars = cars.Where(a => a.Ad.AdPrice >= from && a.Ad.AdPrice <= to);

            }
            var cities = cars.Select(c => c.Ad.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(cars.Count(), page, pageSize);
            CarrIndexViewModel vModel = new CarrIndexViewModel()
            {
                Cars = cars.OrderBy(a => a.Ad.AdPrice).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City = city,
                Status = status,
                From = from,
                To = to

            };
            return View(vModel);
        }

        // GET: Cars/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);

            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(),car.Ad.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(car.AdId,User.Identity.GetUserId())!=null;
            var adverRate = db.Ratings.Where(r => r.AdvertiserId.Equals(car.Ad.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(car.Ad.UserId))
                                .Select(r => r.RatingNumber)
                                .Average() : 0;

            ViewBag.ratersCount = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(car.Ad.UserId))
                                 .Count() : 0;
            ViewBag.Rated = rate!=null;
            //end of rating

            if (car == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                String userId = User.Identity.GetUserId();
                bool isOwner = car.Ad.UserId.Equals(userId);
                int view = db.Views.Where(v => v.UserId.Equals(userId) && v.AdId == car.AdId).Count();
                if (!isOwner && view == 0)
                {
                    db.Views.Add(new View()
                    {
                        AdId = car.AdId,
                        UserId = userId,
                    });
                    db.SaveChanges();
                }
            }
            var views = db.Views.Where(c => c.AdId.Equals(car.AdId)).Count();

            ViewBag.views = views;
            //similar ads
            var similarCars = db.Cars
                            .Where(c => c.Ad.Confirmed)
                            .OrderBy(c=>car.CarName)
                            .ThenBy(c => car.Ad.AdTitle)
                            .ThenBy(c => car.CarModel)
                            .ThenBy(c => c.Ad.User.DatePayingStarted)
                            .Take(4);
            ViewBag.similarCars = similarCars;
            return View(car);
        }

        // GET: Cars/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");
            return View();
        }

        // POST: Cars/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(CarViewModel CarVM, IEnumerable<HttpPostedFileBase> files)
        {

            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            if (!user.Confirmed)
            {
                ViewBag.NotConfirmed = "Sory! wait until the confirmation, thanks!.";
                ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", CarVM.CityId);

                return View(CarVM);
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
                        ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", CarVM.CityId);

                        return View(CarVM);

                    }
                }
                //if user chooses no image show him error
                if (files==null || files.Count() <= 0 || files.FirstOrDefault()==null)
                {
                    ViewBag.chooseImage = "Please choose image";
                    ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", CarVM.CityId);
                    return View(CarVM);
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



                var City = db.Cities.Find(CarVM.CityId);
                Ad Ad = new Ad()
                {
                    AdTitle = CarVM.AdTitle,
                    AdDescribtion = CarVM.AdDescribtion,
                    AdPrice = CarVM.AdPrice,
                    City = City,
                    Discriminator = DiscriminatorOptions.Car,
                    Pictures = pictures,
                    UserId = User.Identity.GetUserId(),
                    Date = DateTime.Now.Date,
                };
                Car Car = new Car()
                {
                    Ad = Ad,
                    CarCompany = CarVM.CarCompany,
                    CarName = CarVM.CarName,
                    CarType = CarVM.CarType,
                    TypeofGas = CarVM.TypeofGas,
                    TypeofGear = CarVM.TypeofGear,
                    CarModel = CarVM.CarModel,
                    EngineType = CarVM.EngineType,
                    CarColor = CarVM.CarColor,
                    Status = CarVM.Status,
                };

                db.Cars.Add(Car);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", CarVM.CityId);
            return View(CarVM);
        }

        // GET: Cars/Edit/5
        [Authorize]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }

            CarViewModel carViewModel = new CarViewModel
            {
                AdDescribtion = car.Ad.AdDescribtion,
                AdPrice = car.Ad.AdPrice,
                AdTitle = car.Ad.AdTitle,
                CarColor = car.CarColor,
                CarCompany = car.CarCompany,
                Id = car.AdId,
                CityId = car.Ad.City.Id,
                CarModel = car.CarModel,
                CarName = car.CarName,
                CarType = car.CarType,
                EngineType = car.EngineType,
                Status = car.Status,
                TypeofGas = car.TypeofGas,
                TypeofGear = car.TypeofGear,
            };


            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", car.Ad.City.Name);
            return View(carViewModel);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(CarViewModel CarVM, IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(CarVM.CityId);

            if (ModelState.IsValid)
            {
                var car = db.Cars.Find(CarVM.Id);
                var pics = car.Ad.Pictures;

                var pictures = new List<Picture>();

                if (files != null && files.Count() > 0 && files.FirstOrDefault()!=null)
                {
                    foreach (var file in files)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = car.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }



                car.Ad.AdDescribtion = CarVM.AdDescribtion;
                car.Ad.AdPrice = CarVM.AdPrice;
                car.Ad.AdTitle = CarVM.AdTitle;
                car.Ad.City = city;
                car.CarColor = CarVM.CarColor;
                car.CarCompany = CarVM.CarCompany;
                car.CarModel = CarVM.CarModel;
                car.CarName = CarVM.CarName;
                car.CarType = CarVM.CarType;
                car.EngineType = CarVM.EngineType;
                car.Status = CarVM.Status;
                car.TypeofGas = CarVM.TypeofGas;
                car.TypeofGear = CarVM.TypeofGear;
                if (pictures.Count() > 0)
                    car.Ad.Pictures.AddRange(pictures);

                db.Entry(car).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyAds","Ads");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", CarVM.CityId);
            return View(CarVM);
        }

        // GET: Cars/Delete/5
        [Authorize]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(long id)
        {
            Ad ad = db.Ads.Find(id);
            var pics = ad.Pictures;

            if (pics != null && pics.Count > 0)
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
