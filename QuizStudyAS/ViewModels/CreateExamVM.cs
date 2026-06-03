using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuizStudyAS.ViewModels
{
    public class CreateExamVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài kiểm tra")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian mở")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn thời gian đóng")]
        public DateTime EndTime { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Vui lòng nhập thời lượng làm bài")]
        [Range(1, 300, ErrorMessage = "Thời lượng từ 1 đến 300 phút")]
        public int DurationMinutes { get; set; }

        public int ClassroomId { get; set; }

        [Required(ErrorMessage = "Vui lòng tải lên file ngân hàng câu hỏi (.csv)")]
        public IFormFile CsvFile { get; set; }
    }
}