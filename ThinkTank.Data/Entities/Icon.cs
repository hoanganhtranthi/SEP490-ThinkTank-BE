using System;
using System.Collections.Generic;

namespace ThinkTank.Domain.Entities
{
    public partial class Icon
    {
        public Icon()
        {
            IconOfAccounts = new HashSet<IconOfAccount>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public int Price { get; set; }
        public bool Status { get; set; }

        public virtual ICollection<IconOfAccount> IconOfAccounts { get; set; }
    }
}
