using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Thư viện cần để dùng IFormFile

namespace QuizStudyAS.ViewModels
{
    public class UserProfileVM
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = "Tên người dùng không được để trống")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }

        // --- 2 TRƯỜNG MỚI ĐƯỢC THÊM VÀO ---
        // 1. Dùng để hiển thị ảnh cũ
        public string? ExistingAvatarUrl { get; set; }

        // 2. Dùng để hứng tệp ảnh mới người dùng upload lên
        [DataType(DataType.Upload)]
        public IFormFile? AvatarFile { get; set; }
    }
}