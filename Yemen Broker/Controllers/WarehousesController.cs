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
    public class WarehousesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Warehouses
        public ActionResult Index(string SearchString, String city, double from = 0.0, double to = 0.0, int page = 1, int numberofDoor = 1)
        {
            var warehouses = db.Warehouses.OfType<Warehouse>()
                .Where(w=>w.Ad.User.Confirmed)
                .Include(l => l.Ad);
            ViewBag.Views = db.Views;
            ViewBag.Wishlists = db.Wishlists;
            if (!String.IsNullOrEmpty(SearchString))
            {
                warehouses = warehouses.Where(a => a.Ad.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdTitle.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (!String.IsNullOrEmpty(city))
            {
                warehouses = warehouses.Where(a => a.Ad.City.Name.ToUpper().Equals(city));
            }

            if (to >= from && to > 1 && from >= 0.0)
            {
                warehouses = warehouses.Where(a => a.Ad.AdPrice >= from && a.Ad.AdPrice <= to);

            }
            var cities = warehouses.Select(c => c.Ad.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(warehouses.Count(), page, pageSize);
            WarehouseIndexViewModel vModel = new WarehouseIndexViewModel()
            {
                Warehouses = warehouses.OrderBy(a => a.Ad.AdPrice).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City = city,
                NumberofDoor = numberofDoor,
                From = from,
                To = to

            };
            return View(vModel);
        }

        // GET: Warehouses/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Warehouse warehouse = db.Warehouses.Find(id);
            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(), warehouse.Ad.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(warehouse.AdId, User.Identity.GetUserId()) != null;
            var adverRate = db.Ratings.Where(r => r.AdvertiserId.Equals(warehouse.Ad.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(warehouse.Ad.UserId))
                                .Select(r => r.RatingNumber)
                                .Average() : 0;

            ViewBag.ratersCount = adverRate.Count() > 0 ?
                               db.Ratings.Where(r => r.AdvertiserId.Equals(warehouse.Ad.UserId))
                               .Count() : 0;
            ViewBag.Rated = rate != null;
            //end of rating
            if (warehouse == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                String userId = User.Identity.GetUserId();
                bool isOwner = warehouse.Ad.UserId.Equals(userId);
                int view = db.Views.Where(v => v.UserId.Equals(userId) && v.AdId == warehouse.AdId).Count();

                if (!isOwner && view == 0)
                {
                    db.Views.Add(new View()
                    {
                        AdId = warehouse.AdId,
                        UserId = userId,
                    });
                    db.SaveChanges();
                }
            }
            var views = db.Views.Where(w => w.AdId.Equals(warehouse.AdId)).Count();

            ViewBag.views = views;
            //similar ads
            var similarWarehouses = db.Warehouses
                            .Where(c => c.Ad.Confirmed)
                            .OrderBy(c => warehouse.Ad.City.Name)
                            .ThenBy(c => warehouse.Ad.AdTitle)
                            .ThenBy(c => warehouse.NumberOfDoors)
                            .ThenBy(c => c.Ad.User.DatePayingStarted)
                            .Take(4);
            ViewBag.similarWarehouses = similarWarehouses;
            return View(warehouse);
        }

        // GET: Warehouses/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");
            return View();
        }

        // POST: Warehouses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create( WarehouseViewModel warehouseViewModel, IEnumerable<HttpPostedFileBase> files)
        {
            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            if (!user.Confirmed)
            {
                ViewBag.NotConfirmed = "Sory! wait until the confirmation, thanks!.";
                ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", warehouseViewModel.CityId);

                return View(warehouseViewModel);
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
                        ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", warehouseViewModel.CityId);

                        return View(warehouseViewModel);

                    }
                }
                //if user chooses no image show him error
                if (files == null || files.Count() <= 0 || files.FirstOrDefault() == null)
                {
                    ViewBag.chooseImage = "Please choose image";
                    ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", warehouseViewModel.CityId);
                    return View(warehouseViewModel);
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



                var City = db.Cities.Find(warehouseViewModel.CityId);
                Ad Ad = new Ad()
                {
                    AdDescribtion = warehouseViewModel.AdDescribtion,
                    AdPrice = warehouseViewModel.AdPrice,
                    City = City,
                    AdTitle = warehouseViewModel.AdTitle,
                    Date = DateTime.Now.Date,
                    Discriminator = DiscriminatorOptions.Warehouse,
                    Pictures = pictures,
                    UserId = User.Identity.GetUserId()
                };

                Warehouse warehouse = new Warehouse()
                {
                   HeightOfWall=warehouseViewModel.HeightOfWall,
                   NumberOfDoors=warehouseViewModel.NumberOfDoors,
                   StreetArea=warehouseViewModel.StreetArea,
                    Ad = Ad
                };


                db.Warehouses.Add(warehouse);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine("error:" + e.Message);
                    Console.WriteLine("trace:" + e.StackTrace);

                }


                return RedirectToAction("Index");
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", warehouseViewModel.CityId);

            return View(warehouseViewModel);
        }

        // GET: Warehouses/Edit/5
        [Authorize]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Warehouse warehouse = db.Warehouses.Find(id);
            if (warehouse == null)
            {
                return HttpNotFound();
            }
            WarehouseViewModel warehouseVM = new WarehouseViewModel
            {
                AdDescribtion = warehouse.Ad.AdDescribtion,
                AdPrice = warehouse.Ad.AdPrice,
                AdTitle=warehouse.Ad.AdTitle,
               HeightOfWall=warehouse.HeightOfWall,
               NumberOfDoors=warehouse.NumberOfDoors,
               StreetArea=warehouse.StreetArea,
                Id = warehouse.AdId,
                CityId = warehouse.Ad.CityId
            };
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", warehouse.Ad.City.Name);

            return View(warehouseVM);
        }

        // POST: Warehouses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(WarehouseViewModel warehouseVM, IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(warehouseVM.CityId);

            if (ModelState.IsValid)
            {
                var warehouse = db.Warehouses.Find(warehouseVM.Id);
                var pics = warehouse.Ad.Pictures;

                var pictures = new List<Picture>();

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0 && files.FirstOrDefault() != null)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = warehouse.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }




                warehouse.Ad.AdDescribtion = warehouseVM.AdDescribtion;
                warehouse.Ad.AdPrice = warehouseVM.AdPrice;
                warehouse.Ad.AdTitle = warehouseVM.AdTitle;
                warehouse.Ad.City = city;
                warehouse.NumberOfDoors = warehouseVM.NumberOfDoors;
                warehouse.HeightOfWall = warehouseVM.HeightOfWall;
                warehouse.StreetArea = warehouseVM.StreetArea;
                warehouse.AdId = warehouseVM.Id;
                if (pictures.Count() > 0)
                    warehouse.Ad.Pictures.AddRange(pictures);




                db.Entry(warehouse).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyAds", "Ads");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", warehouseVM.CityId);
            return View(warehouseVM);
        }

        // GET: Warehouses/Delete/5
        [Authorize]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Warehouse warehouse = db.Warehouses.Find(id);
            if (warehouse == null)
            {
                return HttpNotFound();
            }
            return View(warehouse);
        }

        // POST: Warehouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(long id)
        {
            Warehouse warehouse = db.Warehouses.Find(id);
            Ad ad = warehouse.Ad;
            var pics = ad.Pictures;
            db.Warehouses.Remove(warehouse);
            if (pics != null && pics.Count()>0)
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
