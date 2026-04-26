using QuizStudyAS.Data;
using QuizStudyAS.DTO;
using Microsoft.EntityFrameworkCore;
namespace QuizStudyAS.Service
{
    public class FindClassRoomService : IFindClassRoomService
    {
        private readonly AppDbContext _context;
        public FindClassRoomService(AppDbContext context)
        {
            _context = context;
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
    }
}
