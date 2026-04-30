using System.ComponentModel.DataAnnotations;

namespace QuizStudyAS.ViewModels
{
    public class FlashcardVM
    {
        [Required(ErrorMessage = "Vui lòng nhập thuật ngữ")]
        public string Term { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập định nghĩa")]
        public string Definition { get; set; }
    }

    public class CreateStudySetVM
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(100)]
        public string Title { get; set; }

        public string? Description { get; set; }

        // Danh sách này sẽ chứa các thẻ được thêm linh hoạt từ giao diện
        public List<FlashcardVM> Flashcards { get; set; } = new List<FlashcardVM>();
    }
}
