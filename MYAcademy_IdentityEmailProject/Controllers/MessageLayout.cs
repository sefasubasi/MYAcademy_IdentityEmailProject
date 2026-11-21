using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class MessageLayout : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
