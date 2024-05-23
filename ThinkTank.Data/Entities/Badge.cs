using System;
using System.Collections.Generic;

namespace ThinkTank.Domain.Entities
{
    public partial class Badge
    {
        public int Id { get; set; }
        public int CompletedLevel { get; set; }
        public bool Status { get; set; }
        public int AccountId { get; set; }
        public int ChallengeId { get; set; }
        public DateTime? CompletedDate { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Challenge Challenge { get; set; } = null!;
    }
}
