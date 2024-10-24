namespace server.internals.dbMigrations.tables
{
    public class ChatToUser
    {
        public ulong UserID { get; set; }
        public User User { get; set; }
        public ulong ChatID { get; set; }
        public Chat Chat { get; set; }
    }
}
