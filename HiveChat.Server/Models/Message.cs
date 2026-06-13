namespace HiveChat.Server.Models;

public class Message
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
