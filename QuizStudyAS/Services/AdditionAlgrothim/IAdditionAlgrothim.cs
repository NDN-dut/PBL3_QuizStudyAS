using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services.AdditionAlgrothim
{
    public interface IAdditionAlgrothim
    {
        public Task<string> CreateUniqueLink();
        
    }
}
