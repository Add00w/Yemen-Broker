using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Yemen_Broker.ViewModels;

namespace Yemen_Broker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        [Required]
        public string UserAddress { get; set; }
        [Required]
        public string UserType { get; set; }
        public bool Confirmed { get; set; }
        public bool Rejected { get; set; }
        public bool Blocked { get; set; }
        public DateTime? DatePayingStarted { get; set; }
        public DateTime RegisteredDate { get; set; } = DateTime.Now.Date;
        public DateTime? SubscriptionEndDate { get; set; }

        [ForeignKey("Subscription")]
        public int SubscriptionId { get; set; }
        public virtual Subscription Subscription { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
            : base("YemenBrokerConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<CityModel> Cities { get; set; }

        public DbSet<Home> Homes { get; set; }

        public DbSet<Land> Lands { get; set; }

        public DbSet<Apartment> Apartments { get; set; }

        public DbSet<Warehouse> Warehouses { get; set; }

        public DbSet<Car> Cars { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Computer> Computers { get; set; }

        public DbSet<Mobile> Mobiles { get; set; }
        public DbSet<View> Views { get; set; }

        public DbSet<Wishlist> Wishlists { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Message> Messages { get; set; }

        public DbSet<Rating> Ratings { get; set; }
    }
}