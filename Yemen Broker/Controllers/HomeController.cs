using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yemen_Broker.Models;
using Yemen_Broker.ViewModels;

namespace Yemen_Broker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                int myChatsCount = db.Messages
                .Where(m => m.IsMessage)
                .Where(m => m.SenderId.Equals(userId) || m.RecieverId.Equals(userId)).Count();
                int myNotificationsCount = db.Messages
                     .Where(m => !m.IsMessage)
                     .Where(m => m.SenderId.Equals(userId) || m.RecieverId.Equals(userId)).Count();
                var subscriptionType = db.Users.Find(userId).Subscription.SubscriptionType;

                Session.Add("totalCount", myChatsCount + myNotificationsCount);
                Session.Add("myChatsCount", myChatsCount);
                Session.Add("myNotificationsCount", myNotificationsCount);
                Session.Add("subscriptionType", subscriptionType);
                var Notify = false;
                //check the remaining days for this user
                if (!User.IsInRole("Admin")) { 

                var user = db.Users.Find(userId);
                var remainingDays = user.SubscriptionEndDate.Value.Subtract(DateTime.Now).Days;
                
                if (remainingDays <= 0)
                {
                       
                        user.Confirmed = false;
                        db.Entry(user).State = EntityState.Modified;
                        Notify = false;
                        db.SaveChanges();

                    }
                else if (remainingDays <= 5)
                {
                        var notificationsCount = db.Messages.Where(n=>!n.IsMessage && n.SenderId.Equals(userId)&&n.RecieverId.Equals(userId)).Count();
                        if (notificationsCount <= 0)
                        {
                            Notify = true;
                            Message Message = new Message()
                            {
                                SenderId = userId,
                                IsMessage = false,
                                MessageDateTime = DateTime.Now,
                                MessageContent = "Your subscription ends within 5 days plz renew.",
                                RecieverId = userId,
                            };
                            db.Messages.Add(Message);
                            db.SaveChanges();
                        }
                        else
                        {
                            Notify = false;
                        }
                       
                    }
                    
                }
                ViewBag.Notify = Notify;
            }
            
            return View(db.Ads.ToList());
        }
        public ActionResult Report()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            double myAdsCount = db.Ads.Where(a => a.UserId.Equals(userId)).Count();
            double myOrdersCount = db.Orders.Where(o => o.UserId.Equals(userId)).Count();
            double myFavoritesCount = db.Wishlists.Where(w => w.UserId.Equals(userId)).Count();
            double adsCount = db.Ads.Count();
            double ordersCount = db.Orders.Count();


            ////from here I added for groupby example
            ///......
            var ads = db.Ads.OfType<Ad>().Where(a => a.UserId.Equals(userId));
            var query = ads.GroupBy(a => a.Discriminator)
                            .Select(g => new GroupedAdsViewModel
                            {
                                Name = g.Key.ToString(),
                                Total = g.Count()
                            }).ToList();
            ViewBag.AdsGroups = query;


            var views = db.Views.OfType<View>().Where(a => a.Ad.UserId.Equals(userId));
            var groupedViews = views.GroupBy(a => a.User.UserAddress)
                            .Select(g => new ViewsBasedOnCityViewModel
                            {
                                Name = g.Key.ToString(),
                                Total = g.Count()
                            }).ToList();
            ViewBag.GroupedViews = groupedViews;


            var usersBasedInSubsType = db.Users.OfType<User>();
            var groupedUsers = usersBasedInSubsType.GroupBy(u => u.Subscription.SubscriptionType)
                            .Select(g => new SubscribedUsersBasedOnSubsType
                            {
                                Name = g.Key.ToString(),
                                Total = g.Count()
                            }).ToList();
            ViewBag.GroupedUsers = groupedUsers;

            //end
            var model = new IndexViewModel
            {
                AdsCount = adsCount,
                OrdersCount = ordersCount,
                MyAdsCount = myAdsCount,//.........
                MyFavoritesCount = myFavoritesCount,//.........
                MyOrdersCount = myOrdersCount,//.........
                User = user,//.........
            };

            return View(model);
        }
        public ActionResult SubSubscriptionUsers(string subsType)
        {
            var users = db.Users.Where(u => u.Subscription.SubscriptionType.Equals(subsType));
            return View(users);
        }
        public ActionResult IndividualUserReport(string id)
        {
            //his ads
            int ads = db.Ads.Where(a => a.UserId.Equals(id)).Count();
            //his orders
            int orders = db.Orders.Where(o => o.UserId.Equals(id)).Count();

            var user = db.Users.Find(id);
            IndividualUserReportViewModel individualUser = new IndividualUserReportViewModel()
            {
                AdsCount = ads,
                OrdersCount = orders,
                User = user
            };
            return View(individualUser);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}