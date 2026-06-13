using Microsoft.EntityFrameworkCore;
using HiveChat.Server.Models;

namespace HiveChat.Server.Data;

public class ChatDbContext : DbContext
{
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=chat.db");
    }
}
