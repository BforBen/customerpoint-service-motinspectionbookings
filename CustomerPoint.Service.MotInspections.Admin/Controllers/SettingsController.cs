using System.Web.Mvc;

namespace CustomerPoint.Service.MotInspections.Admin.Controllers
{
    [RoutePrefix("settings")]
    public class SettingsController : Controller
    {
        [Route]
        public ActionResult Index()
        {
            if (ControllerContext.IsChildAction)
            {
                return PartialView();
            }
            return View();
        }
    }
}