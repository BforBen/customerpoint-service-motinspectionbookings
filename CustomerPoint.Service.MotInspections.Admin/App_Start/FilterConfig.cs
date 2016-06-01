using System.Web;
using System.Web.Mvc;

namespace CustomerPoint.Service.MotInspections.Admin
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
