using BCrypt.Net;

namespace QuizStudyAS.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            // Tự động sinh Salt (mặc định là Work Factor 11) và băm mật khẩu
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Tự động tách Salt từ hashedPassword trong DB ra để đối chiếu với password người dùng nhập
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // Bắt lỗi trong trường hợp hashedPassword trong DB là chuỗi thường (chưa được hash bằng BCrypt)
                return false;
            }
        }
    }
}