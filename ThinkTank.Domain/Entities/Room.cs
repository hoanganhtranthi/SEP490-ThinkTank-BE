using System;
using System.Collections.Generic;

namespace ThinkTank.Domain.Entities
{
    public partial class Room
    {
        public Room()
        {
            AccountInRooms = new HashSet<AccountInRoom>();
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public int AmountPlayer { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool? Status { get; set; }
        public int TopicId { get; set; }
        public string Name { get; set; } = null!;

        public virtual Topic Topic { get; set; } = null!;
        public virtual ICollection<AccountInRoom> AccountInRooms { get; set; }
    }
}
