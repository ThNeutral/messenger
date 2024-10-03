using System.ComponentModel.DataAnnotations.Schema;

namespace server.internals.dbMigrations.tables
{
    public class ProfilePicture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }
        public string Base64EncodedImage { get; set; }
        public User User { get; set; }
    }
}
