namespace server.internals.dbMigrations.tables
{
    public class WatchedBy
    {
        public int MessageID { get; set; }
        public Message Message { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
    }
}
