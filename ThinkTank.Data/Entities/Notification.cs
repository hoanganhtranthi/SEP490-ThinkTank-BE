using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Notification
    {
        public int Id { get; set; }
        public string Titile { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DateTime { get; set; }
        public string Avatar { get; set; } = null!;
        public int AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;
    }
}
