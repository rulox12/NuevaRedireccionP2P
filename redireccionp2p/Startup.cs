using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(redireccionp2p.Startup))]
namespace redireccionp2p
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
