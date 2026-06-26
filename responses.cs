using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable CS8981 // Keeps the original Part 2 class name.
namespace NET_Guardian
{
    public class responses
    {
        // Delegate for generating bot responses
        public delegate string BotResponseDelegate(string userInput);
        // Tracks whether the bot has asked for the user's name
        public bool IsNameAsked { get; set; } = false;

        // Stores the user's name after they provide it
        public string UserName { get; set; } = "";

        // Keeps track of the last topic discussed
        public string LastTopic { get; set; } = "";

        // Stores the user's favourite topic for personalisation
        public string FavouriteTopic { get; set; } = "";

        // Stores the full conversation history
        public List<string> ChatHistory { get; private set; } = new List<string>();

        // Lets the window respond to navigation commands
        public string RequestedAction { get; private set; } = "";

        // Used to pick random responses from a topic list
        private Random rng = new Random();

        // Holds multiple responses per cybersecurity topic
        private Dictionary<string, List<string>> topicBank = new Dictionary<string, List<string>>();

        // Maps keywords and shorthand to their full topic name
        private Dictionary<string, string> keywordMap = new Dictionary<string, string>();

        // File used to persist user name and favourite topic between sessions
        private string memoryFilePath = "users.txt";

        // Delegate instance used to route all user input through
        private BotResponseDelegate botHandler;

        public responses()
        {
            LoadTopics();
            LoadKeywords();
            botHandler = BuildResponse;
        }
        private void LoadTopics()
        {
            topicBank["password safety"] = new List<string>
            {
                "A secure password should be at least 12 characters and include a mix of uppercase, lowercase, numbers, and symbols. Avoid using your name or birthday.",
                "Never use the same password on multiple accounts. If one account gets hacked, all others become vulnerable too. Use a password manager to keep track.",
                "Change your passwords every few months and never share them with anyone, not even IT support staff."
            };

            topicBank["phishing"] = new List<string>
            {
                "Phishing attacks trick you into revealing sensitive information through fake emails or websites. Always verify the sender before clicking any links.",
                "Attackers often create a sense of urgency in phishing emails — phrases like 'Your account will be suspended' are red flags. Take a breath and verify first.",
                "Hover over links in emails before clicking. If the URL looks strange or does not match the company name, do not click it."
            };

            topicBank["scams"] = new List<string>
            {
                "Online scammers often pose as government officials or banks. Remember, no legitimate institution will ask for your password or banking PIN over the phone.",
                "If someone is pressuring you to send money quickly or buy gift cards as payment, it is almost certainly a scam. Stop and report it.",
                "Advance fee scams promise large rewards in exchange for a small upfront payment. Once you pay, the scammer disappears."
            };

            topicBank["privacy"] = new List<string>
            {
                "Review your social media privacy settings regularly. Make sure only trusted people can see your personal posts and contact information.",
                "Avoid posting details like your home address, phone number, or daily routine online. This information can be exploited by bad actors.",
                "When signing up for apps or services, only provide the minimum information required. Oversharing personal data increases your risk."
            };

            topicBank["safe browsing"] = new List<string>
            {
                "Only enter sensitive information on websites that show 'https' and a padlock in the address bar. Plain 'http' sites are not encrypted.",
                "Avoid clicking on pop-up ads or downloading software from unknown websites. These are common ways malware gets onto your device.",
                "Keep your browser and its extensions up to date. Outdated software often has known security holes that attackers actively exploit."
            };

            topicBank["malware"] = new List<string>
            {
                "Malware is software designed to damage or gain unauthorised access to your device. Install a reputable antivirus program and scan your device regularly.",
                "Be cautious of email attachments, even from people you know. Their account may have been compromised and used to spread malware.",
                "Ransomware is a type of malware that locks your files until you pay a ransom. Back up your data regularly so you are never at the mercy of an attacker."
            };

            topicBank["two-factor authentication"] = new List<string>
            {
                "Two-factor authentication (2FA) requires a second form of verification beyond your password. Even if your password is stolen, attackers cannot log in without the second factor.",
                "Enable 2FA on all important accounts such as email, banking, and social media. An authenticator app is safer than SMS-based codes.",
                "Hardware security keys are the strongest form of 2FA. They are physical devices that must be present to complete a login."
            };

            topicBank["social engineering"] = new List<string>
            {
                "Social engineering relies on manipulating people rather than hacking systems. Attackers may pose as colleagues, IT staff, or authority figures to gain your trust.",
                "Be sceptical of unexpected requests for access, passwords, or urgent transfers — even if they seem to come from someone you know. Always verify through a separate channel.",
                "Pretexting is when an attacker creates a fabricated scenario to extract information from you. If something feels off, trust your instincts and report it."
            };
        }

        private void LoadKeywords()
        {
            string[] passwordKeys = { "password", "passwords", "password safety", "passcode", "login", "login details", "credentials" };
            foreach (string key in passwordKeys)
                keywordMap[key] = "password safety";

            string[] phishingKeys = { "phishing", "phising", "fake email", "suspicious email", "phish", "email scam" };
            foreach (string key in phishingKeys)
                keywordMap[key] = "phishing";

            string[] scamKeys = { "scam", "scams", "fraud", "fake offer", "scamming", "scammed", "con" };
            foreach (string key in scamKeys)
                keywordMap[key] = "scams";

            string[] privacyKeys = { "privacy", "private", "personal info", "data protection", "data privacy", "personal data" };
            foreach (string key in privacyKeys)
                keywordMap[key] = "privacy";

            string[] browsingKeys = { "safe browsing", "browser", "website", "https", "link", "suspicious link", "web", "browsing" };
            foreach (string key in browsingKeys)
                keywordMap[key] = "safe browsing";

            string[] malwareKeys = { "malware", "virus", "viruses", "trojan", "ransomware", "harmful software", "malicious software" };
            foreach (string key in malwareKeys)
                keywordMap[key] = "malware";

            string[] twoFactorKeys = { "2fa", "two factor", "two-factor", "authentication", "otp", "2-factor", "verification code" };
            foreach (string key in twoFactorKeys)
                keywordMap[key] = "two-factor authentication";

            string[] socialEngKeys = { "social engineering", "manipulation", "tricked", "impersonation", "tricking", "social engineer", "pretexting" };
            foreach (string key in socialEngKeys)
                keywordMap[key] = "social engineering";
        }

        // Match user input to a known topic using the keyword map
        private string? MatchTopic(string input)
        {
            string lowered = input.ToLower();

            string[] words = lowered.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                if (keywordMap.ContainsKey(word))
                    return keywordMap[word];
            }

            foreach (string key in keywordMap.Keys)
            {
                if (lowered.Contains(key))
                    return keywordMap[key];
            }

            return null;
        }

        // Add a line to the chat history log
        public void AddToChatHistory(string entry)
        {
            ChatHistory.Add(entry);
        }

        // Return full chat history as a readable string
        public string GetChatHistory()
        {
            if (ChatHistory.Count == 0)
                return "Your chat history is currently empty.";

            return string.Join("\n", ChatHistory);
        }

        // Write the user's name and favourite topic to a file
        public void SaveUserMemory()
        {
            try
            {
                string record = $"{UserName}|{FavouriteTopic}";
                File.WriteAllText(memoryFilePath, record);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Could not save user memory: " + ex.Message);
            }
        }

        // Read the user's saved data from file if it exists
        public bool LoadUserMemory(string name)
        {
            try
            {
                if (File.Exists(memoryFilePath))
                {
                    string record = File.ReadAllText(memoryFilePath);
                    string[] parts = record.Split('|');

                    if (parts.Length == 2 && parts[0] == name)
                    {
                        UserName = parts[0];
                        FavouriteTopic = parts[1];
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Could not load user memory: " + ex.Message);
            }

            return false;
        }

        // Calculate how different two strings are (used for typo detection)
        private int GetEditDistance(string wordA, string wordB)
        {
            wordA = wordA.ToLower();
            wordB = wordB.ToLower();

            int rowCount = wordA.Length;
            int colCount = wordB.Length;
            int[,] matrix = new int[rowCount + 1, colCount + 1];

            for (int i = 0; i <= rowCount; i++) matrix[i, 0] = i;
            for (int j = 0; j <= colCount; j++) matrix[0, j] = j;

            for (int i = 1; i <= rowCount; i++)
            {
                for (int j = 1; j <= colCount; j++)
                {
                    int substitutionCost = (wordA[i - 1] == wordB[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + substitutionCost);
                }
            }

            return matrix[rowCount, colCount];
        }

        // Try to match a word to a topic even if it has a typo
        private string? MatchTopicWithTypoTolerance(string word)
        {
            string? closestMatch = null;
            int lowestDistance = 3;

            foreach (string key in keywordMap.Keys)
            {
                int distance = GetEditDistance(word, key);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    closestMatch = keywordMap[key];
                }
            }

            return closestMatch;
        }

        // Identify the emotional tone of the user's message
        private string IdentifySentiment(string input)
        {
            string lowered = input.ToLower();
            string[] words = lowered.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                if (word == "worried" || word == "concern" || word == "concerned" || word == "scared" || word == "afraid" || word == "worry")
                    return "worried";

                if (word == "angry" || word == "upset" || word == "mad" || word == "furious" || word == "irritated")
                    return "angry";

                if (word == "confused" || word == "unsure" || word == "lost" || word == "baffled")
                    return "confused";

                if (word == "curious" || word == "inquisitive" || word == "wonder" || word == "wondering")
                    return "curious";

                if (word == "sad" || word == "unhappy" || word == "depressed" || word == "down")
                    return "sad";

                if (word == "happy" || word == "great" || word == "good" || word == "wonderful" || word == "awesome" || word == "excellent")
                    return "happy";

                if (word == "frustrated" || word == "stressed" || word == "overwhelmed" || word == "annoyed")
                    return "frustrated";
            }

            return "neutral";
        }

        // Wrap a tip with a tone-appropriate opener based on the user's sentiment
        private string ApplySentimentWrapper(string sentiment, string topic, string tip)
        {
            switch (sentiment)
            {
                case "worried":
                    return $"It is understandable to feel concerned about {topic}. Here is something that can help: {tip}";
                case "angry":
                    return $"I hear you — {topic} can be really frustrating to deal with. Here is what you should know: {tip}";
                case "confused":
                    return $"No worries, {topic} can be tricky at first. Let me break it down: {tip}";
                case "curious":
                    return $"Love the curiosity about {topic}! Here is something worth knowing: {tip}";
                case "sad":
                    return $"I am here to help you with {topic}. {tip}";
                case "happy":
                    return $"Great energy! Here is something useful about {topic}: {tip}";
                case "frustrated":
                    return $"Let us work through this together. Here is a tip on {topic}: {tip}";
                default:
                    return tip;
            }
        }

        // Handle requests for a tip, either on a specific topic or a random one
        private string HandleTipRequest(string input)
        {
            string? matchedTopic = MatchTopic(input);

            if (matchedTopic != null)
            {
                LastTopic = matchedTopic;
                return $"Here is a tip on {matchedTopic}: " + PickRandomResponse(matchedTopic);
            }

            List<string> allTopics = topicBank.Keys.ToList();
            string randomTopic = allTopics[rng.Next(allTopics.Count)];
            LastTopic = randomTopic;
            return $"Here is a random tip on {randomTopic}: " + PickRandomResponse(randomTopic);
        }

        // Check if the user's message contains a farewell word anywhere in the sentence
        private bool ContainsFarewell(string input)
        {
            string[] farewellWords =
            {
                "goodbye", "bye", "later", "exit", "quit", "farewell",
                "see you", "take care", "catch you", "ciao", "peace",
                "sharp", "gotta go", "signing off", "gtg", "until next time"
            };

            foreach (string word in farewellWords)
            {
                if (input.Contains(word))
                    return true;
            }

            return false;
        }

        // Core method — processes user input and returns an appropriate response
        public string BuildResponse(string userInput)
        {
            string lowered = userInput.ToLower();
            RequestedAction = "";

            // Detect farewell words anywhere in the message
            if (ContainsFarewell(lowered))
                return $"It was great chatting with you, {UserName}! Stay safe online and remember — good cyber habits go a long way. Come back anytime!";

            // Open the task assistant for task and reminder commands
            if (lowered.Contains("add a task") || lowered.Contains("create task") ||
                lowered.Contains("make task") || lowered.Contains("set reminder") ||
                lowered.Contains("remind me to"))
            {
                RequestedAction = "OpenTasks";
                return "I have opened the Tasks tab. Add a title, choose a priority, and optionally select a reminder date.";
            }

            // Start the cybersecurity quiz
            if (lowered.Contains("start quiz") || lowered.Contains("play quiz") ||
                lowered.Contains("test my knowledge") || lowered.Contains("cybersecurity quiz"))
            {
                RequestedAction = "StartQuiz";
                return "The cybersecurity quiz is ready in the Quiz tab. Choose an answer and select Next.";
            }

            // Show saved activity
            if (lowered.Contains("activity log") || lowered.Contains("show activity") ||
                lowered.Contains("what have you done for me"))
            {
                RequestedAction = "ShowActivity";
                return "I have opened your Activity Log.";
            }

            // Show chat history if requested
            if (lowered == "history" || lowered == "show history" ||
                lowered == "chat history" || lowered == "show chat history")
            {
                RequestedAction = "ShowHistory";
                return GetChatHistory();
            }

            // Handle tip or advice requests
            if (lowered.Contains("give me a tip") || lowered.Contains("give me advice") ||
                lowered.Contains("help me stay safe") || lowered == "tip" ||
                lowered.Contains("another tip") || lowered.Contains("phishing tip") ||
                lowered.Contains("password tip"))
            {
                return HandleTipRequest(userInput);
            }

            // Save favourite topic if user declares one
            if (lowered.Contains("interested in") || lowered.Contains("favourite topic is") || lowered.Contains("favorite topic is"))
            {
                string? matched = MatchTopic(userInput);
                if (matched != null)
                {
                    FavouriteTopic = matched;
                    SaveUserMemory();
                    RequestedAction = "FavouriteTopicSaved";
                    return $"Noted, {UserName}! I will remember that your favourite topic is {matched}. What would you like to know about it?";
                }
            }

            string mood = IdentifySentiment(userInput);

            // Handle follow up requests
            if (lowered.Contains("tell me more") || lowered.Contains("explain more") || lowered.Contains("more"))
            {
                if (!string.IsNullOrEmpty(LastTopic) && topicBank.ContainsKey(LastTopic))
                    return PickRandomResponse(LastTopic);

                return "Could you clarify what you would like to know more about?";
            }

            // match to a topic using keywords
            string? topic = MatchTopic(userInput);

            if (topic != null)
            {
                LastTopic = topic;
                string tip = PickRandomResponse(topic);
                if (mood != "neutral")
                    return ApplySentimentWrapper(mood, topic, tip);

                if (topic == FavouriteTopic)
                    return $"Since {topic} is your favourite topic, {UserName}, here is a tip: {tip}";
                return tip;
            }

            // Try typo-tolerant matching on each word
            string[] inputWords = lowered.Split(' ');
            foreach (string word in inputWords)
            {
                if (word.Length > 3)
                {
                    string? fuzzyMatch = MatchTopicWithTypoTolerance(word);
                    if (fuzzyMatch != null)
                    {
                        LastTopic = fuzzyMatch;
                        string tip = PickRandomResponse(fuzzyMatch);

                        if (mood != "neutral")
                            return ApplySentimentWrapper(mood, fuzzyMatch, tip);

                        return tip;
                    }
                }
            }

            // Fallback if nothing matched
            return $"I am not sure I understood that, {UserName}. Try asking about: password safety, phishing, scams, privacy, safe browsing, malware, two-factor authentication, or social engineering.";
        }

        // Pick a random response from the list for a given topic
        private string PickRandomResponse(string topic)
        {
            List<string> options = topicBank[topic];
            return options[rng.Next(options.Count)];
        }

        // Entry point called from the ui routes input through the delegate
        public string ProcessUserInput(string userInput)
        {
            return botHandler(userInput);
        }
    }
}