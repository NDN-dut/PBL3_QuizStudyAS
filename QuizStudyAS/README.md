# QuizStudyAS

Interactive study and flashcard platform for learning, classroom management, and online exams. Built as a **PBL3 (Project-Based Learning)** capstone project.

## Overview

**QuizStudyAS** helps students and teachers:

- Create and study **flashcard sets**
- Organize **classrooms** and share materials
- Take **timed multiple-choice exams** (including CSV import)
- Track **learning progress**, **XP**, **streaks**, and **leaderboards**
- Chat with an **AI study assistant** powered by Google Gemini

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core 10 (.NET 10) — MVC |
| Frontend | Razor Views, Bootstrap 5, Bootstrap Icons, custom CSS/JS |
| Database | SQL Server + Entity Framework Core 10 (Code-First) |
| Authentication | Cookie auth + Google OAuth |
| Security | BCrypt password hashing, role-based authorization |
| Email | SMTP (password reset) |
| AI | Google Gemini API |

## Architecture

The project uses **MVC + Service Layer** with clear separation of concerns:

```
Controllers  →  Services  →  AppDbContext  →  SQL Server
     ↓
  Views / ViewModels / DTOs
```

### Key patterns

- **Dependency Injection** — services registered in `Program.cs`
- **Service pattern** — business logic in `Services/`, controllers stay thin
- **EF Core configurations** — entity mappings in `Data/Configurations/`
- **Session + cookies** — user session for app state; cookies for authentication

### Project structure

```
QuizStudyAS/
├── Controllers/       # HTTP endpoints (Auth, StudySet, ClassRoom, Exam, Admin, …)
├── Services/          # Business logic
├── Models/            # Domain entities
├── ViewModels/        # View-specific data shapes
├── DTOs/              # API request/response objects
├── Data/              # DbContext, DbInitializer, EF configurations
├── Views/             # Razor UI
├── wwwroot/           # Static assets (css, js, lib)
├── Migrations/        # EF Core database migrations
├── DataSamples/       # Sample CSV files for exam import
└── Program.cs         # App entry point & DI setup
```

## Features

| Module | Description |
|--------|-------------|
| **Authentication** | Register, login, Google OAuth, forgot password via email |
| **Study Sets** | Create/edit flashcard sets, learn mode with flip cards |
| **Classrooms** | Create classes, invite codes, join requests, shared materials |
| **Exams** | Timed MCQ exams, CSV import, attempt tracking, review & results |
| **Gamification** | XP, levels, daily streaks, achievements, leaderboard |
| **AI Chatbot** | Gemini-powered study assistant for logged-in users |
| **Admin** | User, study set, and classroom management dashboard |

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- SQL Server (e.g. SQL Server Express)
- Visual Studio 2022 / VS Code (optional)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/pbl3_quizstudyas.git
cd pbl3_quizstudyas/QuizStudyAS
```

### 2. Configure application settings

Create `appsettings.json` (and optionally `appsettings.Development.json`) in the project root. These files are gitignored — do not commit secrets.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
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

Adjust `DefaultConnection` to match your SQL Server instance.

### 3. Apply database migrations

From the `QuizStudyAS` folder:

```bash
dotnet ef database update
```

Or use **Package Manager Console** in Visual Studio:

```powershell
Update-Database
```

### 4. Run the application

```bash
dotnet run
```

Or open `QuizStudyAS.slnx` in Visual Studio and press **F5**.

Default URLs (Development):

- HTTP: `http://localhost:5150`
- HTTPS: `https://localhost:7235`

In **Development**, the app automatically runs migrations and seeds sample data via `DbInitializer`.

## Default Seed Accounts

| Username | Role | Password |
|----------|------|----------|
| `admin_teacher` | Admin | `123456` |
| `sv_it_01` | User | `123456` |
| `sv_it_02` | User | `123456` |
| `sv_nn_03` | User | `123456` |
| `sv_it_04` | User | `123456` |

Sample classrooms are also seeded (e.g. invite code `GRASP2026`).

## Exam CSV Import Format

Exams can be created by uploading a `.csv` file. See `DataSamples/` for examples.

**Header row (required):**

```
Cau_hoi,A,B,C,D,Dap_an_dung,Giai_thich
```

**Rules:**

1. First row must be the header above (skipped during import).
2. Wrap fields containing commas in double quotes (`"`).
3. Save as **UTF-8** so Vietnamese characters display correctly.
4. `Dap_an_dung` must be a single letter: `A`, `B`, `C`, or `D`.

## Main Controllers

| Controller | Purpose |
|------------|---------|
| `AuthController` | Login, register, password reset |
| `StudySetController` | Flashcard sets & learn mode |
| `ClassRoomController` | Classrooms & join requests |
| `ExamController` | Create, take, and review exams |
| `ChatbotController` | AI study assistant |
| `LeaderboardController` | Rankings & gamification |
| `AdminController` | Admin dashboard (Admin role only) |
| `UserController` | User profile & settings |

## Team

PBL3 project — Da Nang University of Science and Technology (DUT):

| Member | Role |
|--------|------|
| Hồ Hoàng Phong Hào | Flashcard logic, UI & frontend |
| Nguyễn Danh Ngôn | Database design, auth, system organization |
| Nguyễn Hữu Minh Khoa | Classroom features |

## License

Academic / educational project — see repository owner for usage terms.
