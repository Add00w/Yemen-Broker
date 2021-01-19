using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Yemen_Broker.Models;
using Yemen_Broker.ViewModels;

namespace Yemen_Broker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public UsersController()
        {
            db = new ApplicationDbContext();
        }

        public UsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;

        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET: Dashboard/Users
        public ActionResult Index(string SearchString, string sortBy, int page = 1)
        {
            var users = UserManager.Users.Where(u=>!u.UserType.Equals("Admin"));

            if (!String.IsNullOrEmpty(SearchString))
            {
                users = users.Where(a => a.UserAddress.ToUpper().Contains(SearchString.ToUpper())
                || a.UserName.ToUpper().Contains(SearchString.ToUpper())
                || a.Email.ToUpper().Contains(SearchString.ToUpper())
                || a.Subscription.SubscriptionType.ToUpper().Contains(SearchString.ToUpper()));
            }

            switch (sortBy)
            {
                case "Active":
                    users = users.Where(a => a.Confirmed);
                    break;
                case "Inactive":
                    users = users.Where(a => !a.Confirmed);
                    break;
                case "Blocked":
                    users = users.Where(a => a.Blocked);
                    break;
                default:
                    break;
            }

            //pagination steps
            int pageSize = 6;
            var pager = new Pager(users.Count(), page, pageSize);
            UserViewModel User = new UserViewModel()
            {
                Users = users.OrderBy(a => a.RegisteredDate).Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize).ToList(),
                Pager = pager,
                SearchString = SearchString,


            };
            User.Sorts = new Dictionary<string, string>
            {
                {"Active", "Active" },
                {"Inactive", "Inactive" },
                {"Blocked", "Blocked" }
            };
            return View(User);
        }



        // GET: Dashboard/Users/Details/5
        [Authorize]
        public async Task<ActionResult> Details(string id)
        {
            var user = await UserManager.FindByIdAsync(id);

            return View(user);
        }

        // GET: Dashboard/Users/Create
        public ActionResult Register()
        {
            return View();
        }

        // POST: Dashboard/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var rolemanager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            string passwordErro = "";

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    UserAddress = model.UserAddress,
                    SubscriptionId = 2,//default to free
                    RegisteredDate = DateTime.Now,
                    UserType = "User",
                    PhoneNumber = model.PhoneNumber,
                    Blocked = false,
                    Confirmed=false,
                    Rejected=false,
                    SubscriptionEndDate = DateTime.Now.AddDays(5)

                };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {


                    if (!rolemanager.RoleExists("User"))
                    {
                        IdentityRole role = new IdentityRole()
                        {
                            Name = "User"
                        };
                       await rolemanager.CreateAsync(role);
                    }
                    await UserManager.AddToRoleAsync(user.Id, "User");
                    return RedirectToAction("Index");
                }
                else
                {
                    passwordErro = result.Errors.FirstOrDefault().ToString();

                    ViewBag.passwordErro = passwordErro;

                }
            }
          
            return View(model);


        }

        // GET: Dashboard/Users/Edit/5
        public async Task<ActionResult> Upgrade(string id)
        {

            RegisterViewModel model = new RegisterViewModel();

            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


            var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                    return HttpNotFound();

            Session.Add("userId", id);

                model.UserAddress = user.UserAddress;
                model.Email = user.Email;
                model.UserName = user.UserName;
                model.PhoneNumber = user.PhoneNumber;



                ViewBag.SubscriptionId = new SelectList(db.Subscriptions, "SubscriptionId", "SubscriptionType", user.Subscription.SubscriptionId);


                return View(model);
            }

        // POST: Dashboard/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upgrade(RegisterViewModel model, int SubscriptionId)
        {
            var userid = Session["userId"].ToString();
            User user = await UserManager.FindByIdAsync(userid);
           
                //Before downgrade chech if he is allowed to.
                if (user.SubscriptionId != SubscriptionId)
                {
                    var oldSubsSize = user.Subscription.SubscripsionSize;
                    var newSubsSize = db.Subscriptions.Where(s => s.SubscriptionId == SubscriptionId).Select(su => su.SubscripsionSize).FirstOrDefault();
                    if (oldSubsSize > newSubsSize)//he is downgrading
                    {
                        //find user's listings and check if he advertised more than allowed size
                        int uAdsCount = db.Ads.Where(ad => ad.UserId.Equals(userid )).Count();



                        if (uAdsCount > newSubsSize)
                        {
                            ViewBag.DowngradingError = "You can't downgrade now, first you have delete some listings";


                            ViewBag.SubscriptionId = new SelectList(db.Subscriptions, "SubscriptionId", "SubscriptionType", user.Subscription.SubscriptionId);
                            return View(model);
                        }
                    }
                }
                //upgrade user
                user.SubscriptionId = SubscriptionId;//Only this can change
            if (!user.SubscriptionEndDate.HasValue)
            {
                user.DatePayingStarted = DateTime.Now.Date;

            }
            //calculate subscription end date
            int daysRemaining;
            if (user.SubscriptionEndDate.HasValue)
            {
              daysRemaining = DateTime.Now.Subtract(user.SubscriptionEndDate.Value).Days;
                if (daysRemaining > 0)
                {
                    user.SubscriptionEndDate = DateTime.Now.AddMonths(1).AddDays(daysRemaining);
                }
                else
                {
                    user.SubscriptionEndDate = DateTime.Now.AddMonths(1);
                }

            }
            user.Confirmed = false;
                IdentityResult result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            

            ViewBag.SubscriptionId = new SelectList(db.Subscriptions, "SubscriptionId", "SubscriptionType", user.Subscription.SubscriptionId);
            return View(model);
        }
        
        public ActionResult ConfirmOrReject(string id, bool isReject = false, bool isConfirm = false)
        {
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            User user = db.Users.Find(id);
            if (isConfirm)
            {
                user.Confirmed = true;
                user.Rejected = false;//anaa kudaray markan confirm baroobe kadib
                user.Blocked = false;//anaa kudaray markan confirm baroobe kadib
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            else if (isReject)
            {
                Session.Add("rejectedUserId", id);
                return View("ConfirmReject", user);
            }
            return RedirectToAction("Index");
        }
        // Post: Users/ConfirmOrReject
        [HttpPost]
        public ActionResult ConfirmOrReject(String message)
        {
            var userId = Session.Contents["rejectedUserId"];
            User user = db.Users.Find(userId.ToString());
            //here send message to the user and turn rejected to true
            Message Message = new Message()
            {
                SenderId= User.Identity.GetUserId(),
                IsMessage=false,
                MessageDateTime=DateTime.Now,
                MessageContent=message,
                RecieverId = userId.ToString(),
            };
            user.Rejected = true;
            db.Entry(user).State = EntityState.Modified;
            db.Messages.Add(Message);
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<ActionResult> Unblock(string id)
        {
            User user = await UserManager.FindByIdAsync(id);
            user.Blocked = false;
            user.Confirmed = true;
            user.Rejected = false;
            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction("Index");
            return View();
        }
        // GET: Dashboard/Users/Delete/5
        [HttpGet]
        public async Task<ActionResult> Block(string id)
        {
            User user = await UserManager.FindByIdAsync(id);

            return View(user);
        }

        // POST: Dashboard/Users/Delete/5
        [HttpPost, ActionName("Block")]
        public async Task<ActionResult> BlockConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                User user = await UserManager.FindByIdAsync(id);
                user.Blocked = true;
                user.Confirmed = false;
                var result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Index");

            }
            return View();

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