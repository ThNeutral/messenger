namespace server.internals.dbMigrations.tables
{
    public class WatchedBy
    {
        public ulong MessageID { get; set; }
        public Message Message { get; set; }
        public ulong UserID { get; set; }
        public User User { get; set; }
    }
}
