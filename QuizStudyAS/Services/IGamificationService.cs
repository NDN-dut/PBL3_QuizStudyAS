namespace QuizStudyAS.Services
{
    public interface IGamificationService
    {
        // Trả về một object chứa thông tin để báo cho Frontend biết (có lên cấp không, chuỗi ngày hiện tại là bao nhiêu)
        Task<GamificationResultVM> UpdateUserProgressAsync(string userId, int earnedXP);
    }

    public class GamificationResultVM
    {
        public bool Success { get; set; }
        public int EarnedXP { get; set; }
        public int CurrentXP { get; set; }
        public int Level { get; set; }
        public bool IsLeveledUp { get; set; }
        public int CurrentStreak { get; set; }
        public bool IsStreakSaved { get; set; } // Kiểm tra xem hôm nay đã cứu chuỗi chưa
    }
}
