using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Topic
    {
        public Topic()
        {
            Rooms = new HashSet<Room>();
            TopicOfGames = new HashSet<TopicOfGame>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Room> Rooms { get; set; }
        public virtual ICollection<TopicOfGame> TopicOfGames { get; set; }
    }
}
