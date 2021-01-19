using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Yemen_Broker.Models;
using Yemen_Broker.ViewModels;

namespace Yemen_Broker.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Orders
        public ActionResult Index(string SearchString, String city, int page = 1)
        {
            var orders = db.Orders.OfType<Order>();
           
            if (!String.IsNullOrEmpty(SearchString))
            {
                orders = orders.Where(a => a.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Order_description.ToUpper().Contains(SearchString.ToUpper())
                || a.Order_title.ToUpper().Contains(SearchString.ToUpper()));
            }
            var cities = orders.Select(o=>o.City.Name).Distinct();
            ViewBag.city = new SelectList(cities);
            //pagination steps
            int pageSize = 6;
            var pager = new Pager(orders.Count(), page, pageSize);
            OrderIndexViewModel vModel = new OrderIndexViewModel()
            {
                Orders = orders.OrderBy(a => a.Order_date).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager,
                SearchString = SearchString,
                City = city,
            };
            return View(vModel);
        }
        [Authorize]
        public ActionResult MyOrders(string SearchString, string sortBy, int page = 1)
        {
            var uId = User.Identity.GetUserId();
            string adminId = db.Users.Where(u => u.UserType.Equals("Admin")).Select(u => u.Id).FirstOrDefault();
            ViewBag.adminId = adminId;
            var orders = db.Orders.OfType<Order>().Where(or => or.UserId.Equals(uId));
            if (!String.IsNullOrEmpty(SearchString))
            {
                orders = orders.Where(a => a.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Order_description.ToUpper().Contains(SearchString.ToUpper())
                || a.Order_title.ToUpper().Contains(SearchString.ToUpper()));
            }

            switch (sortBy)
            {
                case "Active":
                    orders = orders.Where(a => a.Confirmed);
                    break;
                case "Inactive":
                    orders = orders.Where(a => !a.Confirmed);
                    break;
                default:
                    break;
            }

            //pagination steps
            int pageSize = 6;
            var pager = new Pager(orders.Count(), page, pageSize);
            MyOrdersViewModel Myorders = new MyOrdersViewModel()
            {
                Orders = orders.OrderBy(a => a.Order_date).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize).ToList(),
                Pager = pager,
                SearchString = SearchString,


            };
            Myorders.Sorts = new Dictionary<string, string>
            {
                {"Active", "Active" },
                {"Inactive", "Inactive" }
            };
            return View(Myorders);
        }
        // Get: Orders/ManageOrders
        [Authorize(Roles = "Admin")]
        public ActionResult ManageOrders(string SearchString, int page = 1)
        {

            var orders = db.Orders.OfType<Order>().Where(order => !order.Confirmed);
            if (!String.IsNullOrEmpty(SearchString))
            {
                orders = orders.Where(a => a.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Order_description.ToUpper().Contains(SearchString.ToUpper())
                || a.Order_title.ToUpper().Contains(SearchString.ToUpper()));
            }

            //pagination steps
            int pageSize = 6;
            var pager = new Pager(orders.Count(), page, pageSize);
            MyOrdersViewModel myOrders = new MyOrdersViewModel()
            {
                Orders = orders.OrderBy(a => a.Order_date).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize).ToList(),
                Pager = pager,
                SearchString = SearchString,


            };

            return View(myOrders);
        }

        // Get: Orders/ConfirmOrReject
        [Authorize(Roles = "Admin")]
        public ActionResult ConfirmOrReject(long id = 0, bool isReject = false, bool isConfirm = false)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Order order = db.Orders.Find(id);
            if (isConfirm)
            {
                order.Confirmed = true;
                order.Rejected = false;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            else if (isReject)
            {
                ViewBag.rejectedOrderId = id;
                //Session.Add("rejectedOrderId", id);
                return View("ConfirmReject",order);
            }
            return RedirectToAction("ManageOrders");
        }
        // Post: Orders/ConfirmOrReject
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public ActionResult ConfirmOrReject(String message,long id)
        {
            //var orderId =long.Parse(Session.Contents["rejectedOrderId"].ToString());
            var order = db.Orders.Find(id);
            //here send message to the user and turn rejected to true
            Message Message = new Message()
            {
                SenderId = User.Identity.GetUserId(),
                IsMessage = false,
                MessageDateTime = DateTime.Now,
                MessageContent = message,
                RecieverId = order.UserId,
            };
            order.Rejected = true;
            db.Entry(order).State = EntityState.Modified;
            db.Messages.Add(Message);
            db.SaveChanges();
            return RedirectToAction("ManageOrders");

        }
        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            //code for rating start
            var rate = db.Ratings.Find(User.Identity.GetUserId(), order.UserId);
            ViewBag.inFavorite = db.Wishlists.Find(order.Order_id, User.Identity.GetUserId()) != null;
            var adverRate = db.Ratings.Where(r => r.AdvertiserId.Equals(order.UserId)).ToList();
            ViewBag.ratingAverage = adverRate.Count() > 0 ?
                                 db.Ratings.Where(r => r.AdvertiserId.Equals(order.UserId))
                                .Select(r => r.RatingNumber)
                                .Average() : 0;
            ViewBag.Rated = rate != null;
            //end of rating
            if (order == null)
            {
                return HttpNotFound();
            }
           
            //           //similar ads
            //var similarAds = db.Ads
            //                .Where(c => c.Confirmed)
            //                .OrderBy(c => order.City.Name)
            //                .ThenBy(c => order.Order_title)
            //                .ThenBy(c => c.User.DatePayingStarted)
            //                .Take(4);
            //ViewBag.similarAds = similarAds;
            return View(order);
        }

        // GET: Orders/Create
        [Authorize()]
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(OrderViewModel ordersVM)
        {
            if (ModelState.IsValid)
            {

                var City = db.Cities.Find(ordersVM.CityId);
                Order order = new Order()
                {
                    City = City,
                    UserId = User.Identity.GetUserId(),
                    Order_date = DateTime.Now.Date,
                    Order_title= ordersVM.Order_title,
                    Order_description= ordersVM.Order_description,
                };
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ordersVM.CityId);
            return View(ordersVM);
        }

        // GET: Orders/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            OrderViewModel orderModel = new OrderViewModel
            {
                Order_description = order.Order_description,
                Order_title = order.Order_title,
                CityId = order.City.Id
            };


            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", order.City.Name);
            return View(orderModel);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OrderViewModel ordersVM)
        {
            var city = db.Cities.Find(ordersVM.CityId);

            if (ModelState.IsValid)
            {
                var order = db.Orders.Find(ordersVM.Order_id);

                order.Order_description = ordersVM.Order_description;
                order.Order_title = ordersVM.Order_title;
                order.CityId = ordersVM.CityId;


                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyOrders", "Orders");
            }
            ViewBag.CityId = new SelectList(db.Cities, "Id", "Name", ordersVM.CityId);
            return View(ordersVM);
        }

        // GET: Orders/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
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
