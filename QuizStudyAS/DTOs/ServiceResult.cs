namespace QuizStudyAS.DTOs
{
    // Dành cho các tác vụ chỉ cần biết Thành công/Thất bại (Ví dụ: Đăng ký, Đổi mật khẩu)
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult IsSuccess(string message = "") => new ServiceResult { Success = true, Message = message };
        public static ServiceResult IsError(string message) => new ServiceResult { Success = false, Message = message };
    }

    // Dành cho các tác vụ cần trả về kèm Dữ liệu (Ví dụ: Đăng nhập trả về User)
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> IsSuccess(T data, string message = "") => new ServiceResult<T> { Success = true, Data = data, Message = message };
        public new static ServiceResult<T> IsError(string message) => new ServiceResult<T> { Success = false, Data = default, Message = message };
    }
}