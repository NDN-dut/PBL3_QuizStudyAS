using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using System.Diagnostics;

namespace QuizStudyAS.Controllers
{
    public class ClassRoomController : Controller
    {
        private readonly IClassRoomServices _ClassRoomServices;
        
        public ClassRoomController(IClassRoomServices ClassRoomServices)
        {
            _ClassRoomServices = ClassRoomServices;
            
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Search(string NameClass)
        {

            var ClassRoomData = await _ClassRoomServices.FindClassRoomByName(NameClass);
            return View("ClassRoom",ClassRoomData);
        }
        public async Task<IActionResult> Join(string className)
        {
            await _ClassRoomServices.CreateRequest(className);
            return View("ClassRoom", await _ClassRoomServices.FindClassRoomByName(className));
        }
        public async Task<IActionResult> YourClass()
        {
            var YourClassData = await _ClassRoomServices.GetYourClass();
            return View(YourClassData);
        }
        public async Task<IActionResult> Request()
        {
            var ListRequesData = await _ClassRoomServices.GetJoinVMs();
            return View(ListRequesData);
        }
        [HttpPost]
        public async Task<IActionResult> AcceptRequest(string userId, int classroomId)
        {
            await _ClassRoomServices.AcceptRequest(userId, classroomId);
            return Json( new { success = true } ) ;
        }
        [HttpPost]
        public async Task<IActionResult> DeniedRequest(string userId, int classroomId)
        {
            await _ClassRoomServices.DeninedRequest(userId, classroomId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> CreateClassRoom(string ClassName)
        {
            await _ClassRoomServices.CreateClassRoom(ClassName);
            return RedirectToAction("YourClass", "ClassRoom");
        }
        public async Task<IActionResult> ClassRoomDetail(string LinkLop)
        {
            var ClassRoomDetailData = await _ClassRoomServices.GetClassRoomDetail(LinkLop);
            
            return View(ClassRoomDetailData);
        }
    }
}
