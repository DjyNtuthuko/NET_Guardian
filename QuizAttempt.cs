using System;

namespace NET_Guardian
{
    public class QuizAttempt
    {
        public int QuizAttemptId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }
}