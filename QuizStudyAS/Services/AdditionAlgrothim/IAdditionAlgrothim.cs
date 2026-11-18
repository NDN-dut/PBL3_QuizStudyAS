namespace QuizStudyAS.Services.AdditionAlgorithm
{
    public interface IAdditionAlgorithm
    {
        int DistanceLevenshtein(string s1, string s2);
        int ApproximateSubstringDistance(string text, string pattern);
    }
}
