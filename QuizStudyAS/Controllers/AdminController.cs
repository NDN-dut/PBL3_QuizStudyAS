using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Attributes;
using QuizStudyAS.Services;
using QuizStudyAS.DTOs;
using System;

namespace QuizStudyAS.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

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

        // ==========================================
        // QUẢN LÝ NGƯỜI DÙNG
        // ==========================================

        [HttpGet]
        // BỔ SUNG: Tham số pageIndex (mặc định = 1)
        public IActionResult ManageUsers(string searchString, int? roleId, bool? isActive, DateTime? fromDate, DateTime? toDate, int pageIndex = 1)
        {
            int pageSize = 10; // Bạn có thể chỉnh sửa số bản ghi hiển thị trên 1 trang ở đây

            // Kiểu trả về lúc này của "users" sẽ là PaginatedList<ApplicationUser>
            var users = _adminService.GetFilteredUsers(searchString, roleId, isActive, fromDate, toDate, pageIndex, pageSize);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentRole = roleId;
            ViewBag.CurrentIsActive = isActive;
            ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");
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
            var result = _adminService.ToggleUserStatus(id, currentUserId ?? "");
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public IActionResult AddUser(string userName, string email, string password, int roleId)
        {
            var result = _adminService.AddUser(userName, email, password, roleId);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ==========================================
        // QUẢN LÝ LỚP HỌC (CLASSROOM MANAGEMENT)
        // ==========================================

        [HttpGet]
        // BỔ SUNG: Tham số pageIndex (mặc định = 1)
        public IActionResult ManageClassrooms(string searchString, bool? isActive, string? ownerName, DateTime? fromDate, DateTime? toDate, int pageIndex = 1)
        {
            int pageSize = 10;
            var classrooms = _adminService.GetFilteredClassrooms(searchString, isActive, ownerName, fromDate, toDate, pageIndex, pageSize);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentIsActive = isActive;
            ViewBag.CurrentOwnerName = ownerName;
            ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");

            return View(classrooms);
        }

        [HttpPost]
        public IActionResult ToggleClassroomStatus(int id)
        {
            var result = _adminService.ToggleClassroomStatus(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ==========================================
        // QUẢN LÝ HỌC PHẦN (STUDYSET MANAGEMENT)
        // ==========================================

        [HttpGet]
        // BỔ SUNG: Tham số pageIndex (mặc định = 1)
        public IActionResult ManageStudySets(string searchString, bool? isActive, string? ownerName, DateTime? fromDate, DateTime? toDate, int pageIndex = 1)
        {
            int pageSize = 10;
            var studySets = _adminService.GetFilteredStudySets(searchString, isActive, ownerName, fromDate, toDate, pageIndex, pageSize);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentIsActive = isActive;
            ViewBag.CurrentOwnerName = ownerName;
            ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");

            return View(studySets);
        }

        [HttpPost]
        public IActionResult ToggleStudySetStatus(int id)
        {
            var result = _adminService.ToggleStudySetStatus(id);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}