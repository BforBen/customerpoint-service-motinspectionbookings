using System.Web;
using System.Web.Optimization;

namespace CustomerPoint.Service.MotInspections.Admin
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/assets/css").Include(
                      "~/assets/main.css"));
        }
    }
}
