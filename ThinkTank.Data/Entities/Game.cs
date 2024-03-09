using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Game
    {
        public Game()
        {
            Contests = new HashSet<Contest>();
            Topics = new HashSet<Topic>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Contest> Contests { get; set; }
        public virtual ICollection<Topic> Topics { get; set; }
    }
}
