using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizStudyAS.Models
{
    public class StudySetStatus
    {
        [Key]
        public int StatusId { get; set; }
        public string Name { get; set; } // Sẽ lưu: "Active", "DeletedByUser", "LockedByAdmin"

        // --- Navigation Properties ---
        public virtual ICollection<StudySet> StudySets { get; set; }

        public StudySetStatus()
        {
            StudySets = new HashSet<StudySet>();
        }
    }
}