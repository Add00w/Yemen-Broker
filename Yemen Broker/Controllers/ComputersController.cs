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
    public class ComputersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Computers
        public ActionResult Index(string SearchString, String city, string status, double from = 0.0, double to = 0.0, int page = 1)
        {
            var computers = db.Computers.OfType<Computer>()
                .Where(w => w.Ad.User.Confirmed)
                .Include(l => l.Ad);
            ViewBag.Views = db.Views;
            ViewBag.Wishlists = db.Wishlists;
            if (!String.IsNullOrEmpty(SearchString))
            {
                computers = computers.Where(a => a.Ad.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.ComputerCompany.ToUpper().Contains(SearchString.ToUpper())
                || a.ComputerOS.ToUpper().Contains(SearchString.ToUpper())
                || a.ComputerColor.ToUpper().Contains(SearchString.ToUpper()));
            }
            if (!String.IsNullOrEmpty(city))
            {
                computers = computers.Where(a => a.Ad.City.Name.ToUpper().Equals(city));
            }

            if (to >= from && to > 1 && from >= 0.0)
            {
                computers = computers.Where(a => a.Ad.AdPrice >= from && a.Ad.AdPrice <= to);

            }
            var cities = computers.Select(c => c.Ad.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(computers.Count(), page, pageSize);
            ComputerIndexViewModel vModel = new ComputerIndexViewModel()
            {
                Computers = computers.OrderBy(a => a.Ad.AdPrice).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City = city,
                Status = status,
                From = from,
                To = to

            };
            return View(vModel);
        }

        // GET: Computers/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Computer computer = db.Computers.Find(id);
            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(), computer.Ad.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(computer.AdId, User.Identity.GetUserId()) != null;
            var adverRate = db.Ratings.Where(r => r.AdvertiserId.Equals(computer.Ad.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(computer.Ad.UserId))
                                .Select(r => r.RatingNumber)
                                .Average() : 0;

            ViewBag.ratersCount = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(computer.Ad.UserId))
                                 .Count() : 0;
            ViewBag.Rated = rate != null;
            //end of rating
            if (computer == null)
            {
                return HttpNotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                String userId = User.Identity.GetUserId();
                bool isOwner = computer.Ad.UserId.Equals(userId);
                int view = db.Views.Where(v => v.UserId.Equals(userId) && v.AdId == computer.AdId).Count();

                if (!isOwner && view == 0)
                {
                    db.Views.Add(new View()
                    {
                        AdId = computer.AdId,
                        UserId = userId,
                    });
                    db.SaveChanges();
                }
            }
            var views = db.Views.Where(co => co.AdId.Equals(computer.AdId)).Count();

            ViewBag.views = views;
            //similar ads
            var similarComputers = db.Computers
                            .Where(c => c.Ad.Confirmed)
                            .OrderBy(c => computer.ComputerCompany)
                            .ThenBy(c => computer.Ad.AdTitle)
                            .ThenBy(c => computer.ComputerOS)
                            .ThenBy(c => c.Ad.User.DatePayingStarted)
                            .Take(4);
            ViewBag.similarComputers = similarComputers;
            return View(computer);
        }

        // GET: Computers/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");
            return View();
        }

        // POST: Computers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(ComputerViewModel ComputerVM, IEnumerable<HttpPostedFileBase> files)
        {

            var uid = User.Identity.GetUserId();
            var user = db.Users.Find(uid);
            if (!user.Confirmed)
            {
                ViewBag.NotConfirmed = "Sory! wait until the confirmation, thanks!.";
                ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ComputerVM.CityId);

                return View(ComputerVM);
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
                        ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ComputerVM.CityId);

                        return View(ComputerVM);

                    }
                }
                //if user chooses no image show him error
                if (files == null || files.Count() <= 0 || files.FirstOrDefault() == null)
                {
                    ViewBag.chooseImage = "Please choose image";
                    ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ComputerVM.CityId);
                    return View(ComputerVM);
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



                var City = db.Cities.Find(ComputerVM.CityId);
                Ad Ad = new Ad()
                {
                    AdTitle = ComputerVM.AdTitle,
                    AdDescribtion = ComputerVM.AdDescribtion,
                    AdPrice = ComputerVM.AdPrice,
                    City = City,
                    Discriminator = DiscriminatorOptions.Computer,
                    Pictures = pictures,
                    UserId = User.Identity.GetUserId(),
                    Date = DateTime.Now.Date,
                };
                Computer Computer = new Computer()
                {
                    Ad = Ad,
                    ComputerCompany = ComputerVM.ComputerCompany,
                    ComputerRam = ComputerVM.ComputerRam,
                    ComputerStorage = ComputerVM.ComputerStorage,
                    ComputerScreenSize = ComputerVM.ComputerScreenSize,
                    ComputerCpu = ComputerVM.ComputerCpu,
                    ComputerOS = ComputerVM.ComputerOS,
                    ComputerColor = ComputerVM.ComputerColor,
                    ComputerBattery = ComputerVM.ComputerBattery,
                    ComputerStatus = ComputerVM.ComputerStatus,
                    CD_Drive = ComputerVM.CD_Drive,
                    ComputerScreenCard = ComputerVM.ComputerScreenCard,

                };

                db.Computers.Add(Computer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ComputerVM.CityId);
            return View(ComputerVM);
        }

        // GET: Computers/Edit/5
        [Authorize]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Computer computer = db.Computers.Find(id);
            if (computer == null)
            {
                return HttpNotFound();
            }
            ComputerViewModel computerViewModel = new ComputerViewModel
            {
                AdDescribtion = computer.Ad.AdDescribtion,
                AdPrice = computer.Ad.AdPrice,
                AdTitle = computer.Ad.AdTitle,
                Id = computer.AdId,
                CityId = computer.Ad.City.Id,
                CD_Drive = computer.CD_Drive,
                ComputerBattery = computer.ComputerBattery,
                ComputerColor = computer.ComputerColor,
                ComputerCompany = computer.ComputerCompany,
                ComputerCpu = computer.ComputerCpu,
                ComputerOS = computer.ComputerOS,
                ComputerRam = computer.ComputerRam,
                ComputerScreenCard = computer.ComputerScreenCard,
                ComputerScreenSize = computer.ComputerScreenSize,
                ComputerStatus = computer.ComputerStatus,
                ComputerStorage = computer.ComputerStorage,


            };


            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", computer.Ad.City.Name);
            return View(computerViewModel);
        }

        // POST: Computers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(ComputerViewModel ComputerVM, IEnumerable<HttpPostedFileBase> files)
        {
            var city = db.Cities.Find(ComputerVM.CityId);

            if (ModelState.IsValid)
            {
                var computer = db.Computers.Find(ComputerVM.Id);
                var pics = computer.Ad.Pictures;

                var pictures = new List<Picture>();

                if (files != null && files.Count() > 0 && files.FirstOrDefault() != null)
                {
                    foreach (var file in files)
                    {
                        string filName = Guid.NewGuid() + Path.GetFileName(file.FileName);
                        pictures.Add(new Picture { PictureURL = filName, AdId = computer.AdId });
                        file.SaveAs(Path.Combine(Server.MapPath("/Uploads/"), filName));

                    }
                }



                computer.Ad.AdDescribtion = ComputerVM.AdDescribtion;
                computer.Ad.AdPrice = ComputerVM.AdPrice;
                computer.Ad.AdTitle = ComputerVM.AdTitle;
                computer.Ad.City = city;
                computer.CD_Drive = ComputerVM.CD_Drive;
                computer.ComputerBattery = ComputerVM.ComputerBattery;
                computer.ComputerColor = ComputerVM.ComputerColor;
                computer.ComputerCompany = ComputerVM.ComputerCompany;
                computer.ComputerCpu = ComputerVM.ComputerCpu;
                computer.ComputerOS = ComputerVM.ComputerOS;
                computer.ComputerRam = ComputerVM.ComputerRam;
                computer.ComputerScreenCard = ComputerVM.ComputerScreenCard;
                computer.ComputerScreenSize = ComputerVM.ComputerScreenSize;
                computer.ComputerStatus = ComputerVM.ComputerStatus;
                computer.ComputerStorage = ComputerVM.ComputerStorage;
                if (pictures.Count() > 0)
                    computer.Ad.Pictures.AddRange(pictures);

                db.Entry(computer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyAds", "Ads");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ComputerVM.CityId);
            return View(ComputerVM);
        }

        // GET: Computers/Delete/5
        [Authorize]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Computer computer = db.Computers.Find(id);
            if (computer == null)
            {
                return HttpNotFound();
            }
            return View(computer);
        }

        // POST: Computers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(long id)
        {
            Ad ad = db.Ads.Find(id);
            var pics = ad.Pictures;

            if (pics != null && pics.Count>0)
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
