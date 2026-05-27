using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NET_Guardian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.cs
    /// </summary>
    public partial class MainWindow : Window
    {
        private responses chatManager;
        private audio audioManager;

        public MainWindow()
        {
            InitializeComponent();
            chatManager = new responses();
            audioManager = new audio();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            audioManager.PlayAudioGreeting(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    AddMessage("Hello! Welcome to NET Guardian Chatbot. What is your name?", isBot: true);
                    chatManager.IsNameAsked = true;
                });
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
                chatManager.LoadUserMemory(input);
                string greeting = $"Nice to meet you, {chatManager.UserName}! Let's talk about cybersecurity. What do you want to learn about?";

                if (!string.IsNullOrEmpty(chatManager.FavouriteTopic))
                    greeting = $"Welcome back, {chatManager.UserName}! I remember your favourite topic is {chatManager.FavouriteTopic}. What can I help you with today?";

                chatManager.AddToChatHistory("Bot: " + greeting);
                AddMessage(greeting, isBot: true);
                return;
            }
            string response = chatManager.ProcessUserInput(input);
            chatManager.AddToChatHistory("Bot: " + response);
            AddMessage(response, isBot: true);
        }

        private void AddMessage(string message, bool isBot)
        {
            /* The code below was extracted from
             * Microsoft. (2024). WPF Controls — TextBlock, Border, StackPanel, Separator. 
             * Microsoft Learn. https://learn.microsoft.com/dotnet/desktop/wpf/controls
            */
            // Small label above the bubble showing who sent the message
            TextBlock senderLabel = new TextBlock();
            senderLabel.Text = isBot ? "NET GUARDIAN" : "You";
            senderLabel.FontSize = 10;
            senderLabel.FontWeight = FontWeights.Bold;
            senderLabel.Margin = new Thickness(isBot ? 2 : 0, 0, isBot ? 0 : 2, 2);
            senderLabel.HorizontalAlignment = isBot ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            senderLabel.Foreground = new SolidColorBrush(
                isBot
                ? (Color)ColorConverter.ConvertFromString("#CCA43B")
                : (Color)ColorConverter.ConvertFromString("#363636"));

            // The actual message content
            TextBlock messageText = new TextBlock();
            messageText.Text = message;
            messageText.FontSize = 13;
            messageText.TextWrapping = TextWrapping.Wrap;
            messageText.Foreground = new SolidColorBrush(
                isBot
                ? (Color)ColorConverter.ConvertFromString("#242F40")
                : Colors.White);

            // Bubble container for the message
            Border bubble = new Border();
            bubble.Padding = new Thickness(10, 8, 10, 8);
            bubble.MaxWidth = 310;
            bubble.BorderThickness = new Thickness(1);
            bubble.Child = messageText;
            bubble.Background = new SolidColorBrush(
                isBot
                ? (Color)ColorConverter.ConvertFromString("#F0F0F0")
                : (Color)ColorConverter.ConvertFromString("#CCA43B"));
            bubble.BorderBrush = new SolidColorBrush(
                isBot
                ? (Color)ColorConverter.ConvertFromString("#CCA43B")
                : (Color)ColorConverter.ConvertFromString("#a07f28"));

            // Stack the label and bubble together
            StackPanel messageBlock = new StackPanel();
            messageBlock.Orientation = Orientation.Vertical;
            messageBlock.HorizontalAlignment = isBot ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            messageBlock.Margin = new Thickness(isBot ? 0 : 60, 4, isBot ? 60 : 0, 4);
            messageBlock.Children.Add(senderLabel);
            messageBlock.Children.Add(bubble);

            // Thin line to separate each message
            Separator divider = new Separator();
            divider.Height = 1;
            divider.Margin = new Thickness(0, 4, 0, 0);
            divider.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

            lstChat.Items.Add(messageBlock);
            lstChat.Items.Add(divider);
            chatScroller.ScrollToEnd();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            lstChat.Items.Clear();
            AddMessage($"Chat cleared. How else can I help you, {chatManager.UserName}?", isBot: true);
        }
    }
}