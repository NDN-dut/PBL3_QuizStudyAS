using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    public class ClassroomRequestService : IClassroomRequestService
    {
        private readonly AppDbContext _context;
        public ClassroomRequestService(AppDbContext context)
        {
            _context = context;
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
    }
}
