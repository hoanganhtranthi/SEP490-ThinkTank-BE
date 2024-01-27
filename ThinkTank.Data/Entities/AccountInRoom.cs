using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class AccountInRoom
    {
        public int Id { get; set; }
        public bool IsAdmin { get; set; }
        public int AccountId { get; set; }
        public int RoomId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
    }
}
