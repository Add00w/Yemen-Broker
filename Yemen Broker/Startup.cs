using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Yemen_Broker.Startup))]
namespace Yemen_Broker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
