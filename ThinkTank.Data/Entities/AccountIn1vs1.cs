using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class AccountIn1vs1
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Coin { get; set; }
        public int WinnerId { get; set; }
        public int AccountId1 { get; set; }
        public int AccountId2 { get; set; }
        public string RoomOfAccountIn1vs1Id { get; set; }
        public int GameId { get; set; }

        public virtual Account AccountId1Navigation { get; set; } = null!;
        public virtual Account AccountId2Navigation { get; set; } = null!;
        public virtual Game Game { get; set; } = null!;
    }
}
