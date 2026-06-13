using System;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;

namespace HiveChat.Client;

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
            MessagesList.Items.Add("Please enter a username.");
            return;
        }

        ConnectButton.IsEnabled = false;
        UsernameBox.IsEnabled = false;

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

        _connection.Closed += error =>
        {
            Dispatcher.Invoke(() =>
            {
                MessagesList.Items.Add("Disconnected from server.");
                SendButton.IsEnabled = false;
                MessageBox.IsEnabled = false;
                ConnectButton.IsEnabled = true;
                UsernameBox.IsEnabled = true;
            });
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            Dispatcher.Invoke(() =>
            {
                MessagesList.Items.Add("Reconnected to server.");
                SendButton.IsEnabled = true;
                MessageBox.IsEnabled = true;
            });
            return Task.CompletedTask;
        };

        try
        {
            await _connection.StartAsync();
            MessagesList.Items.Add("Connected to server.");
            SendButton.IsEnabled = true;
            MessageBox.IsEnabled = true;
            MessageBox.Focus();
        }
        catch (Exception ex)
        {
            MessagesList.Items.Add($"Connection error: {ex.Message}");
            ConnectButton.IsEnabled = true;
            UsernameBox.IsEnabled = true;
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
            MessageBox.Focus();
        }
        catch (Exception ex)
        {
            MessagesList.Items.Add($"Send error: {ex.Message}");
        }
    }
}