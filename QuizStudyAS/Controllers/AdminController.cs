using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm thư viện này để dùng Include()
using QuizStudyAS.Attributes;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.Services;

namespace QuizStudyAS.Controllers
{
    [AuthorizeRole("Admin")] // Toàn bộ Controller này được bảo vệ
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher; // Khai báo hasher

        // Dependency Injection: Nhận DbContext từ Program.cs
        // 2. Thêm IPasswordHasher vào tham số của hàm khởi tạo
        public AdminController(AppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;

            // 3. GÁN GIÁ TRỊ: Đây chính là dòng bạn đang thiếu khiến nó bị Null!
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            // Sử dụng LINQ Count() để đếm số lượng bản ghi thực tế trong CSDL
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalStudySets = _context.StudySets.Count();
            ViewBag.TotalClassrooms = _context.Classrooms.Count();

            // Bạn có thể lấy thêm các thống kê nâng cao khác ở đây
            // Ví dụ: Số lượng Admin
            ViewBag.TotalAdmins = _context.Users.Count(u => u.RoleId == 1);

            return View();
        }

        // --- QUẢN LÝ NGƯỜI DÙNG ---
        [HttpGet]
        public IActionResult ManageUsers(string searchString, int? roleId)
        {
            // 1. Lấy toàn bộ danh sách User (chưa chạy query xuống DB vội nhờ AsQueryable)
            var usersQuery = _context.Users.Include(u => u.Role).AsQueryable();

            // 2. Nếu có nhập từ khóa tìm kiếm (Lọc theo Tên hoặc Email)
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchString) ||
                                                   u.Email.Contains(searchString));
            }

            // 3. Nếu có chọn Vai trò (Lọc theo RoleId)
            if (roleId.HasValue && roleId > 0)
            {
                usersQuery = usersQuery.Where(u => u.RoleId == roleId);
            }

            // 4. Truyền dữ liệu trạng thái hiện tại sang View để giữ nguyên giá trị ô tìm kiếm
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentRole = roleId;

            // Lấy danh sách Roles để đổ vào thẻ <select>
            ViewBag.Roles = _context.Roles.ToList();

            // 5. Thực thi query và trả về View
            return View(usersQuery.ToList());
        }

        // --- LẤY THÔNG TIN NGƯỜI DÙNG ĐỂ HIỆN LÊN MODAL SỬA ---
        [HttpGet]
        public IActionResult GetUser(string id)
        {
            var user = _context.Users.Select(u => new {
                u.Id,
                u.UserName,
                u.Email,
                u.RoleId
            }).FirstOrDefault(u => u.Id == id);

            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });
            return Json(new { success = true, data = user });
        }

        // --- XỬ LÝ SỬA NGƯỜI DÙNG ---
        [HttpPost]
        public IActionResult EditUser(string id, string userName, int roleId)
        {
            var user = _context.Users.Find(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            // Cập nhật thông tin
            user.UserName = userName;
            user.RoleId = roleId;

            _context.SaveChanges();

            return Json(new { success = true, message = "Cập nhật thông tin thành công." });
        }

        // --- XỬ LÝ KHÓA/MỞ KHÓA NGƯỜI DÙNG (SOFT DELETE) ---
        [HttpPost]
        public IActionResult DeleteUser(string id) // Giữ nguyên tên hàm để file JS cũ khỏi lỗi
        {
            var user = _context.Users.Find(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            var currentUserId = HttpContext.Session.GetString("UserId");
            if (user.Id == currentUserId)
            {
                return Json(new { success = false, message = "Bạn không thể tự khóa tài khoản của chính mình!" });
            }

            // Đảo ngược trạng thái: Đang true thành false (Khóa), đang false thành true (Mở khóa)
            user.IsActive = !user.IsActive;

            // KHÔNG dùng Remove nữa, chỉ cần SaveChanges để cập nhật
            _context.SaveChanges();

            var msg = user.IsActive ? "Đã mở khóa tài khoản thành công." : "Đã khóa tài khoản thành công.";
            return Json(new { success = true, message = msg });
        }

        // --- XỬ LÝ THÊM NGƯỜI DÙNG ---
        [HttpPost]
        public IActionResult AddUser(string userName, string email, string password, int roleId)
        {
            // 1. Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, message = "Vui lòng nhập đủ Tên tài khoản và Mật khẩu." });
            }

            // 2. Kiểm tra trùng lặp trong Database
            bool isExists = _context.Users.Any(u => u.UserName == userName || u.Email == email);
            if (isExists)
            {
                return Json(new { success = false, message = "Tên tài khoản hoặc Email đã tồn tại!" });
            }

            // 3. Tạo User mới
            var newUser = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password), // Băm mật khẩu!
                RoleId = roleId,
                //IsActive = true // Nếu bạn đã thêm cột IsActive
            };

            // 4. Lưu vào DB
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Json(new { success = true, message = "Thêm người dùng thành công!" });
        }
    }
}