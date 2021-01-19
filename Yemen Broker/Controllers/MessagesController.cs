using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yemen_Broker.Models;

namespace Yemen_Broker.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Messages
        public ActionResult Index()
        {
            var currentUserId = User.Identity.GetUserId();
            var messages = db.Messages.OrderBy(m => m.MessageDateTime)
                .Where(m => m.SenderId.Equals(currentUserId) || m.RecieverId.Equals(currentUserId));

            return View(messages);
        }
        // GET: Messages/Chat/id
        public ActionResult Chat(string id)
        {
            ViewBag.ReceiverId = id;
            var SenderId = User.Identity.GetUserId();
            var messages = db.Messages.OrderBy(m=>m.MessageDateTime)
                .Where(m => (m.SenderId.Equals(SenderId) && m.RecieverId.Equals(id))|| (m.SenderId.Equals(id) && m.RecieverId.Equals(SenderId)));
            return View(messages);
        }
        [HttpPost, ActionName("Chat")]
        public ActionResult ChatPost(string message,string id)
        {
            var SenderId = User.Identity.GetUserId();
            //string RecieverId = Session["ReceiverId"].ToString();
            if (string.IsNullOrEmpty(message)) return View();
            Message Message = new Message()
            {
                MessageContent = message,
                SenderId = SenderId,
                RecieverId = id,
                MessageDateTime = DateTime.Now,
                User = db.Users.Find(SenderId),
                Advertiser = db.Users.Find(id),
                IsMessage = true
                
            };
            db.Messages.Add(Message);
            db.SaveChanges();
            var messages = db.Messages.OrderBy(m => m.MessageDateTime)
                           .Where(m => (m.SenderId.Equals(SenderId) && m.RecieverId.Equals(id)) || (m.SenderId.Equals(id) && m.RecieverId.Equals(SenderId)));

            return View(messages);
        }
        // GET: Messages/Notifications
        public ActionResult Notifications()
        {
            var currentUserId = User.Identity.GetUserId();
            //note: get notifications for one user 
            var notifications = db.Messages.Where(n => !n.IsMessage && (n.RecieverId.Equals(currentUserId)||n.SenderId.Equals(currentUserId)));
            return View(notifications);
        }
    }
}