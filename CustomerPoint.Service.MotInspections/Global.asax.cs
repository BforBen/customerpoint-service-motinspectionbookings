using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Serilog;
using SerilogWeb.Classic.Enrichers;

namespace CustomerPoint.Service.MotInspections
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var Logger = new LoggerConfiguration()
                .WriteTo.Seq(Properties.Settings.Default.SeqLogUrl, apiKey: Properties.Settings.Default.SeqLogKey, bufferBaseFilename: AppDomain.CurrentDomain.BaseDirectory + @"App_Data\Logs")
                .Enrich.WithMachineName()
                .Enrich.With<HttpRequestUrlEnricher>()
                .Enrich.WithThreadId()
                .Enrich.With<HttpRequestClientHostIPEnricher>()
                .Enrich.WithProcessId()
                .Enrich.With<HttpRequestIdEnricher>()
                .Enrich.With<HttpRequestTypeEnricher>()
                .Enrich.With<HttpRequestUserAgentEnricher>()
                .Enrich.With<HttpRequestUrlReferrerEnricher>();

            Log.Logger = Logger.CreateLogger();
        }
    }
}
