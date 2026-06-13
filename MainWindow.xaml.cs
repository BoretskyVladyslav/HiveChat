using System;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;

namespace HiveChat.Client
{
    public partial class MainWindow : Window
    {
        private HubConnection? _connection;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5284/chat")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string, DateTime>("ReceiveMessage", (username, message, timestamp) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessagesList.Items.Add($"[{timestamp:HH:mm:ss}] {username}: {message}");
                });
            });

            try
            {
                await _connection.StartAsync();
                MessagesList.Items.Add("Connected to server.");
            }
            catch (Exception ex)
            {
                MessagesList.Items.Add($"Connection error: {ex.Message}");
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_connection is null || _connection.State != HubConnectionState.Connected)
            {
                MessagesList.Items.Add("Not connected.");
                return;
            }

            var username = UsernameBox.Text;
            var message = MessageBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                await _connection.InvokeAsync("SendMessage", username, message);
                MessageBox.Clear();
            }
            catch (Exception ex)
            {
                MessagesList.Items.Add($"Send error: {ex.Message}");
            }
        }
    }
}