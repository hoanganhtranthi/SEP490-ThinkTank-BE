using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Achievement
    {
        public int Id { get; set; }
        public DateTime CompletedTime { get; set; }
        public decimal Duration { get; set; }
        public int Mark { get; set; }
        public int Level { get; set; }
        public int GameId { get; set; }
        public int AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Game Game { get; set; } = null!;
    }
}
