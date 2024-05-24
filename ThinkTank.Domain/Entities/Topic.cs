using System;
using System.Collections.Generic;

namespace ThinkTank.Domain.Entities
{
    public partial class Topic
    {
        public Topic()
        {
            Assets = new HashSet<Asset>();
            Rooms = new HashSet<Room>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? GameId { get; set; }

        public virtual Game? Game { get; set; }
        public virtual ICollection<Asset> Assets { get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
