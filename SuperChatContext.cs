using Microsoft.EntityFrameworkCore;
using SuperChat.API.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace SuperChat.API
{
    public class SuperChatContext : DbContext
    {
        public SuperChatContext(DbContextOptions<SuperChatContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // We can set the DB table names ourselves
            // Otherwise EF will simply use the plurals of the property names as the table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<ChatRoom>().ToTable("ChatRooms");
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessages");
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
