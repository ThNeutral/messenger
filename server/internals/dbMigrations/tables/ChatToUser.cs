namespace server.internals.dbMigrations.tables
{
    public class ChatToUser
    {
        public int UserID { get; set; }
        public User User { get; set; }
        public int ChatID { get; set; }
        public Chat Chat { get; set; }
    }
}
