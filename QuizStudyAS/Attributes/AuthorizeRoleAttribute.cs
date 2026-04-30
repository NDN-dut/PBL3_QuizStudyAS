using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuizStudyAS.Attributes
{
    // Attribute này cho phép bạn dùng [AuthorizeRole("Admin")] trên các Action
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string _role;
        public AuthorizeRoleAttribute(string role)
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Lấy thông tin Role từ Session
            var sessionRole = context.HttpContext.Session.GetString("UserRole");

            // 2. Kiểm tra nếu chưa đăng nhập
            if (string.IsNullOrEmpty(sessionRole))
            {
                // Đẩy về trang chủ và hiện Modal đăng nhập (hoặc trang AccessDenied)
                context.Result = new RedirectToActionResult("Index", "Home", new { authError = "Chưa đăng nhập" });
                return;
            }

            // 3. Kiểm tra quyền hạn
            if (sessionRole != _role && _role != "All")
            {
                // Trả về kết quả từ chối truy cập
                context.Result = new ViewResult { ViewName = "AccessDenied" };
            }

            base.OnActionExecuting(context);
        }
    }
}