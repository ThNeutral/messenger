using System.ComponentModel.DataAnnotations.Schema;

namespace server.internals.dbMigrations.tables
{
    public class Chat
    {
        public ulong ChatID { get; set; }
        public string ChatName { get; set; }
        public ICollection<ChatToUser> ChatsToUsers { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
