using Microsoft.AspNetCore.Mvc;

namespace EasyLoan.Api.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
