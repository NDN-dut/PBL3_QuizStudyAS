using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Attributes;
using QuizStudyAS.Services.ClassRoom;
using QuizStudyAS.ViewModels;
using System.Threading.Tasks;

namespace QuizStudyAS.Controllers
{
    [AuthorizeRole("All")]
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
        public async Task<IActionResult> Join(string classCode, string? searchName)
        {
            await _ClassRoomServices.CreateRequest(classCode);
            return RedirectToAction("Index", new { NameClass = searchName });
        }


        public async Task<IActionResult> Search() => RedirectToAction("Index");

        // ĐỔI TÊN TỪ Request KHÁI NIỆM TRÙNG THÀNH JoinRequests
        public async Task<IActionResult> JoinRequests()
        {
            var listRequestData = await _ClassRoomServices.GetJoinVMs();
            return View(listRequestData);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(string userId, int classroomId)
        {
            var result = await _ClassRoomServices.AcceptRequest(userId, classroomId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> DeniedRequest(string userId, int classroomId)
        {
            var result = await _ClassRoomServices.DeniedRequest(userId, classroomId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateClassRoom(string ClassName)
        {
            await _ClassRoomServices.CreateClassRoom(ClassName);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ClassRoomDetail(string? classCode, string? LinkLop)
        {
            string targetCode = classCode ?? LinkLop ?? "";
            if (string.IsNullOrEmpty(targetCode)) return NotFound();

            if (!await _ClassRoomServices.IsMemberOrOwner(targetCode))
            {
                return RedirectToRefererWithLockMessage("Bạn chưa phải là thành viên của lớp học này. Vui lòng gửi yêu cầu tham gia và chờ phê duyệt!");
            }

            var ClassRoomDetailData = await _ClassRoomServices.GetClassRoomDetail(targetCode);

            if (ClassRoomDetailData == null) return NotFound();

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
        public async Task<IActionResult> DeleteUserOfClass(string classcode,string userid)
        {
            bool result = await _ClassRoomServices.DeleteUserOfClass(classcode, userid);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudySetOfClass(string classCode, int studysetid)
        {
            bool result = await _ClassRoomServices.DeleteStudySet(classCode, studysetid);
            return Json(new { success = result });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteClass(string classcode)
        {
            bool result = await _ClassRoomServices.DeleteClassRoom(classcode);
            return Json(new {success= result });
        }

        [HttpPost]
        public async Task<IActionResult> LeaveClass(string classCode)
        {
            bool result = await _ClassRoomServices.LeaveClassRoom(classCode);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> RegenerateInviteCode(string classCode)
        {
            string? newCode = await _ClassRoomServices.RegenerateInviteCode(classCode);
            if (newCode == null)
            {
                return Json(new { success = false, message = "Không có quyền thực hiện thao tác này." });
            }
            return Json(new { success = true, newCode = newCode });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(string classCode, string content)
        {
            bool result = await _ClassRoomServices.CreatePost(classCode, content);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int postId)
        {
            bool result = await _ClassRoomServices.DeletePost(postId);
            return Json(new { success = result });
        }
        
        [HttpGet]
        public async Task<IActionResult> GetPendingRequestCount()
        {
            var listRequest = await _ClassRoomServices.GetJoinVMs();
            int count = listRequest.RequestJoinVMs?.Count ?? 0;
            return Json(new { count = count });
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
        [HttpGet]
        public async Task<IActionResult> GetUserOfClass(string classcode)
        {
            if (string.IsNullOrEmpty(classcode) || !await _ClassRoomServices.IsMemberOrOwner(classcode))
            {
                return Json(new { classUsers = new List<object>() });
            }

            ClassRoomDetailVM result = await _ClassRoomServices.GetAllUserOfClass(classcode);
            return Json(result);
        }
    }
}