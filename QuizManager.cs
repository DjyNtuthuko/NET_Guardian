using System;
using System.Collections.Generic;

namespace NET_Guardian
{
    public class QuizManager
    {
        public List<QuizQuestion> Questions { get; private set; }
        public int CurrentQuestionIndex { get; private set; }
        public int Score { get; private set; }

        public QuizManager()
        {
            Questions = BuildQuestions();
        }

        public QuizQuestion CurrentQuestion => Questions[CurrentQuestionIndex];
        public bool IsLastQuestion => CurrentQuestionIndex == Questions.Count - 1;

        public void Restart()
        {
            CurrentQuestionIndex = 0;
            Score = 0;
        }

        public (bool IsCorrect, string Feedback) SubmitAnswer(string selectedAnswer)
        {
            bool isCorrect = selectedAnswer == CurrentQuestion.CorrectAnswer;
            if (isCorrect)
                Score++;

            string result = isCorrect
                ? "Correct. " + CurrentQuestion.Explanation
                : $"Incorrect. The correct answer is {CurrentQuestion.CorrectAnswer}. {CurrentQuestion.Explanation}";

            return (isCorrect, result);
        }

        public bool MoveNext()
        {
            if (IsLastQuestion)
                return false;

            CurrentQuestionIndex++;
            return true;
        }

        public string GetResultMessage()
        {
            if (Score >= 8)
                return "Excellent! You are cyber aware.";
            if (Score >= 5)
                return "Good effort. Keep improving your cyber habits.";
            return "Keep learning. Cybersecurity takes practice.";
        }

        public QuizAttempt SaveAttempt()
        {
            QuizAttempt attempt = new QuizAttempt
            {
                Score = Score,
                TotalQuestions = Questions.Count,
                ResultMessage = GetResultMessage(),
                CompletedAt = DateTime.Now
            };

            using NetGuardianDbContext database = new NetGuardianDbContext();
            database.QuizAttempts.Add(attempt);
            database.SaveChanges();
            return attempt;
        }

        private List<QuizQuestion> BuildQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Category = "Password Safety",
                    QuestionText = "Which password is the strongest choice?",
                    Options = new List<string> { "Student2004", "P@ssword1", "River-Copper-71-Lantern", "1234567890" },
                    CorrectAnswer = "River-Copper-71-Lantern",
                    Explanation = "A long, unique passphrase is harder to guess or crack."
                },
                new QuizQuestion
                {
                    Category = "Phishing",
                    QuestionText = "An urgent email asks you to confirm your bank password using a link. What should you do?",
                    Options = new List<string> { "Use the link immediately", "Reply with the password", "Open the bank app or type its official address yourself", "Forward it to friends" },
                    CorrectAnswer = "Open the bank app or type its official address yourself",
                    Explanation = "Using an official channel avoids a link that may lead to a fake website."
                },
                new QuizQuestion
                {
                    Category = "Malware",
                    QuestionText = "True or False: An unexpected attachment can contain malware even when it appears to come from someone you know.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "True",
                    Explanation = "A contact's account may be compromised, so unexpected files should be verified first."
                },
                new QuizQuestion
                {
                    Category = "Privacy",
                    QuestionText = "Which detail is safest to avoid posting publicly?",
                    Options = new List<string> { "Your favourite colour", "Your home address and daily routine", "A movie review", "A recipe" },
                    CorrectAnswer = "Your home address and daily routine",
                    Explanation = "Location and routine details can expose you to stalking, theft, or impersonation."
                },
                new QuizQuestion
                {
                    Category = "Safe Browsing",
                    QuestionText = "True or False: HTTPS means a website is guaranteed to be honest and safe.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "False",
                    Explanation = "HTTPS encrypts the connection, but scammers can also use HTTPS on fake websites."
                },
                new QuizQuestion
                {
                    Category = "Two-Factor Authentication",
                    QuestionText = "What is the main benefit of two-factor authentication?",
                    Options = new List<string> { "It shortens your password", "It adds another verification step", "It removes the need for updates", "It hides your username" },
                    CorrectAnswer = "It adds another verification step",
                    Explanation = "A second factor can block an attacker who has stolen only your password."
                },
                new QuizQuestion
                {
                    Category = "Social Engineering",
                    QuestionText = "A caller claims to be IT support and asks for your login code. What is the best response?",
                    Options = new List<string> { "Share it because IT asked", "Verify the request through an official contact", "Post the code in a group chat", "Disable your screen lock" },
                    CorrectAnswer = "Verify the request through an official contact",
                    Explanation = "Independent verification helps expose impersonation and pretexting attempts."
                },
                new QuizQuestion
                {
                    Category = "Online Scams",
                    QuestionText = "True or False: A prize that requires an upfront gift-card payment is a common scam warning sign.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "True",
                    Explanation = "Legitimate prizes do not demand hard-to-reverse gift-card payments first."
                },
                new QuizQuestion
                {
                    Category = "Password Safety",
                    QuestionText = "What is the safest way to manage different passwords for many accounts?",
                    Options = new List<string> { "Reuse one password", "Write them in public notes", "Use a reputable password manager", "Email them to yourself" },
                    CorrectAnswer = "Use a reputable password manager",
                    Explanation = "A password manager helps you use a unique strong password for every account."
                },
                new QuizQuestion
                {
                    Category = "Safe Browsing",
                    QuestionText = "Before installing a browser extension, what should you check?",
                    Options = new List<string> { "Only its icon colour", "Its source, reviews, and requested permissions", "Whether it has pop-up ads", "Whether a stranger sent it" },
                    CorrectAnswer = "Its source, reviews, and requested permissions",
                    Explanation = "Untrusted extensions with excessive permissions may read or change sensitive browser data."
                }
            };
        }
    }
}