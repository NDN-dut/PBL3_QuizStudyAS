using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Service;
using System.Diagnostics;

namespace QuizStudyAS.Controllers
{
    public class ClassRoomController : Controller
    {
        private readonly IFindClassRoomService _FindClassRoomService;
        private readonly IClassroomRequestService _ClassroomRequestService;
        public ClassRoomController(IFindClassRoomService FindClassRoomService,IClassroomRequestService classroomRequestService)
        {
            _FindClassRoomService = FindClassRoomService;
            _ClassroomRequestService = classroomRequestService;
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
        public async Task<IActionResult> Join(string className)
        {
            await _ClassroomRequestService.CreateRequest(className);
            return View("ClassRoom", await _FindClassRoomService.FindClassRoomByName(className));
        }
    }
}
