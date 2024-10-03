using System.ComponentModel.DataAnnotations.Schema;

namespace server.internals.dbMigrations.tables
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; } 
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public int UserStatus { get; set; }
        public Token Token { get; set; }
        public ProfilePicture ProfilePicture { get; set; }
    }
}
