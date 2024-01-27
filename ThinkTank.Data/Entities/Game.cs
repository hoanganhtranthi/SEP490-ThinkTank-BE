using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Game
    {
        public Game()
        {
            AccountIn1vs1s = new HashSet<AccountIn1vs1>();
            Achievements = new HashSet<Achievement>();
            Rooms = new HashSet<Room>();
            TopicOfGames = new HashSet<TopicOfGame>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<AccountIn1vs1> AccountIn1vs1s { get; set; }
        public virtual ICollection<Achievement> Achievements { get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
        public virtual ICollection<TopicOfGame> TopicOfGames { get; set; }
    }
}
