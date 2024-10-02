namespace server.internals.dbMigrations.tables
{
    public class Token
    {
        public int UserID { get; set; }
        public string JWToken { get; set; }
        public User User { get; set; }
    }
}
