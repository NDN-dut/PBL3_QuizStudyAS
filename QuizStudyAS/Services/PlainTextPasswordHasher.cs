namespace QuizStudyAS.Services
{
    public class PlainTextPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            // TẠM THỜI: Trả về y nguyên mật khẩu gốc để dễ nhìn trong Database
            return password;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            // TẠM THỜI: So sánh chuỗi trực tiếp
            return password == hashedPassword;
        }
    }
}
