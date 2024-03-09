using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Contest
    {
        public Contest()
        {
            AccountInContests = new HashSet<AccountInContest>();
            AssetOfContests = new HashSet<AssetOfContest>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Thumbnail { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool? Status { get; set; }
        public int? CoinBetting { get; set; }
        public int? GameId { get; set; }

        public virtual Game? Game { get; set; }
        public virtual ICollection<AccountInContest> AccountInContests { get; set; }
        public virtual ICollection<AssetOfContest> AssetOfContests { get; set; }
    }
}
