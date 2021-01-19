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
    [Authorize]
    public class WishlistsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Wishlists
        public ActionResult Index(string SearchString, int page = 1)
        {
           
            var userId = User.Identity.GetUserId();
            var wishlists = db.Wishlists.Where(w=>w.UserId==userId).Include(w => w.Ad).Include(w => w.User);
            if (!String.IsNullOrEmpty(SearchString))
            {
                wishlists = wishlists.Where(a => a.Ad.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdDescribtion.ToUpper().Contains(SearchString.ToUpper())
                || a.Ad.AdTitle.ToUpper().Contains(SearchString.ToUpper()));
            }

            //pagination steps
            int pageSize = 6;
            var pager = new Pager(wishlists.Count(), page, pageSize);
            WishlistViewModel mywishlist = new WishlistViewModel()
            {
                Wishlists = wishlists.OrderBy(a => a.Ad.Date).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize).ToList(),
                Pager = pager,
                SearchString = SearchString,


            };

            return View(mywishlist);
        }

        
        // get: Wishlists/Remove/5
        public ActionResult Remove(long id)
        {            var userId = User.Identity.GetUserId();

            Wishlist wishlist = db.Wishlists.Find(id,userId);
            db.Wishlists.Remove(wishlist);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        //Get:Wishlists/Add/5
        public ActionResult Add(long id)
        {
            Ad ad = db.Ads.Find(id);
            var userId = User.Identity.GetUserId();
            var wishlistExist = db.Wishlists
                .Where(w => w.UserId.Equals(userId)
                &&
                w.AdId==ad.AdId
                );
            if (wishlistExist.Count() <= 0)
            {
            db.Wishlists.Add(new Wishlist() { AdId=id,UserId=userId});
            db.SaveChanges();
            }
            
            return RedirectToAction("Index");
        }

        //Get:Wishlists/Add/5
        public ActionResult AddToWishlistWithNote(string note,long id)
        {
            Ad ad = db.Ads.Find(id);
            var userId = User.Identity.GetUserId();
            var wishlistExist = db.Wishlists
                .Where(w => w.UserId.Equals(userId)
                &&
                w.AdId == ad.AdId
                );
            if (wishlistExist.Count() <= 0)
            {
                db.Wishlists.Add(new Wishlist() { AdId = id, UserId = userId,Note=note });
                db.SaveChanges();
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
