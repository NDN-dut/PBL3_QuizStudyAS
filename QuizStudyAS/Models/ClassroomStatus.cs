using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizStudyAS.Models
{
    public class ClassroomStatus
    {
        [Key]
        public int StatusId { get; set; }
        public string Name { get; set; }

        // --- Navigation Properties ---
        public virtual ICollection<Classroom> Classrooms { get; set; }

        public ClassroomStatus()
        {
            Classrooms = new HashSet<Classroom>();
        }
    }
}