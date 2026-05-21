using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    public interface IAdditionAlgrothim
    {
        public Task<string> CreateUniqueLink();
        
    }
}
