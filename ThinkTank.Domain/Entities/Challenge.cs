using System;
using System.Collections.Generic;

namespace ThinkTank.Domain.Entities
{
    public partial class Challenge
    {
        public Challenge()
        {
            Badges = new HashSet<Badge>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CompletedMilestone { get; set; }
        public string Unit { get; set; } = null!;
        public bool Status { get; set; }
        public string MissionsImg { get; set; } = null!;

        public virtual ICollection<Badge> Badges { get; set; }
    }
}
