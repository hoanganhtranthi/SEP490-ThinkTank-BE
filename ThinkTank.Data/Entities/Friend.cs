using System;
using System.Collections.Generic;

namespace ThinkTank.Domain.Entities
{
    public partial class Friend
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public int AccountId1 { get; set; }
        public int AccountId2 { get; set; }

        public virtual Account AccountId1Navigation { get; set; } = null!;
        public virtual Account AccountId2Navigation { get; set; } = null!;
    }
}
