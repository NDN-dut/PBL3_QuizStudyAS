using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Service;
using System.Diagnostics;

namespace QuizStudyAS.Controllers
{
    public class FindClassRoomController : Controller
    {
        private readonly IFindClassRoomService _FindClassRoomService;
        public FindClassRoomController(IFindClassRoomService FindClassRoomService)
        {
            _FindClassRoomService = FindClassRoomService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Search(string NameClass)
        {

            var ClassRoomData = await _FindClassRoomService.FindClassRoomByName(NameClass);
            return View("ClassRoom",ClassRoomData);
        }
    }
}
