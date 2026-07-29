# 📚 QuizStudyAS

**QuizStudyAS** is a comprehensive, interactive study and flashcard platform designed to seamlessly connect learning, classroom management, and online examinations. Developed as a **PBL3 (Project-Based Learning)** capstone project.

## 🚀 Overview

**QuizStudyAS** empowers both students and educators to:
- Create, manage, and master **flashcard sets**.
- Organize **virtual classrooms** and share learning materials.
- Conduct **timed multiple-choice exams** (with automated CSV import capabilities).
- Track progress dynamically via **XP, daily streaks, and leaderboards**.
- Interact with a smart **AI study assistant** powered by Google Gemini.

## 🛠 Tech Stack

- **Backend Framework:** ASP.NET Core 10 (.NET 10) — MVC
- **Frontend UI:** Razor Views, Bootstrap 5, Bootstrap Icons, Custom CSS/JS
- **Database & ORM:** SQL Server + Entity Framework Core 10 (Code-First)
- **Authentication:** Cookie Auth + Google OAuth
- **Security:** BCrypt password hashing, Role-based authorization
- **External Services:** SMTP (Password Recovery), Google Gemini API (AI Chatbot)

## 🏗 Architecture & Patterns

The application is built upon an **MVC + Service Layer** architecture to ensure clean separation of concerns, scalability, and maintainability.

```text
Controllers  →  Services  →  AppDbContext  →  SQL Server
     ↓
  Views / ViewModels / DTOs
```

**Key Implementation Patterns:**
- **Dependency Injection (DI):** Centralized service registration in `Program.cs`.
- **Service Pattern:** Business logic is encapsulated in the `Services/` layer, ensuring thin and highly testable controllers.
- **EF Core Configurations:** Explicit entity mappings utilizing Fluent API decoupled into `Data/Configurations/`.

## ✨ Key Features

| Feature | Description |
|---|---|
| 🔐 **Authentication** | Register, Login, Google OAuth integration, Password recovery via Email (SMTP). |
| 🗂️ **Study Sets** | Create/manage Flashcard sets, Learn mode with intuitive card flipping effects. |
| 🏫 **Classrooms** | Classroom space management: Invite codes, member approval system, shared learning materials. |
| 📝 **Exams** | Timed multiple-choice exams, automatic exam import from `.csv` files, exam history tracking. |
| 🎮 **Gamification** | Experience points (XP) system, levels, learning streaks, automatic leaderboards. |
| 🤖 **AI Chatbot** | Smart virtual learning assistant utilizing the power of the Google Gemini API. |
| ⚙️ **Admin Panel** | Centralized administration dashboard for Admins to manage Users, Classrooms, and Study Sets. |

## ⚙️ Getting Started

### 1. Requirements
- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- SQL Server 
- Visual Studio 2022 / Visual Studio Code

### 2. Clone the repository
```bash
git clone https://github.com/NDN-dut/PBL3_QuizStudyAS.git
cd PBL3_QuizStudyAS/QuizStudyAS
```

### 3. Application Configuration
Create an `appsettings.json` file in the root directory `QuizStudyAS` (Do not commit real API keys to version control).

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=QuizStudyAS_DB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "QSAS System",
    "SenderEmail": "your-email@gmail.com",
    "AppPassword": "your-app-password"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  },
  "GeminiSettings": {
    "ApiKey": "your-gemini-api-key",
    "Model": "gemini-2.5-flash"
  }
}
```

### 4. Database Initialization
Apply Entity Framework Core migrations to build the database schema.

Using .NET CLI:
```bash
dotnet ef database update
```
*Alternatively, using Package Manager Console in Visual Studio:*
```powershell
Update-Database
```

### 5. Run the Application
```bash
dotnet run
```
*Note: In the Development environment, the application automatically runs migrations and seeds default accounts/sample classrooms via `DbInitializer` upon startup.*

## 🧑‍💻 Team Development

| Member | Tasks & Technical Implementation |
|------------|--------------------------------|
| **Hồ Hoàng Phong Hào** | **Flashcard Logic, UI & Frontend**<br>- Design and develop the user interface (UI) using **Razor Views**, **Bootstrap 5**, and custom CSS/JS.<br>- Implement Flashcard, Flip card, and Learn mode feature logic. Handle client-side interactions and render data from Controllers to Views. |
| **Nguyễn Danh Ngôn** | **Database Design, Authentication & System Architecture**<br>- Design the **SQL Server** database system using **Entity Framework Core 10 (Code-First)**.<br>- Set up the overall system architecture following the **MVC + Service Layer** pattern, integrating **Dependency Injection**.<br>- Implement security and authorization: **Cookie Auth**, **Google OAuth**, password hashing with **BCrypt**, and password recovery via **SMTP**. |
| **Nguyễn Hữu Minh Khoa** | **Classroom & Exam Features**<br>- Build the Classroom management module (create classes, invite codes, approve join requests) leveraging MVC architecture.<br>- Develop the timed multiple-choice exam system, track results, and manage exam history.<br>- Process file read/write operations to support the **Automatic exam import via CSV file** feature. |

## 📄 License
Academic / educational project — see repository owner for usage terms.
