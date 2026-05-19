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
            
            string currentUserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");

            var classroom = await _context.Classrooms
                .Where(p => p.ClassName == NameClass)
                .Select(p => new ShowClassRoom
                {
                    ClassName = p.ClassName,
                    Link = p.InviteCode,
                    OwnerName = p.OwnerUser.UserName,
                    // 3. Truyền biến string vào đây để so sánh string == string
                    Status_Class =  p.JoinRequests
                                    .Where(r => r.UserId == currentUserId)
                                    .Select(r => r.Status)
                                    .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            return classroom;
        }

        public async Task CreateRequest(string ClassName)
        {
            var User = _context.Users.FirstOrDefault(e=>e.Id == _httpContextAccessor.HttpContext.Session.GetString("UserId"));
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
        public async Task<ListRequestJoinVM> GetJoinVMs()
        {
            var ListRequestJoin = await _context.RequestJoinClasses
                .Where(e => e.Classroom.OwnerUserId == _httpContextAccessor.HttpContext.Session.GetString("UserId") && e.Status == "PENDING")
                .Select(e => new RequestJoinVM
                {
                    UserId = e.UserId,
                    UserName = e.User.UserName,
                    ClassName = e.Classroom.ClassName,
                    ClassRoomId = e.Classroom.ClassroomId
                }).ToListAsync();
            return new ListRequestJoinVM
            {
                RequestJoinVMs = ListRequestJoin
            };
        }
        public async Task DeninedRequest(string userid, int classroomid)
        {
            var request = await _context.RequestJoinClasses.FirstOrDefaultAsync(e=>e.ClassroomId == classroomid && e.UserId ==  userid);
            if(request != null)
            {
                request.Status = "DENIED";
                await _context.SaveChangesAsync();
            }
        }
        public async Task AcceptRequest(string userid, int classroomid)
        {
            var request = await _context.RequestJoinClasses.FirstOrDefaultAsync(e => e.ClassroomId == classroomid && e.UserId == userid);
            if (request != null)
            {
                request.Status = "APPROVED";
                await _context.AddAsync(new ClassroomUser
                {
                    ClassroomId = classroomid,
                    UserId = userid,
                    JoinedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
        }
        public async Task CreateClassRoom(string ClassName)
        {
            var newLink = CreateUniqueLink();
            await _context.Classrooms.AddAsync(new Classroom
            {
                InviteCode = await newLink,
                ClassName = ClassName,
                OwnerUserId = _httpContextAccessor.HttpContext.Session.GetString("UserId")

            });
            await _context.SaveChangesAsync();
            
        }
        public async Task<string> CreateUniqueLink()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string newLink = "";
            bool isUnique = false;

            while (!isUnique)
            {
                var codeArray = new char[8];
                for (int i = 0; i < 8; i++)
                {
                    // Random.Shared.Next() lấy ngẫu nhiên 1 vị trí trong chuỗi chars
                    codeArray[i] = chars[Random.Shared.Next(chars.Length)];
                }
                newLink = new string(codeArray);

                bool LinkExists = await _context.Classrooms.AnyAsync(c => c.InviteCode == newLink);

                if (!LinkExists)
                {
                    isUnique = true; // Mã duy nhất -> Dừng vòng lặp!
                }
            }

            return newLink;
        }
        public async Task<ClassRoomDetailVM> GetClassRoomDetail(string LinkLop)
        {
            var ClassRoomDetailData = await _context.Classrooms
                .Where(c => c.InviteCode == LinkLop)
                .Select(e => new ClassRoomDetailVM
                {
                    ClassName = e.ClassName,
                    OwnerName = e.OwnerUser.UserName,
                    ClassCode = LinkLop,
                    StudySets = e.StudySets.Select(s => new StudySetItemVM
                    {
                        StudySetId = s.StudySetId,
                        Title = s.Title
                    }).ToList()
                }).FirstOrDefaultAsync();
            return ClassRoomDetailData;
        }
    }
}
