using SQLite;
using System;
using System.Collections.Generic;

namespace NeuroMate.Database.Entities
{
    public class PlayerProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string Username { get; set; } = string.Empty;
        public int Points { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int SelectedAvatarId { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLogin { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
