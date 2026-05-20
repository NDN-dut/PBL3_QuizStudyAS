using Microsoft.AspNetCore.Mvc;
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
        public async Task<ListShowClassRoomVM> FindClassRoomByName(string NameClass)
        {
            AdditionAlgrothim AddAl = new AdditionAlgrothim(_context);
            string currentUserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");

            var allClassBasicInfo = await _context.Classrooms
                                .Select(c => new { c.ClassroomId, c.ClassName })
                                .ToListAsync();

            // BƯỚC 2: Chạy Levenshtein trên RAM để lấy ra 5 ID có độ lệch thấp nhất
            var nameToSearch = NameClass.ToLower();

            var top5Ids = allClassBasicInfo
                .OrderBy(c => AddAl.DistanceLevenshtein(c.ClassName.ToLower(), nameToSearch))
                .Take(5)
                .Select(c => c.ClassroomId)
                .ToList();

            // Nếu không tìm thấy gì (DB rỗng)
            if (!top5Ids.Any()) return new ListShowClassRoomVM();

            // BƯỚC 3: Lấy chi tiết 5 lớp học dựa trên 5 cái ID vừa tìm được (Giữ nguyên cấu trúc Select của bạn)
            var top5Classrooms = await _context.Classrooms
                .Where(p => top5Ids.Contains(p.ClassroomId)) 
                .Select(p => new ShowClassRoom
                {
                    ClassName = p.ClassName,
                    Link = p.InviteCode,
                    OwnerName = p.OwnerUser.UserName,
                    // Vẫn lấy Status bình thường dựa vào currentUserId
                    Status_Class = p.JoinRequests
                                    .Where(r => r.UserId == currentUserId)
                                    .Select(r => r.Status)
                                    .FirstOrDefault()
                })
                .ToListAsync(); // Sửa FirstOrDefaultAsync thành ToListAsync vì giờ là lấy 1 list
            
            // BƯỚC 4: (Quan trọng) Sắp xếp lại thứ tự list cuối cùng 
            // Vì toán tử .Contains(ID) của SQL không đảm bảo trả về đúng thứ tự độ lệch Levenshtein
            var finalResult = top5Classrooms
                .OrderBy(p => AddAl.DistanceLevenshtein(p.ClassName.ToLower(), nameToSearch))
                .ToList();
            var data = new ListShowClassRoomVM()
            {
                ListClassRoom = top5Classrooms
            };
            return data;
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
                    StudySets = e.Materials.
                                Where(s => s.Status == "AVAILABLE").
                                Select(k => new StudySetItemVM
                                {
                                    StudySetId = k.StudySetId,
                                    Title = k.StudySet.Title
                                }).ToList()
                }).FirstOrDefaultAsync();
            return ClassRoomDetailData;
        }
        public async Task<bool> AddStudySet(string ClassCode, int StudySetId)
        {
            
            int Classid = await _context.Classrooms.Where(c=>c.InviteCode == ClassCode)
                                                   .Select(c=>c.ClassroomId).FirstOrDefaultAsync();
            if (await CheckAuthorityClass(Classid) == false)
            {
                return false;
            }
            var record = await _context.ClassRoomMaterials.FirstOrDefaultAsync(e => e.ClassRoomId == Classid && e.StudySetId == StudySetId);
            
            if (record!=null)
            {
                record.Status = "AVAILABLE";
            }
            else
            {
                await _context.ClassRoomMaterials.AddAsync(new ClassRoomMaterial
                {
                    StudySetId = StudySetId,
                    ClassRoomId = Classid,
                    Status = "AVAILABLE"
                    
                });
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CheckAuthorityClass(int ClassId)
        {
            string CurrentUserId = _httpContextAccessor.HttpContext.Session.GetString("UserId");
            string OwnerClassId = await _context.Classrooms.Where(c=>c.ClassroomId== ClassId)
                                  .Select(c=>c.OwnerUserId).FirstOrDefaultAsync();
            return CurrentUserId == OwnerClassId;
        }
    }
}
