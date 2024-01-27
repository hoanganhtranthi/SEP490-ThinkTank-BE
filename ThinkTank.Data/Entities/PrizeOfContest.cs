using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class PrizeOfContest
    {
        public int Id { get; set; }
        public int FromRank { get; set; }
        public int ToRank { get; set; }
        public int Prize { get; set; }
        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; } = null!;
    }
}
