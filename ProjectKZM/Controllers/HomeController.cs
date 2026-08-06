using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectKZM.Models;
using System.Diagnostics;

namespace ProjectKZM.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Test xem có lỗi Admin hay ko, đường dẫn: /Home/TestClaims (có thể xóa)
        public IActionResult TestClaims()
        {
            var result = "";

            foreach (var claim in User.Claims)
            {
                result += $"{claim.Type} = {claim.Value}<br>";
            }

            return Content(result, "text/html");
        }
    }
}
