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
            var classroom = await _context.Classrooms
            .Where(p => p.ClassName == NameClass)
            .Select(p => new ShowClassRoom
            {
                ClassName = p.ClassName,
                Link = p.InviteCode,
                // Giả sử bạn đã có quan hệ Navigation Property (User) trong model Classroom
                OwnerName = p.OwnerUser.UserName
            })
            .FirstOrDefaultAsync();

            return classroom;
        }
    }
}
