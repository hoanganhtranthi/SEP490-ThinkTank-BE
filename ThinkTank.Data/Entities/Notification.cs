using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Notification
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public DateTime DateNotification { get; set; }
        public string Avatar { get; set; } = null!;
        public int AccountId { get; set; }
        public string? Title { get; set; }
        public bool? Status { get; set; }

        public virtual Account Account { get; set; } = null!;
    }
}
