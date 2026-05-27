using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.ViewModels;
// Thư viện cần thêm
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace QuizStudyAS.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        // --- THAY ĐỔI 1: Khai báo Environment ---
        private readonly IWebHostEnvironment _environment;

        // Cập nhật Constructor để tiêm Environment
        public UserController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /User/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var vm = new UserProfileVM
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                // --- THAY ĐỔI 2: Map ảnh cũ sang VM ---
                ExistingAvatarUrl = user.AvatarUrl,

                XP = user.XP,
                Level = user.Level,
                CurrentStreak = user.CurrentStreak,
                HighestStreak = user.HighestStreak
            };

            return View(vm);
        }

        // POST: /User/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == vm.UserId);
            if (user == null) return NotFound();

            user.UserName = vm.UserName;
            user.Email = vm.Email;

            // --- THAY ĐỔI 3: XỬ LÝ UPLOAD AVATAR MỚI ---
            if (vm.AvatarFile != null && vm.AvatarFile.Length > 0)
            {
                // 1. Định nghĩa thư mục lưu trữ: wwwroot/uploads/avatars/
                string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // 2. Tạo tên tệp tin duy nhất (UserId_Guid_Name.jpg)
                string fileName = $"{user.Id}_{Guid.NewGuid()}_{Path.GetFileName(vm.AvatarFile.FileName)}";
                string filePath = Path.Combine(uploadDir, fileName);

                // 3. Lưu tệp mới vào thư mục
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.AvatarFile.CopyToAsync(fileStream);
                }

                // 4. Xóa ảnh cũ (nếu có và không phải ảnh mặc định) để tiết kiệm dung lượng
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    // Chuyển URL (/uploads/avatars/...) thành đường dẫn vật lý (C:\...)
                    string oldFilePath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // 5. Cập nhật đường dẫn URL mới vào database
                user.AvatarUrl = $"/uploads/avatars/{fileName}";
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", vm.UserName);

            TempData["SuccessMessage"] = "Cập nhật thông tin và ảnh đại diện thành công!";
            return RedirectToAction(nameof(Profile));
        }
    }
}