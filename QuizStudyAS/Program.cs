using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;

var builder = WebApplication.CreateBuilder(args);

// --- THÊM DÒNG NÀY ĐỂ BƠM DbContext ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

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
            context.Database.EnsureDeleted();
            // Thay vì EnsureCreated, ta dùng Migrate để EF tự tạo bảng lịch sử, khi cấu trúc ổn định, để có thể dùng Migration
            context.Database.Migrate();

            // // 3. Gọi hàm nạp dữ liệu mẫu (Seed Data)
            // DbInitializer.Initialize(context);

            Console.WriteLine("Database đã được làm mới và nạp dữ liệu mẫu thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi khởi tạo Database: {ex.Message}");
        }
    }
}

app.Run();
