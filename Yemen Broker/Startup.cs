using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System;
using Yemen_Broker.Models;

[assembly: OwinStartupAttribute(typeof(Yemen_Broker.Startup))]
namespace Yemen_Broker
{
    public partial class Startup
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateDefaultRolesAndUsers();
        }
        public void CreateDefaultRolesAndUsers()
        {
            var rolemanager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var usermanager = new UserManager<User>(new UserStore<User>(db));

            var checkUser = usermanager.FindByEmail("Admin@YB.com");

            if (checkUser == null)
            {
                if (!rolemanager.RoleExists("Admin"))
                {
                    IdentityRole role = new IdentityRole();
                    role.Name = "Admin";
                    rolemanager.Create(role);
                }

                User user = new User();
                user.UserName = "Admin";
                user.Email = "Admin@YB.com";
                user.PhoneNumber = "775263811";
                user.RegisteredDate = DateTime.Now;
                user.SubscriptionId = 1;
                user.UserType = "Admin";
                user.UserAddress = "Mukalla";
                var check = usermanager.Create(user, "Passw0rd!");
                if (check.Succeeded)
                {
                    usermanager.AddToRole(user.Id, "Admin");
                }
            }
        }
    }
}
