﻿using System.ComponentModel.DataAnnotations.Schema;

namespace server.internals.dbMigrations.tables
{
    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageID { get; set; }
        public string Content { get; set; }
        public long SendTime { get; set; }
        public bool IsRedirect { get; set; }
        public int ChatID { get; set; }
        public Chat Chat { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public ICollection<WatchedBy> WatchedBies { get; set; }
    }
}
