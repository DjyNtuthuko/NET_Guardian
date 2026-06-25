using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;

namespace NET_Guardian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly responses chatManager;
        private readonly audio audioManager;
        private readonly TaskManager taskManager;
        private readonly ActivityLogger activityLogger;
        private readonly QuizManager quizManager;
        private bool databaseAvailable;
        private bool quizAnswerChecked;
        private bool quizStarted;
        private bool quizCompleted;
        private bool welcomeShown;

        public MainWindow()
        {
            InitializeComponent();
            chatManager = new responses();
            audioManager = new audio();
            taskManager = new TaskManager();
            activityLogger = new ActivityLogger();
            quizManager = new QuizManager();
            DisplayQuizQuestion();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitialiseDatabase();

            if (databaseAvailable)
            {
                TryLogActivity("App opened", "NET Guardian was opened.");
                LoadTasks();
                LoadActivity();
                CheckDueReminders();
            }

            bool audioStarted = audioManager.PlayAudioGreeting(
                ShowWelcomeMessage,
                message => Dispatcher.Invoke(() =>
                    MessageBox.Show(message, "Audio", MessageBoxButton.OK, MessageBoxImage.Information)));

            if (audioStarted)
            {
                TryLogActivity("Welcome audio played", "The welcome greeting was played.");
            }
            else
            {
                ShowWelcomeMessage();
            }
        }

        private void InitialiseDatabase()
        {
            try
            {
                using NetGuardianDbContext database = new NetGuardianDbContext();
                database.Database.Migrate();
                databaseAvailable = true;
            }
            catch (Exception ex)
            {
                databaseAvailable = false;
                Debug.WriteLine("Database connection error: " + ex.Message);
                MessageBox.Show(
                    "NET Guardian could not connect to MySQL. Start MySQL and check the net_guardian_db connection settings. Chat features will still work.",
                    "Database Connection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void ShowWelcomeMessage()
        {
            Dispatcher.Invoke(() =>
            {
                if (welcomeShown)
                    return;

                welcomeShown = true;
                AddMessage("Hello! Welcome to NET Guardian Chatbot. What is your name?", isBot: true);
                chatManager.IsNameAsked = true;
            });
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ProcessInput();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void ProcessInput()
        {
            string input = txtInput.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Please type a message before sending.", "Empty Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            chatManager.AddToChatHistory("User: " + input);
            AddMessage(input, isBot: false);

            txtInput.Clear();
            txtInput.Focus();

            // First message after the bot asks for a name
            if (chatManager.IsNameAsked && string.IsNullOrEmpty(chatManager.UserName))
            {
                chatManager.UserName = input;
                bool returningUser = chatManager.LoadUserMemory(input);
                string greeting;

                if (returningUser)
                {
                    greeting = string.IsNullOrWhiteSpace(chatManager.FavouriteTopic)
                        ? $"Welcome back, {chatManager.UserName}! What can I help you with today?"
                        : $"Welcome back, {chatManager.UserName}! I remember your favourite topic is {chatManager.FavouriteTopic}. What can I help you with today?";
                    TryLogActivity("Returning user welcomed", $"{chatManager.UserName} was welcomed back.");
                }
                else
                {
                    chatManager.SaveUserMemory();
                    greeting = $"Nice to meet you, {chatManager.UserName}! Let's talk about cybersecurity. What do you want to learn about?";
                    TryLogActivity("User name saved", $"Saved the name {chatManager.UserName}.");
                }

                chatManager.AddToChatHistory("Bot: " + greeting);
                AddMessage(greeting, isBot: true);
                return;
            }

            string response = chatManager.ProcessUserInput(input);

            switch (chatManager.RequestedAction)
            {
                case "OpenTasks":
                    mainTabs.SelectedItem = tabTasks;
                    break;
                case "StartQuiz":
                    OpenQuizFromChat();
                    break;
                case "ShowActivity":
                    mainTabs.SelectedItem = tabActivity;
                    LoadActivity();
                    response = BuildActivitySummary();
                    break;
                case "ShowHistory":
                    TryLogActivity("Chat history viewed", "The user viewed the current chat history.");
                    break;
                case "FavouriteTopicSaved":
                    TryLogActivity("Favourite topic saved", $"Saved {chatManager.FavouriteTopic} as the favourite topic.");
                    break;
            }

            chatManager.AddToChatHistory("Bot: " + response);
            AddMessage(response, isBot: true);
        }

        private void AddMessage(string message, bool isBot)
        {
            /* The code below was extracted from
             * Microsoft. (2024). WPF Controls — TextBlock, Border, StackPanel, Separator.
             * Microsoft Learn. https://learn.microsoft.com/dotnet/desktop/wpf/controls
            */
            TextBlock senderLabel = new TextBlock
            {
                Text = isBot ? "NET GUARDIAN" : "You",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(isBot ? 2 : 0, 0, isBot ? 0 : 2, 2),
                HorizontalAlignment = isBot ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(
                    isBot
                        ? (Color)ColorConverter.ConvertFromString("#CCA43B")
                        : (Color)ColorConverter.ConvertFromString("#363636"))
            };

            TextBlock messageText = new TextBlock
            {
                Text = message,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(
                    isBot
                        ? (Color)ColorConverter.ConvertFromString("#242F40")
                        : Colors.White)
            };

            Border bubble = new Border
            {
                Padding = new Thickness(10, 8, 10, 8),
                MaxWidth = 600,
                BorderThickness = new Thickness(1),
                Child = messageText,
                Background = new SolidColorBrush(
                    isBot
                        ? (Color)ColorConverter.ConvertFromString("#F0F0F0")
                        : (Color)ColorConverter.ConvertFromString("#CCA43B")),
                BorderBrush = new SolidColorBrush(
                    isBot
                        ? (Color)ColorConverter.ConvertFromString("#CCA43B")
                        : (Color)ColorConverter.ConvertFromString("#a07f28"))
            };

            StackPanel messageBlock = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = isBot ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                Margin = new Thickness(isBot ? 0 : 60, 4, isBot ? 60 : 0, 4)
            };
            messageBlock.Children.Add(senderLabel);
            messageBlock.Children.Add(bubble);

            Separator divider = new Separator
            {
                Height = 1,
                Margin = new Thickness(0, 4, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"))
            };

            lstChat.Items.Add(messageBlock);
            lstChat.Items.Add(divider);
            chatScroller.ScrollToEnd();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            lstChat.Items.Clear();
            string name = string.IsNullOrWhiteSpace(chatManager.UserName) ? "there" : chatManager.UserName;
            AddMessage($"Chat cleared. How else can I help you, {name}?", isBot: true);
        }

        private void ImgLogo_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            imgLogo.Visibility = Visibility.Collapsed;
            MessageBox.Show(
                "The NET Guardian logo file could not be loaded.",
                "Logo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTaskTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "Task Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTaskTitle.Focus();
                return;
            }

            if (!EnsureDatabaseAvailable())
                return;

            try
            {
                string priority = ((ComboBoxItem)cmbPriority.SelectedItem).Content.ToString() ?? "Medium";
                GuardianTask task = taskManager.AddTask(
                    title,
                    txtTaskDescription.Text.Trim(),
                    priority,
                    dpReminderDate.SelectedDate);

                TryLogActivity("Task added", $"Added task: {task.Title} ({task.Priority}).");
                txtTaskTitle.Clear();
                txtTaskDescription.Clear();
                dpReminderDate.SelectedDate = null;
                cmbPriority.SelectedIndex = 1;
                LoadTasks();
                MessageBox.Show("Task saved successfully.", "Task Added", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowDatabaseError("The task could not be saved.", ex);
            }
        }

        private void BtnCompleteTask_Click(object sender, RoutedEventArgs e)
        {
            GuardianTask? selectedTask = dgTasks.SelectedItem as GuardianTask;
            if (selectedTask == null)
            {
                MessageBox.Show("Please select a task to complete.", "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedTask.IsCompleted)
            {
                MessageBox.Show("This task is already completed.", "Task Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                taskManager.CompleteTask(selectedTask.GuardianTaskId);
                TryLogActivity("Task completed", $"Completed task: {selectedTask.Title}.");
                LoadTasks();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("The selected task could not be completed.", ex);
            }
        }

        private void BtnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            GuardianTask? selectedTask = dgTasks.SelectedItem as GuardianTask;
            if (selectedTask == null)
            {
                MessageBox.Show("Please select a task to delete.", "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Delete the task '{selectedTask.Title}'?",
                "Delete Task",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                taskManager.DeleteTask(selectedTask.GuardianTaskId);
                TryLogActivity("Task deleted", $"Deleted task: {selectedTask.Title}.");
                LoadTasks();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("The selected task could not be deleted.", ex);
            }
        }

        private void BtnRefreshTasks_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }

        private void LoadTasks()
        {
            if (!databaseAvailable)
            {
                dgTasks.ItemsSource = null;
                return;
            }

            try
            {
                dgTasks.ItemsSource = taskManager.GetTasks();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Tasks could not be loaded.", ex);
            }
        }

        // Checks if a reminder is due
        private void CheckDueReminders()
        {
            try
            {
                List<GuardianTask> reminders = taskManager.GetDueReminders();
                foreach (GuardianTask task in reminders)
                {
                    MessageBox.Show(
                        $"Reminder: {task.Title}\nDue date: {task.ReminderDate:yyyy-MM-dd}",
                        "Task Reminder",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    TryLogActivity("Reminder shown", $"Reminder shown for task: {task.Title}.");
                }
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Task reminders could not be checked.", ex);
            }
        }

        private void DisplayQuizQuestion()
        {
            QuizQuestion question = quizManager.CurrentQuestion;
            lblQuizCategory.Text = question.Category;
            lblQuizProgress.Text = $"Question {quizManager.CurrentQuestionIndex + 1} of {quizManager.Questions.Count}";
            txtQuizQuestion.Text = question.QuestionText;
            txtQuizFeedback.Text = "Choose an answer, then select Next.";
            pnlQuizOptions.Children.Clear();

            foreach (string option in question.Options)
            {
                RadioButton answer = new RadioButton
                {
                    Content = option,
                    GroupName = "QuizAnswers",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242F40")),
                    FontSize = 14,
                    Margin = new Thickness(0, 7, 0, 7),
                    Tag = option
                };
                pnlQuizOptions.Children.Add(answer);
            }

            quizAnswerChecked = false;
            quizCompleted = false;
            btnQuizNext.Content = "Next";
            btnQuizNext.IsEnabled = true;
        }

        private void BtnQuizNext_Click(object sender, RoutedEventArgs e)
        {
            if (quizCompleted)
                return;

            if (quizAnswerChecked)
            {
                if (quizManager.IsLastQuestion)
                {
                    FinishQuiz();
                }
                else
                {
                    quizManager.MoveNext();
                    DisplayQuizQuestion();
                }
                return;
            }

            RadioButton? selectedAnswer = pnlQuizOptions.Children
                .OfType<RadioButton>()
                .FirstOrDefault(answer => answer.IsChecked == true);

            if (selectedAnswer == null)
            {
                MessageBox.Show("Please select an answer before continuing.", "No Answer Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            (bool isCorrect, string feedback) = quizManager.SubmitAnswer(selectedAnswer.Tag?.ToString() ?? "");
            txtQuizFeedback.Text = feedback;
            txtQuizFeedback.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(isCorrect ? "#287A3D" : "#9C2F2F"));

            foreach (RadioButton answer in pnlQuizOptions.Children.OfType<RadioButton>())
                answer.IsEnabled = false;

            quizAnswerChecked = true;
            btnQuizNext.Content = quizManager.IsLastQuestion ? "Finish Quiz" : "Next Question";
        }

        private void BtnRestartQuiz_Click(object sender, RoutedEventArgs e)
        {
            RestartQuiz(logStart: true);
        }

        private void RestartQuiz(bool logStart)
        {
            quizManager.Restart();
            quizStarted = true;
            quizCompleted = false;
            txtQuizFeedback.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242F40"));
            DisplayQuizQuestion();

            if (logStart)
                TryLogActivity("Quiz started", "The cybersecurity quiz was started.");
        }

        private void OpenQuizFromChat()
        {
            bool wasStarted = quizStarted;
            mainTabs.SelectedItem = tabQuiz;

            if (wasStarted || !quizStarted)
                RestartQuiz(logStart: true);
        }

        private void FinishQuiz()
        {
            quizCompleted = true;
            string resultMessage = quizManager.GetResultMessage();
            lblQuizCategory.Text = "Quiz Complete";
            lblQuizProgress.Text = $"{quizManager.Score} of {quizManager.Questions.Count}";
            txtQuizQuestion.Text = $"Final score: {quizManager.Score}/{quizManager.Questions.Count}";
            txtQuizFeedback.Text = resultMessage;
            txtQuizFeedback.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242F40"));
            pnlQuizOptions.Children.Clear();
            btnQuizNext.IsEnabled = false;

            TryLogActivity("Quiz completed", $"Completed the quiz with {quizManager.Score}/{quizManager.Questions.Count}.");

            if (!databaseAvailable)
            {
                MessageBox.Show(
                    $"Your score is {quizManager.Score}/{quizManager.Questions.Count}. {resultMessage}\nThe attempt could not be saved because MySQL is unavailable.",
                    "Quiz Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                QuizAttempt attempt = quizManager.SaveAttempt();
                TryLogActivity("Quiz score saved", $"Saved quiz score: {attempt.Score}/{attempt.TotalQuestions}.");
                MessageBox.Show(
                    $"Your score is {attempt.Score}/{attempt.TotalQuestions}. {attempt.ResultMessage}",
                    "Quiz Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowDatabaseError("The quiz score could not be saved.", ex);
            }
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || e.Source != mainTabs)
                return;

            if (mainTabs.SelectedItem == tabTasks)
                LoadTasks();
            else if (mainTabs.SelectedItem == tabActivity)
                LoadActivity();
            else if (mainTabs.SelectedItem == tabQuiz && !quizStarted)
                RestartQuiz(logStart: true);
        }

        private void BtnRefreshActivity_Click(object sender, RoutedEventArgs e)
        {
            LoadActivity(showEmptyMessage: true);
        }

        private void LoadActivity(bool showEmptyMessage = false)
        {
            if (!databaseAvailable)
            {
                dgActivity.ItemsSource = null;
                return;
            }

            try
            {
                List<ActivityLogEntry> entries = activityLogger.GetEntries();
                dgActivity.ItemsSource = entries;

                if (showEmptyMessage && entries.Count == 0)
                {
                    MessageBox.Show("The activity log is empty.", "Activity Log", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowDatabaseError("The activity log could not be loaded.", ex);
            }
        }

        private string BuildActivitySummary()
        {
            if (!databaseAvailable)
                return "I cannot show saved activity because MySQL is currently unavailable.";

            try
            {
                List<ActivityLogEntry> entries = activityLogger.GetEntries();
                if (entries.Count == 0)
                    return "Your activity log is currently empty.";

                IEnumerable<string> recentEntries = entries
                    .Take(10)
                    .Select(entry => $"{entry.CreatedAt:yyyy-MM-dd HH:mm} - {entry.Action}: {entry.Details}");
                return "Here is your recent activity:\n" + string.Join("\n", recentEntries);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Activity summary error: " + ex.Message);
                return "I could not load your activity log. Please check the MySQL connection.";
            }
        }

        private void TryLogActivity(string action, string details)
        {
            if (!databaseAvailable)
                return;

            try
            {
                activityLogger.Log(action, details);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Activity logging error: " + ex.Message);
            }
        }

        private bool EnsureDatabaseAvailable()
        {
            if (databaseAvailable)
                return true;

            MessageBox.Show(
                "This feature needs MySQL. Start MySQL, then restart NET Guardian.",
                "Database Unavailable",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        private void ShowDatabaseError(string message, Exception ex)
        {
            Debug.WriteLine(message + " " + ex.Message);
            MessageBox.Show(
                message + " Please check that MySQL is running and try again.",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}