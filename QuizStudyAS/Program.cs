using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Services.Admin;
using QuizStudyAS.Services.Ai;
using QuizStudyAS.Services.Auth;
using QuizStudyAS.Services.ClassRoom;
using QuizStudyAS.Services.Email;
using QuizStudyAS.Services.Exam;
using QuizStudyAS.Services.Gamification;
using QuizStudyAS.Services.Leaderboard;
using QuizStudyAS.Services.StudySet;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IStudySetService, StudySetService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddHttpClient<IAiService, AiService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "Vui-long-cai-dat-ClientId";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "Vui-long-cai-dat-ClientSecret";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IClassRoomServices, ClassRoomServices>();
builder.Services.AddScoped<IGamificationService, GamificationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var hasher = services.GetRequiredService<IPasswordHasher>();
            context.Database.Migrate();
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
