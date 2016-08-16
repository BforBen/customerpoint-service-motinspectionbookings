using System.Web.Mvc;

namespace CustomerPoint.Service.MotInspections.Controllers
{
    [RoutePrefix("vehicle")]
    public class VehicleController : Controller
    {
        [Route]
        public PartialViewResult Info(string vrm)
        {
            vrm = vrm.ToUpper().Replace(" ", "");

            if (string.IsNullOrWhiteSpace(vrm))
            {
                throw new System.ArgumentNullException(vrm);
            }

            var vd = VehicleInfo.Lookup.Vehicle(vrm) ?? new VehicleInfo.Models.VehicleData();

            return PartialView(vd);
        }
    }
}