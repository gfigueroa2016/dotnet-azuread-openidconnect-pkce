using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Owin;
using Owin;
using Serilog;

[assembly: OwinStartup(typeof(AzureAD.DotNet.Startup))]

namespace AzureAD.DotNet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Events)
                .CreateLogger();


            ConfigureAuth(app);

            Log.Logger.Information("Testing login message");
        }
    }
}