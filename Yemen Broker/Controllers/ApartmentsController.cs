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
        public ActionResult Index(string SearchString, String city, double from=0.0, double to=0.0, int page = 1)
        {
            //string uId = User.Identity.GetUserId();

            var apartments = db.Apartments.OfType<Apartment>()
                .Where(w => w.Ad.User.Confirmed)
                .Include(l => l.Ad);
            ViewBag.Views = db.Views;
            ViewBag.Wishlists = db.Wishlists;   
            if (!String.IsNullOrEmpty(SearchString))
            {
                apartments = apartments.Where(a => a.Ad.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.TypeOfFinishing.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (!String.IsNullOrEmpty(city))
            {
                apartments = apartments.Where(a => a.Ad.City.Name.ToUpper().Equals(city));
            }

            if (to >= from && to > 1 && from >= 0.0)
            {
                apartments = apartments.Where(a => a.Ad.AdPrice >= from && a.Ad.AdPrice <= to);

            }
            var cities = apartments.Select(c => c.Ad.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(apartments.Count(), page, pageSize);
            ApartmentIndexViewModel vModel = new ApartmentIndexViewModel()
            {
                Apartments = apartments.OrderBy(a => a.Ad.AdPrice).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City=city,
                From=from,
                To=to
                
            };
            return View(vModel);
        }

        // GET: Apartments/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Apartment apartment = db.Apartments.Find(id);
            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(), apartment.Ad.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(apartment.AdId, User.Identity.GetUserId()) != null;
            var adverRate = db.Ratings.Where(r => r.AdvertiserId.Equals(apartment.Ad.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(apartment.Ad.UserId))
                                .Select(r => r.RatingNumber)
                                .Average() : 0;
            ViewBag.ratersCount = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(apartment.Ad.UserId))
                                 .Count() : 0;
            ViewBag.Rated = rate != null;
            //end of rating
            if (apartment == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                String userId = User.Identity.GetUserId();
                bool isOwner = apartment.Ad.UserId.Equals(userId);
                int view = db.Views.Where(v => v.UserId.Equals(userId) && v.AdId == apartment.AdId).Count();

                if (!isOwner && view == 0)
                {
                    db.Views.Add(new View()
                    {
                        AdId = apartment.AdId,
                        UserId = userId,
                    });
                    db.SaveChanges();
                }
            }
            var views = db.Views.Where(A => A.AdId.Equals(apartment.AdId)).Count();

            ViewBag.views = views;
            //similar ads
            var similarApartments = db.Apartments
                            .Where(c => c.Ad.Confirmed)
                            .OrderBy(c => apartment.NumberOfRooms)
                            .ThenBy(c => apartment.Ad.AdTitle)
                            .ThenBy(c => apartment.FloorNumber)
                            .ThenBy(c => c.Ad.User.DatePayingStarted)
                            .Take(4);
            ViewBag.similarApartments = similarApartments;
            return View(apartment);
        }

        // GET: Apartments/Create
        [Authorize]
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
        [Authorize]
        public ActionResult Create(ApartmentViewModel apartmentVM, IEnumerable<HttpPostedFileBase> files)
        {
            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            if (!user.Confirmed)
            {
                ViewBag.NotConfirmed = "Sory! wait until the confirmation, thanks!.";
                ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", apartmentVM.CityId);

                return View(apartmentVM);
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
                        ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", apartmentVM.CityId);

                        return View(apartmentVM);

                    }
                }
                //if user chooses no image show him error
                if (files == null || files.Count() <= 0 || files.FirstOrDefault() == null)
                {
                    ViewBag.chooseImage = "Please choose image";
                    ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", apartmentVM.CityId);
                    return View(apartmentVM);
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



                var City = db.Cities.Find(apartmentVM.CityId);
                Ad Ad = new Ad()
                {
                    AdTitle = apartmentVM.AdTitle,
                    AdDescribtion = apartmentVM.AdDescribtion,
                    AdPrice = apartmentVM.AdPrice,
                    City = City,
                    Discriminator = DiscriminatorOptions.Apartment,
                    Pictures = pictures,
                    UserId = User.Identity.GetUserId(),
                    Date = DateTime.Now.Date,
                };
                Apartment Apartment = new Apartment()
                {
                    Ad=Ad,
                    FloorNumber=apartmentVM.FloorNumber,
                    NumberOfBathrooms=apartmentVM.NumberOfBathrooms,
                    NumberOfDoors=apartmentVM.NumberOfDoors,
                    NumberOfKitchens=apartmentVM.NumberOfKitchens,
                    TypeOfFinishing=apartmentVM.TypeOfFinishing,
                    NumberOfRooms = apartmentVM.NumberOfRooms,

                };

                db.Apartments.Add(Apartment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", apartmentVM.CityId);
            return View(apartmentVM);
        }

        // GET: Apartments/Edit/5
        [Authorize]
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
                AdTitle=apartment.Ad.AdTitle,
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
        [Authorize]
        public ActionResult Edit(ApartmentViewModel apartmentVM, IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(apartmentVM.CityId);

            if (ModelState.IsValid)
            {
                var apartment = db.Apartments.Find(apartmentVM.Id);
                var pics = apartment.Ad.Pictures;

                var pictures = new List<Picture>();

                if (files != null && files.Count() > 0 && files.FirstOrDefault() != null)
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
                apartment.Ad.AdTitle = apartmentVM.AdTitle;
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
                return RedirectToAction("MyAds", "Ads");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", apartmentVM.CityId);
            return View(apartmentVM);
        }

        // GET: Apartments/Delete/5
        [Authorize]
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
        [Authorize]
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
