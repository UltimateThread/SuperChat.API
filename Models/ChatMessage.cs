#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace SuperChat.API.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public User Sender { get; set; }
        public ChatRoom ChatRoom { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
