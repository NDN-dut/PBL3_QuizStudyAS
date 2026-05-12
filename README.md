# 📚 QuizStudyAS - Interactive Study & Flashcard Platform

![.NET Core](https://img.shields.io/badge/.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET MVC](https://img.shields.io/badge/ASP.NET%20MVC-0088CC?style=for-the-badge&logo=asp.net&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/Entity_Framework-339933?style=for-the-badge&logo=nuget&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)

## 📌 Giới thiệu (About The Project)
**QuizStudyAS** là một hệ thống ứng dụng web được phát triển nhằm hỗ trợ quá trình học tập và ôn thi. Ứng dụng cung cấp các công cụ tạo bộ thẻ ghi nhớ (Flashcards), quản lý tiến độ học tập (Learning Progress), tổ chức lớp học (Classrooms) và các bài kiểm tra trắc nghiệm tương tác (Game Sessions). 

Đây là sản phẩm phục vụ cho đồ án Project-Based Learning (PBL3), được thiết kế với trọng tâm vào luồng dữ liệu chặt chẽ và trải nghiệm người dùng tối ưu.

## 🚀 Kỹ thuật & Kiến trúc nổi bật (Technical Highlights)
Thay vì chỉ viết code trên Controller, dự án này được thiết kế với tư duy chia tách trách nhiệm rõ ràng (Separation of Concerns), điểm cộng lớn đối với hệ thống Enterprise:
* **Architecture:** Mô hình MVC (Model-View-Controller) kết hợp chặt chẽ với Service Pattern.
* **Dependency Injection (DI):** Đăng ký và tiêm các dependencies (như `IAuthService`, `IEmailService`, `IAdminService`) giúp code dễ dàng bảo trì và Unit Test.
* **Security:** Tích hợp `BCryptPasswordHasher` để mã hóa mật khẩu an toàn. Phân quyền (Role-based Authorization) với Custom Attributes (`AuthorizeRoleAttribute`).
* **Database ORM:** Sử dụng Entity Framework Core (Code-First Approach) với các file Configuration (`IEntityTypeConfiguration`) riêng biệt cho từng Entity để giữ `AppDbContext` gọn gàng.

## 🔑 Các tính năng chính (Key Features)
- **Hệ thống Tài khoản (Authentication):** Đăng ký, Đăng nhập, Quên mật khẩu (gửi mã qua Email Service).
- **Quản lý Lớp học (Classrooms):** Tạo lớp, yêu cầu tham gia (`RequestJoinClass`), duyệt thành viên.
- **Tài nguyên Học tập (Study Sets & Flashcards):** Tạo, chỉnh sửa bộ từ vựng/kiến thức, trình bày dưới dạng thẻ lật.
- **Đánh giá & Trắc nghiệm (Quiz & Game Sessions):** Tham gia các session làm bài, lưu kết quả từng câu hỏi (`QuizQuestionResult`).
- **Phân tích (Learning Progress):** Theo dõi tiến độ học tập và tỉ lệ hoàn thành của người dùng.
- **Admin Dashboard:** Quản lý toàn bộ người dùng, bộ học liệu và lớp học trên hệ thống.

## 🛠 Hướng dẫn Cài đặt & Chạy dự án (Getting Started)

**Yêu cầu hệ thống:**
* [.NET SDK 8.0](https://dotnet.microsoft.com/download) (Hoặc phiên bản nhóm đang dùng)
* SQL Server
* Visual Studio 2022 / Visual Studio Code

Dưới đây là các bước để cài đặt và chạy hệ thống trên môi trường local của bạn:

**1. Clone repository:**
Mở terminal hoặc Git Bash và chạy lệnh sau để tải source code về máy:

git clone [https://github.com/your-username/pbl3_quizstudyas.git](https://github.com/your-username/pbl3_quizstudyas.git)

**2. Mở Solution:**
Khởi động Visual Studio và mở file QuizStudyAS.slnx nằm trong thư mục dự án vừa tải về.

**3. Thiết lập chuỗi kết nối cơ sở dữ liệu:**

Tìm đến file appsettings.json (hoặc copy nội dung từ appsettings.Example.json nếu chưa có).

Thay đổi chuỗi kết nối DefaultConnection sao cho phù hợp với cấu hình SQL Server trên máy của bạn.

(Tùy chọn) Cấu hình thêm thông tin SMTP cho Email Service nếu bạn muốn test tính năng gửi mail khôi phục mật khẩu.

**4. Áp dụng Migrations để tạo Database:**
Mở công cụ Package Manager Console (PMC) trong Visual Studio và chạy lệnh:

PowerShell
Update-Database
(Hoặc nếu bạn sử dụng .NET CLI ở terminal, hãy chạy: dotnet ef database update)

**5. Build và Run dự án:**
Nhấn F5 hoặc nút Run trên Visual Studio. Hệ thống DbInitializer sẽ tự động chạy và seed (khởi tạo) dữ liệu tài khoản Admin mặc định cùng các Role cần thiết nếu đã được cấu hình.

📸 Giao diện ứng dụng (Screenshots)
Dưới đây là một số hình ảnh thực tế về giao diện và tính năng của hệ thống:

Trang chủ (Homepage)
Mô tả ngắn gọn: Giao diện tổng quan nơi người dùng bắt đầu các phiên học và xem danh sách lớp.

Giao diện học Flashcard
Mô tả ngắn gọn: Màn hình tương tác lật thẻ nhớ giúp học viên ôn tập hiệu quả.

Dashboard Quản lý (Admin/Teacher)
Mô tả ngắn gọn: Nơi giáo viên hoặc Admin quản lý danh sách học viên, bộ đề và tiến độ học tập.

🤝 Đội ngũ phát triển (Team)
Dự án PBL3 này được thiết kế và phát triển bởi sinh viên Bách Khoa Đà Nẵng:

🧑‍💻 Hồ Hoàng Phong Hào * Vai trò: xử lí logic cho chức năng tạo và học bằng flashcard , thiết kế UI và FrontEnd
GitHub: https://github.com/HaoVann

👩‍💻 Nguyễn Danh Ngôn * Vai trò: thiết kế database , tổ chức hệ thống , xử lí chức năng đăng nhập

GitHub: https://github.com/NDN-dut

🧑‍💻 Nguyễn Hữu Minh Khoa * Vai trò: tổ chức các chức năng liên quan đến tạo lớp học
GitHub: https://github.com/simpboy2k6
