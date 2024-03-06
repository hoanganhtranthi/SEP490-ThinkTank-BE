using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Topic
    {
        public Topic()
        {
            AccountIn1vs1s = new HashSet<AccountIn1vs1>();
            Achievements = new HashSet<Achievement>();
            Assets = new HashSet<Asset>();
            Rooms = new HashSet<Room>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? GameId { get; set; }

        public virtual Game? Game { get; set; }
        public virtual ICollection<AccountIn1vs1> AccountIn1vs1s { get; set; }
        public virtual ICollection<Achievement> Achievements { get; set; }
        public virtual ICollection<Asset> Assets { get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
