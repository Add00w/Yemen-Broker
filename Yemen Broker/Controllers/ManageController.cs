using System;
using System.Linq;
using System.Net;
using System.Data.SqlClient;

using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Yemen_Broker.Models;

namespace Yemen_Broker.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationDbContext db=new ApplicationDbContext();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        [Authorize()]
        public async Task<ActionResult> Upgrade(string id)
        {

            RegisterViewModel model = new RegisterViewModel();

            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
                return HttpNotFound();

            //TempData["userId"]= id;
            model.UserId = id;

            model.UserAddress = user.UserAddress;
            model.Email = user.Email;
            model.UserName = user.UserName;
            model.PhoneNumber = user.PhoneNumber;



            ViewBag.SubscriptionId = new SelectList(db.Subscriptions, "SubscriptionId", "SubscriptionType", user.Subscription.SubscriptionId);


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize()]
        public async Task<ActionResult> Upgrade(RegisterViewModel model, int SubscriptionId)
        {
            //var userid = TempData["userId"] as string;         

            var userid = model.UserId;

            User user = await UserManager.FindByIdAsync(userid);

            //Before downgrade chech if he is allowed to.
            if (user.SubscriptionId != SubscriptionId)
            {
                var oldSubsSize = user.Subscription.SubscripsionSize;
                var newSubsSize = db.Subscriptions.Where(s => s.SubscriptionId == SubscriptionId).Select(su => su.SubscripsionSize).FirstOrDefault();
                if (oldSubsSize > newSubsSize)//he is downgrading
                {
                    //find user's listings and check if he advertised more than allowed size
                    int uAdsCount = db.Ads.Where(ad => ad.UserId.Equals(userid)).Count();



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

        // GET: /Manage/Editprofile
        [Authorize]
        public async Task<ActionResult> Editprofile(string id)
        {
            RegisterViewModel model = new RegisterViewModel();

            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
                return HttpNotFound();

            //TempData["userId"]= id;
            model.UserId = id;


            model.UserAddress = user.UserAddress;
            model.Email = user.Email;
            model.UserName = user.UserName;
            model.PhoneNumber = user.PhoneNumber;


            ViewBag.SubscriptionId = new SelectList(db.Subscriptions, "SubscriptionId", "SubscriptionType", user.Subscription.SubscriptionId);


            return View(model);
        }

        // Post: /Manage/Editprofile
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Editprofile(RegisterViewModel model)
        {
            //var userId = TempData["userId"] as string;

            var userId = model.UserId;


            if (string.IsNullOrEmpty(userId)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
                return HttpNotFound();

           
               

                user.UserAddress = model.UserAddress;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;

                IdentityResult result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            
            ViewBag.SubscriptionId = new SelectList(db.Subscriptions, "SubscriptionId", "SubscriptionType", user.Subscription.SubscriptionId);


            return View(model);
        }


        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";
            //anagaa leh start
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            double myAdsCount = db.Ads.Where(a => a.UserId.Equals(userId)).Count();
            double myOrdersCount = db.Orders.Where(o => o.UserId.Equals(userId)).Count();
            double myFavoritesCount = db.Wishlists.Where(w => w.UserId.Equals(userId)).Count();
            double size = user.Subscription.SubscripsionSize;
            double persentage = (myAdsCount / size)*100;
            //end
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                MyAdsCount=myAdsCount,//.........
                MyFavoritesCount=myFavoritesCount,//.........
                MyOrdersCount =myOrdersCount,//.........
                User =user,//.........
                Percentage = persentage //.........
            };
            return View(model);
        }
        //[HttpPost]
        public ActionResult Backup()
        {
            var BackupPath = "D:\\YemenBBackup";
            var con = db.Database.Connection;
            var sql = $"BACKUP DATABASE [{db.Database.Connection.Database}]"
                + $" TO DISK = N'{BackupPath}\\{DateTime.Now.ToString().Replace('/', '-').Replace(':', '-').Replace(" ", "")}.bak'"
                + $"WITH NOFORMAT, NOINIT, SKIP, STATS = 10; ";

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = (SqlConnection)con;
            cmd.CommandText = sql;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                TempData["Success"] = "تمت أخذ نسخة احتياطية بنجاح";
                con.Close();
            }
            catch (Exception ex)
            {
                TempData["ErrorDetails"] = ex/*.GetErrorMessage()*/;
                TempData["failure"] = "حدث خطأ لم يتم أخذ نسخة احتياطية";
            }
            return RedirectToAction("BackupAndRestore");
        }
        public ActionResult Restore(string fileName)
        {
            var BackupPath = "D:\\YemenBBackup";
            var conStr = db.Database.Connection.ConnectionString;//.Replace(db.Database.Connection.Database, "master");
            var sql = $"alter database [{db.Database.Connection.Database}] set offline with rollback immediate "
                + $"RESTORE DATABASE [{db.Database.Connection.Database}]"
                + $" From DISK = N'{BackupPath}\\{fileName}'"
                + $"WITH FILE = 1, NOUNLOAD, REPLACE, STATS = 10 "
                + $"alter database [{db.Database.Connection.Database}] set online";
            SqlConnection con = new SqlConnection(conStr);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = sql;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                TempData["Success"] = "تمت استرجاع النسخة الاحتياطية بنجاح";
                con.Close();
            }
            catch (Exception ex)
            {
                TempData["ErrorDetails"] = ex/*.GetErrorMessage()*/;
                TempData["failure"] = "حدث خطأ لم يتم استرجاع النسخة الاحتياطية";
            }
            return RedirectToAction("BackupAndRestore");
        }
        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }
        public ActionResult BackupAndRestore()
        {
            return View();
        }
        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}