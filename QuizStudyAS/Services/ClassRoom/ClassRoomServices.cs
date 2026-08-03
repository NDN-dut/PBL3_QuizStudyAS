using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Constants;
using QuizStudyAS.Data;
using QuizStudyAS.Models;
using QuizStudyAS.ViewModels;

namespace QuizStudyAS.Services.ClassRoom
{
    using AdditionAlgrothim = QuizStudyAS.Services.AdditionAlgrothim.AdditionAlgrothim;
    public class ClassRoomServices : IClassRoomServices
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ClassRoomServices(AppDbContext context,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            
        }
        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("UserId");
        }
        public async Task<ListShowClassRoomVM> FindClassRoomByName(string NameClass)
        {
            // THÊM ĐOẠN KIỂM TRA CHẶN NULL HOẶC RỖNG Ở ĐÂY
            if (string.IsNullOrWhiteSpace(NameClass))
            {
                return new ListShowClassRoomVM { ListClassRoom = new List<ShowClassRoom>() };
            }

            AdditionAlgrothim AddAl = new AdditionAlgrothim(_context);
            string currentUserId = GetCurrentUserId();

            // THAY THẾ DÒNG CŨ: var allClassBasicInfo = await _context.Classrooms.Where(c=>c.IsActive==true)
            var allClassBasicInfo = await _context.Classrooms.Where(c=>c.StatusId == (int)ClassroomStatusEnum.Active)
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
                    ExistingAvatarUrl = p.OwnerUser.AvatarUrl,
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
                ListClassRoom = finalResult
            };
            return data;
        }
        

        public async Task CreateRequest(string ClassName)
        {
            var currentUserId = GetCurrentUserId();
            var User = _context.Users.FirstOrDefault(e=>e.Id == currentUserId);
            var record = await _context.RequestJoinClasses.FirstOrDefaultAsync(c=>c.Classroom.ClassName == ClassName && c.UserId == User.Id);
            if(record != null)
            {
                record.Status = RequestJoinStatus.Pending;
                await _context.SaveChangesAsync();
                return;
            }
            var RequestJoin = await _context.RequestJoinClasses.AddAsync(new RequestJoinClass
            {
                UserId = User.Id,
                ClassroomId = await _context.Classrooms.Where(e => e.ClassName == ClassName)
                                                        .Select(e => e.ClassroomId).FirstOrDefaultAsync(),
                Status = RequestJoinStatus.Pending
            });
            await _context.SaveChangesAsync();

        }
        public async Task<YourClassVM> GetYourClass()
        {
            var UserID = GetCurrentUserId();
            // SỬA: Đổi điều kiện thành != DeletedByUser (2)
            var MyOwerClass = await _context.Classrooms
                .Where(p => p.OwnerUserId == UserID && p.StatusId != (int)ClassroomStatusEnum.DeletedByUser)
                .Select(c=>new ShowClassRoom{
                    ClassName = c.ClassName,
                    Link = c.InviteCode,
                    OwnerName = c.OwnerUser.UserName,
                    ExistingAvatarUrl = c.OwnerUser.AvatarUrl,
                    Status_Class = ClassroomRoleStatus.Owner
                }).ToListAsync();

            // Sửa câu truy vấn MyJoinedClass:
            var MyJoinedClass = await _context.ClassroomUsers
                .Where(p => p.UserId == UserID && p.Status == ClassroomUserStatus.Studying && p.Classroom.StatusId == (int)ClassroomStatusEnum.Active)
                .Select(c => new ShowClassRoom
                {
                    ClassName = c.Classroom.ClassName,
                    Link = c.Classroom.InviteCode,
                    OwnerName = c.Classroom.OwnerUser.UserName,
                    ExistingAvatarUrl = c.Classroom.OwnerUser.AvatarUrl,
                    Status_Class = ClassroomRoleStatus.Joined
                }).ToListAsync();

            return new YourClassVM
            {
                MyClasses = MyOwerClass,
                JoinedClasses = MyJoinedClass
            };
            
        }
        public async Task<ListRequestJoinVM> GetJoinVMs()
        {
            var currentUserId = GetCurrentUserId();
            var ListRequestJoin = await _context.RequestJoinClasses
                .Where(e => e.Classroom.OwnerUserId == currentUserId && e.Status == RequestJoinStatus.Pending && e.Classroom.StatusId == (int)ClassroomStatusEnum.Active)
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
                request.Status = RequestJoinStatus.Denied;
                await _context.SaveChangesAsync();
            }
        }
        public async Task AcceptRequest(string userid, int classroomid)
        {

            var request = await _context.RequestJoinClasses.FirstOrDefaultAsync(e => e.ClassroomId == classroomid && e.UserId == userid);
            if (request != null)
            {
                request.Status = RequestJoinStatus.Approved;
                var record = await _context.ClassroomUsers.FirstOrDefaultAsync(c=>c.ClassroomId==classroomid && c.UserId == userid);
                if (record != null)
                {
                    record.Status = ClassroomUserStatus.Studying;
                    await _context.SaveChangesAsync();
                    return;
                }
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
                OwnerUserId = GetCurrentUserId()

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
            // 1. Lấy UserId đang đăng nhập để xác định xem họ có phải Chủ phòng không
            string currentUserId = GetCurrentUserId();

            var ClassRoomDetailData = await _context.Classrooms
                .Where(c => c.InviteCode == LinkLop)
                .Select(e => new ClassRoomDetailVM
                {
                    ClassroomId = e.ClassroomId, // BỔ SUNG để làm Link tạo đề thi
                    ClassName = e.ClassName,
                    OwnerName = e.OwnerUser.UserName,
                    ExistingAvatarUrl = e.OwnerUser.AvatarUrl,
                    ClassCode = LinkLop,
                    IsActive = e.StatusId == (int)ClassroomStatusEnum.Active,

                    // BỔ SUNG: Kiểm tra cờ Chủ phòng
                    IsOwner = e.OwnerUserId == currentUserId,

                    StudySets = e.Materials.
                                Where(s => s.Status == ClassroomMaterialStatus.Available).
                                Select(k => new StudySetItemVM
                                {
                                    StudySetId = k.StudySetId,
                                    Title = k.StudySet.Title
                                }).ToList()
                }).FirstOrDefaultAsync();

            // 2. Lấy bổ sung danh sách Bài kiểm tra nếu tìm thấy lớp học
            if (ClassRoomDetailData != null)
            {
                var exams = await _context.Exams
                    .Where(ex => ex.ClassroomId == ClassRoomDetailData.ClassroomId)
                    .Select(ex => new ClassroomExamItemVM
                    {
                        ExamId = ex.ExamId,
                        Title = ex.Title,
                        StartTime = ex.StartTime,
                        EndTime = ex.EndTime,
                        DurationMinutes = ex.DurationMinutes
                    })
                    .OrderByDescending(ex => ex.ExamId) // Bài thi mới tạo xếp lên đầu
                    .ToListAsync();

                ClassRoomDetailData.Exams = exams;
            }

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
                record.Status = ClassroomMaterialStatus.Available;
            }
            else
            {
                await _context.ClassRoomMaterials.AddAsync(new ClassRoomMaterial
                {
                    StudySetId = StudySetId,
                    ClassRoomId = Classid,
                    Status = ClassroomMaterialStatus.Available
                    
                });
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteStudySet(string ClassCode, int StudySetId)
        {
            int Classid = await _context.Classrooms.Where(c => c.InviteCode == ClassCode)
                                                   .Select(c => c.ClassroomId).FirstOrDefaultAsync();
            if (await CheckAuthorityClass(Classid) == false)
            {
                return false;
            }
            var record = await _context.ClassRoomMaterials.FirstOrDefaultAsync(e => e.ClassRoomId == Classid && e.StudySetId == StudySetId);

            if (record != null)
            {
                record.Status = ClassroomMaterialStatus.Deleted;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteClassRoom(string Classcode)
        {
            int Classid = await _context.Classrooms.Where(c => c.InviteCode == Classcode)
                                                   .Select(c => c.ClassroomId).FirstOrDefaultAsync();
            if (await CheckAuthorityClass(Classid) == false)
            {
                return false;
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c=>c.InviteCode == Classcode);
            if (classroom == null)
            {
                return false;
            }
            // THAY THẾ DÒNG CŨ: classroom.IsActive = false;
            // Thực hiện Xóa mềm bằng Enum
            classroom.StatusId = (int)ClassroomStatusEnum.DeletedByUser;
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> LeaveClassRoom(string classCode)
        {
            string? currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return false;
            }
            // 1. Find the classroom by invite code
            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.InviteCode == classCode && c.StatusId == (int)ClassroomStatusEnum.Active);
            if (classroom == null)
            {
                return false;
            }
            // 2. Prevent Owner from leaving their own classroom (Owner must delete or transfer ownership)
            if (classroom.OwnerUserId == currentUserId)
            {
                return false;
            }
            // 3. Find user membership record
            var memberRecord = await _context.ClassroomUsers
                .FirstOrDefaultAsync(cu => cu.ClassroomId == classroom.ClassroomId && cu.UserId == currentUserId);
            if (memberRecord != null)
            {
                memberRecord.Status = ClassroomUserStatus.Left;
            }
            // 4. Also clean up any pending/approved join requests so the user can re-apply in the future if desired
            var joinRequest = await _context.RequestJoinClasses
                .FirstOrDefaultAsync(r => r.ClassroomId == classroom.ClassroomId && r.UserId == currentUserId);
            if (joinRequest != null)
            {
                joinRequest.Status = RequestJoinStatus.Denied;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CheckAuthorityClass(int ClassId)
        {
            string CurrentUserId = GetCurrentUserId();
            if(string.IsNullOrEmpty(CurrentUserId))
            {
                return false;
            }
            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c=>c.ClassroomId== ClassId);
            if(classroom == null)
            {
                return false;
            }
            return CurrentUserId == classroom.OwnerUserId;
        }
        public async Task<bool> DeleteUserOfClass(string ClassCode, string UserId)
        {
            var ClassId = await _context.Classrooms.Where(cl=> cl.InviteCode == ClassCode)
                                                    .Select(cl=>cl.ClassroomId)
                                                    .FirstOrDefaultAsync();
            if ((await CheckAuthorityClass(ClassId)) == false)
            {
                return false;
            }

            var record = await _context.ClassroomUsers.FirstOrDefaultAsync(c => c.UserId == UserId && c.ClassroomId == ClassId);
            var request = await _context.RequestJoinClasses.FirstOrDefaultAsync(c => c.UserId == UserId && c.ClassroomId == ClassId);

            if (record != null)
            {
                record.Status = ClassroomUserStatus.Kicked;
                
            }
            if(request != null){
                request.Status = RequestJoinStatus.Denied;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<ClassRoomDetailVM> GetAllUserOfClass(string ClassCode)
        {
            var userList = await _context.ClassroomUsers
                                                        .Where(cu => cu.Classroom.InviteCode == ClassCode && cu.Status == ClassroomUserStatus.Studying) // Móc thẳng vào thẻ Navigation
                                                        .Select(cu => new UserInfo
                                                        {
                                                            UserId = cu.UserId,
                                                            UserName = cu.User.UserName
                                                        })
                                                        .ToListAsync();
            var Data = new ClassRoomDetailVM
            {
                ClassUsers = userList
            };
            return Data;

        }
        public async Task<string?> RegenerateInviteCode(string oldClassCode)
        {
            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.InviteCode == oldClassCode);

            if (classroom == null)
            {
                return null;
            }

            // Security check: Only the classroom owner can regenerate the code
            if (!await CheckAuthorityClass(classroom.ClassroomId))
            {
                return null;
            }

            // Generate a new unique 8-character invite code
            string newCode = await CreateUniqueLink();
            classroom.InviteCode = newCode;

            await _context.SaveChangesAsync();
            return newCode;
        }
    }
}
