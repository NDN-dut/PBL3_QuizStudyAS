using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// --- THÊM DÒNG NÀY ĐỂ BƠM DbContext ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Service mã hóa mật khẩu
//builder.Services.AddScoped<IPasswordHasher, PlainTextPasswordHasher>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

// Đăng ký Service cho Đăng nhập, Đăng ký
builder.Services.AddScoped<IAuthService, AuthService>();
// Đăng ký Service cho Admin
builder.Services.AddScoped<IAdminService, AdminService>();

// Đăng ký dịch vụ gửi Email
builder.Services.AddScoped<IEmailService, EmailService>();

// Đăng ký StudySet Service
builder.Services.AddScoped<IStudySetService, StudySetService>();

// Đăng kí LeaderBoardService
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

// Thêm cấu hình Authentication (Đã gộp chung Cookie và Google)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied"; // Đường dẫn báo lỗi quyền truy cập
    options.ExpireTimeSpan = TimeSpan.FromDays(7);   // Thời gian sống của Cookie 7 ngày
})
.AddGoogle(options =>
{
    // Thông tin này sẽ được lấy từ file appsettings.json
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "Vui-long-cai-dat-ClientId";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "Vui-long-cai-dat-ClientSecret";
    //options.CallbackPath = "/Auth/GoogleResponse";
});

// 1. ĐĂNG KÝ DỊCH VỤ SESSION (Thêm đoạn này)
builder.Services.AddDistributedMemoryCache(); // Bộ nhớ tạm để lưu Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Session sống 60 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// THÊM ĐOẠN NÀY ĐỂ VIEW (HTML) ĐỌC ĐƯỢC SESSION
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllersWithViews();

//// Kích hoạt tính năng sử dụng Cookie để xác thực
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        // Nếu người dùng chưa đăng nhập mà cố tình vào URL cấm, đẩy về đây
//        options.LoginPath = "/Auth/Login";

//        // Nếu đăng nhập rồi nhưng không đủ quyền (ví dụ Student vào trang Admin), đẩy về đây
//        options.AccessDeniedPath = "/Auth/AccessDenied";

//        // Thời gian sống của Cookie (ví dụ: 7 ngày)
//        options.ExpireTimeSpan = TimeSpan.FromDays(7);
//    });

builder.Services.AddScoped<IClassRoomServices, ClassRoomServices>();

builder.Services.AddScoped<IGamificationService, GamificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 2. KÍCH HOẠT SESSION (Bắt buộc phải nằm GIỮA UseRouting và UseAuthorization)
app.UseSession();

// Thêm dòng này nếu chưa có
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// --- Logic khởi tạo và Nạp dữ liệu mẫu ---
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();

            // ĐÃ SỬA: Lấy Hasher từ hệ thống ra
            var hasher = services.GetRequiredService<IPasswordHasher>();

            // context.Database.EnsureDeleted(); // Cứ đóng comment dòng này cho an toàn
            context.Database.Migrate();

            // ĐÃ SỬA: Truyền 'hasher' vào làm tham số thứ 2
            DbInitializer.Initialize(context, hasher);

            Console.WriteLine("Database đã được làm mới và nạp dữ liệu mẫu thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi khởi tạo Database: {ex.Message}");
        }
    }
}


app.Run();
