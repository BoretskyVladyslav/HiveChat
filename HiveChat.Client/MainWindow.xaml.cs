using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.AspNetCore.SignalR.Client;

namespace HiveChat.Client;

public class Message
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ChatMessage
{
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsOwnMessage { get; set; }
    public bool IsSystemMessage { get; set; }

    public string DisplayTime => Timestamp.ToString("HH:mm");
}

public partial class MainWindow : Window
{
    private HubConnection? _connection;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameBox.Text))
        {
            AddSystemMessage("⚠ Please enter a username.");
            return;
        }

        ConnectButton.IsEnabled = false;
        UsernameBox.IsEnabled = false;

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5284/chat")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<List<Message>>("ReceiveHistory", (history) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessagesList.Items.Clear();
                foreach (var msg in history)
                {
                    bool isOwn = msg.Username == UsernameBox.Text;
                    MessagesList.Items.Add(new ChatMessage 
                    { 
                        Username = msg.Username, 
                        Text = msg.Text, 
                        Timestamp = msg.Timestamp, 
                        IsOwnMessage = isOwn 
                    });
                }
                ScrollToBottom();
            });
        });

        _connection.On<string, string, DateTime>("ReceiveMessage", (username, message, timestamp) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                bool isOwn = username == UsernameBox.Text;
                
                MessagesList.Items.Add(new ChatMessage 
                { 
                    Username = username, 
                    Text = message, 
                    Timestamp = timestamp, 
                    IsOwnMessage = isOwn 
                });
                
                ScrollToBottom();

                if (!isOwn)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                }
            });
        });

        _connection.Closed += error =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Disconnected";
                StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
                AddSystemMessage("🔴 Disconnected from server.");
                SendButton.IsEnabled = false;
                MessageBox.IsEnabled = false;
                ConnectButton.IsEnabled = true;
                UsernameBox.IsEnabled = true;
            });
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Connected";
                StatusIndicator.Fill = new SolidColorBrush(Colors.LimeGreen);
                AddSystemMessage("🟢 Reconnected to server.");
                SendButton.IsEnabled = true;
                MessageBox.IsEnabled = true;
            });
            return Task.CompletedTask;
        };

        try
        {
            await _connection.StartAsync();
            StatusText.Text = "Connected";
            StatusIndicator.Fill = new SolidColorBrush(Colors.LimeGreen);
            AddSystemMessage("🟢 Connected to server.");
            SendButton.IsEnabled = true;
            MessageBox.IsEnabled = true;
            MessageBox.Focus();
            
            await _connection.InvokeAsync("SendMessage", UsernameBox.Text, "приєднався до чату 👋");
        }
        catch (Exception ex)
        {
            AddSystemMessage($"🔴 Connection error: {ex.Message}");
            ConnectButton.IsEnabled = true;
            UsernameBox.IsEnabled = true;
        }
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        await SendMessageAsync();
    }

    private void MessageBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
        {
            e.Handled = true;
            _ = SendMessageAsync();
        }
    }

    private async Task SendMessageAsync()
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
        {
            AddSystemMessage("⚠ Not connected.");
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
            MessageBox.Focus();
        }
        catch (Exception ex)
        {
            AddSystemMessage($"🔴 Send error: {ex.Message}");
        }
    }

    private void AddSystemMessage(string text)
    {
        MessagesList.Items.Add(new ChatMessage 
        { 
            Text = text, 
            IsSystemMessage = true,
            Timestamp = DateTime.Now
        });
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        if (MessagesList.Items.Count > 0)
            MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
    }
}