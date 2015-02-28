using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(mydin.Startup))]
namespace mydin
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
