using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class AccountInContest
    {
        public int Id { get; set; }
        public DateTime? CompletedTime { get; set; }
        public decimal Duration { get; set; }
        public int Mark { get; set; }
        public int Prize { get; set; }
        public int AccountId { get; set; }
        public int ContestId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Contest Contest { get; set; } = null!;
    }
}
