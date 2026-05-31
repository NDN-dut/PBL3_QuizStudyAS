using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services;
using QuizStudyAS.ViewModels;
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

        // ĐỔI TÊN TỪ Request KHÁI NIỆM TRÙNG THÀNH JoinRequests
        public async Task<IActionResult> JoinRequests()
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

            if (ClassRoomDetailData == null) return NotFound();

            // ĐOẠN KIỂM TRA NÀY ĐANG BỊ THIẾU TRONG CODE CỦA BẠN
            if (!ClassRoomDetailData.IsActive)
            {
                return RedirectToRefererWithLockMessage("Lớp học này đã bị khóa bởi Quản trị viên hệ thống.");
            }

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

        private IActionResult RedirectToRefererWithLockMessage(string message)
        {
            TempData["LockedMessage"] = message;
            string referer = Request.Headers["Referer"].ToString();

            // Nếu không xác định được trang trước đó (ví dụ copy link dán thẳng vào trình duyệt), đẩy về Trang chủ
            if (string.IsNullOrEmpty(referer))
            {
                return RedirectToAction("Index", "Home");
            }
            return Redirect(referer);
        }
        private async Task<IActionResult> GetUserOfClass(int ClassId)
        {
            ClassRoomDetailVM result = await _ClassRoomServices.GetAllUserOfClass(ClassId);
            return Json(result);
        }
    }
}