using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services
{
    public class ClassRoomServices : IClassRoomServices
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ClassRoomServices(AppDbContext context,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ShowClassRoom> FindClassRoomByName(string NameClass)
        {
            var CurrentUser = await _context.Users.FirstOrDefaultAsync();
            string currentUserId = CurrentUser?.Id;

            var classroom = await _context.Classrooms
                .Where(p => p.ClassName == NameClass)
                .Select(p => new ShowClassRoom
                {
                    ClassName = p.ClassName,
                    Link = p.InviteCode,
                    OwnerName = p.OwnerUser.UserName,
                    // 3. Truyền biến string vào đây để so sánh string == string
                    Status_Class = p.JoinRequests
                                    .Where(r => r.UserId == currentUserId)
                                    .Select(r => r.Status)
                                    .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            return classroom;
        }

        public async Task CreateRequest(string ClassName)
        {
            var User = _context.Users.FirstOrDefault();
            var RequestJoin = await _context.RequestJoinClasses.AddAsync(new RequestJoinClass
            {
                UserId = User.Id,
                ClassroomId = await _context.Classrooms.Where(e => e.ClassName == ClassName)
                                                        .Select(e => e.ClassroomId).FirstOrDefaultAsync(),
                Status = "PENDING"
            });
            await _context.SaveChangesAsync();

        }
        public async Task<YourClassVM> GetYourClass()
        {
            var UserID = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            var MyOwerClass = await _context.Classrooms
                .Where(p=> p.OwnerUserId == UserID)
                .Select(c=>new ShowClassRoom{
                    ClassName = c.ClassName,
                    Link = c.InviteCode,
                    OwnerName = c.OwnerUser.UserName,
                    Status_Class = "OWNER"
                }).ToListAsync();

            var MyJoinedClass = await _context.RequestJoinClasses
                .Where(p => p.UserId == UserID && p.Status == "APPROVED")
                .Select(c => new ShowClassRoom
                {
                    ClassName = c.Classroom.ClassName,
                    Link = c.Classroom.InviteCode,
                    OwnerName = c.Classroom.OwnerUser.UserName,
                    Status_Class = "JOINED"
                }).ToListAsync();

            return new YourClassVM
            {
                MyClasses = MyOwerClass,
                JoinedClasses = MyJoinedClass
            };
            
        }
    }
}
