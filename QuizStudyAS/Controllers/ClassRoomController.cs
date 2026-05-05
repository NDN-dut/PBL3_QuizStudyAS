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
    }
}
