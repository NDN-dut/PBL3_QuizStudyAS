using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// --- THÊM DÒNG NÀY ĐỂ BƠM DbContext ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Đăng ký Service mã hóa mật khẩu
builder.Services.AddScoped<IPasswordHasher, PlainTextPasswordHasher>();

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

// Kích hoạt tính năng sử dụng Cookie để xác thực
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Nếu người dùng chưa đăng nhập mà cố tình vào URL cấm, đẩy về đây
        options.LoginPath = "/Auth/Login";

        // Nếu đăng nhập rồi nhưng không đủ quyền (ví dụ Student vào trang Admin), đẩy về đây
        options.AccessDeniedPath = "/Auth/AccessDenied";

        // Thời gian sống của Cookie (ví dụ: 7 ngày)
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddScoped<IFindClassRoomService, FindClassRoomService>();
builder.Services.AddScoped<IClassroomRequestService, ClassroomRequestService>();
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

            // 1. Xóa Database cũ (Drop)
            context.Database.EnsureDeleted();

            // 2. Tạo lại Database mới hoàn toàn (Create)
            context.Database.EnsureCreated();
            //// Thay vì EnsureCreated, ta dùng Migrate để EF tự tạo bảng lịch sử, khi cấu trúc ổn định, để có thể dùng Migration
            //context.Database.Migrate();

            // 3. Gọi hàm nạp dữ liệu mẫu (Seed Data)
            DbInitializer.Initialize(context);

            Console.WriteLine("Database đã được làm mới và nạp dữ liệu mẫu thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi khởi tạo Database: {ex.Message}");
        }
    }
}

app.Run();
