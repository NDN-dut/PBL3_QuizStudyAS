using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace QuizStudyAS.Controllers
{
    public class FindClassRoomController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Search(string NameClass)
        {

            return View();
        }
    }
}
