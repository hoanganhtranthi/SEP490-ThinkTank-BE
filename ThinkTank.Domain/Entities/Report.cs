using System;
using System.Collections.Generic;

namespace ThinkTank.Domain.Entities
{
    public partial class Report
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int AccountId1 { get; set; }
        public int AccountId2 { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? DateReport { get; set; }

        public virtual Account AccountId1Navigation { get; set; } = null!;
        public virtual Account AccountId2Navigation { get; set; } = null!;
    }
}
