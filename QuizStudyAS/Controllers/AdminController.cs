using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Attributes;
using QuizStudyAS.Services;

namespace QuizStudyAS.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        // Chỉ tiêm IAdminService, loại bỏ hoàn toàn AppDbContext và IPasswordHasher
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var stats = _adminService.GetDashboardStats();

            ViewBag.TotalUsers = stats.TotalUsers;
            ViewBag.TotalStudySets = stats.TotalStudySets;
            ViewBag.TotalClassrooms = stats.TotalClassrooms;
            ViewBag.TotalAdmins = stats.TotalAdmins;

            return View();
        }

        [HttpGet]
        public IActionResult ManageUsers(string searchString, int? roleId)
        {
            var users = _adminService.GetFilteredUsers(searchString, roleId);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentRole = roleId;
            ViewBag.Roles = _adminService.GetAllRoles();

            return View(users);
        }

        [HttpGet]
        public IActionResult GetUser(string id)
        {
            var user = _adminService.GetUserById(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            return Json(new
            {
                success = true,
                data = new { user.Id, user.UserName, user.Email, user.RoleId }
            });
        }

        [HttpPost]
        public IActionResult EditUser(string id, string userName, int roleId)
        {
            var result = _adminService.EditUser(id, userName, roleId);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            var result = _adminService.ToggleUserStatus(id, currentUserId);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public IActionResult AddUser(string userName, string email, string password, int roleId)
        {
            var result = _adminService.AddUser(userName, email, password, roleId);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}