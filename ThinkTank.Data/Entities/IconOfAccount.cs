using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class IconOfAccount
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; }
        public int AccountId { get; set; }
        public int IconId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Icon Icon { get; set; } = null!;
    }
}
