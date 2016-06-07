using System.Web.Mvc;

namespace CustomerPoint.Service.MotInspections.Admin.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("~/")]
        public ActionResult Index()
        {
            return View();
        }
    }
}