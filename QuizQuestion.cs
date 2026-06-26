using System.Collections.Generic;

namespace NET_Guardian
{
    public class QuizQuestion
    {
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}