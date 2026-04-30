using System.ComponentModel.DataAnnotations;

namespace QuizStudyAS.ViewModels
{
    public class EditStudySetVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(100)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public List<FlashcardVM> Flashcards { get; set; } = new List<FlashcardVM>();
    }
}
