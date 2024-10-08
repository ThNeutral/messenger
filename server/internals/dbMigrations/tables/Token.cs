using System.ComponentModel.DataAnnotations.Schema;

namespace server.internals.dbMigrations.tables
{
    public class Token
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong UserID { get; set; }
        public string JWToken { get; set; }
        public long ExpiresAt { get; set; }
        public User User { get; set; }
    }
}
