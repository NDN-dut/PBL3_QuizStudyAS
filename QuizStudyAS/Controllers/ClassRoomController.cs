using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using System.Threading.Tasks;

namespace QuizStudyAS.Controllers
{
    public class ClassRoomController : Controller
    {
        private readonly IClassRoomServices _ClassRoomServices;

        public ClassRoomController(IClassRoomServices ClassRoomServices)
        {
            _ClassRoomServices = ClassRoomServices;
        }

        // ACTION CHÍNH: Gom tụm toàn bộ logic hiển thị vào đây
        public async Task<IActionResult> Index(string NameClass)
        {
            // Đẩy từ khóa ngược ra ViewBag để giữ chữ trên input text
            ViewBag.NameClass = NameClass;

            // Luôn lấy danh sách lớp học của tôi làm nền tảng mặc định
            var yourClassData = await _ClassRoomServices.GetYourClass();

            // Nếu người dùng có thực hiện hành động tìm kiếm
            if (!string.IsNullOrWhiteSpace(NameClass))
            {
                var searchResult = await _ClassRoomServices.FindClassRoomByName(NameClass);
                ViewBag.SearchResults = searchResult.ListClassRoom;
            }

            return View(yourClassData);
        }

        // Sửa hàm Tham gia: Sau khi xử lý xong thì Redirect về lại Index kèm từ khóa để giữ giao diện
        [HttpPost] // Chuyển thành HttpPost cho đúng bản chất tạo dữ liệu
        public async Task<IActionResult> Join(string className)
        {
            await _ClassRoomServices.CreateRequest(className);
            return RedirectToAction("Index", new { NameClass = className });
        }

        
        public async Task<IActionResult> Search() => RedirectToAction("Index");

        public async Task<IActionResult> Request()
        {
            var ListRequesData = await _ClassRoomServices.GetJoinVMs();
            return View(ListRequesData);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(string userId, int classroomId)
        {
            await _ClassRoomServices.AcceptRequest(userId, classroomId);
            return Json(new { success = true });
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
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ClassRoomDetail(string LinkLop)
        {
            var ClassRoomDetailData = await _ClassRoomServices.GetClassRoomDetail(LinkLop);
            return View(ClassRoomDetailData);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudySetToClass(string classCode, int studySetId)
        {
            bool result = await _ClassRoomServices.AddStudySet(classCode, studySetId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudySetOfClass(string classCode, int studysetid)
        {
            bool result = await _ClassRoomServices.DeleteStudySet(classCode, studysetid);
            return Json(new { success = result });
        }

        // THÊM API NÀY ĐỂ LẤY SỐ LƯỢNG THÔNG BÁO CHO SIDEBAR
        [HttpGet]
        public async Task<IActionResult> GetPendingRequestCount()
        {
            // Tận dụng lại hàm GetJoinVMs đã có sẵn
            var listRequest = await _ClassRoomServices.GetJoinVMs();
            int count = listRequest.RequestJoinVMs?.Count ?? 0;
            return Json(new { count = count });
        }
    }
}