using System.Web.Optimization;

namespace CustomerPoint.Service.MotInspections
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/assets/css").Include(
                      "~/assets/main.css",
                      "~/assets/local.css"));
        }
    }
}
