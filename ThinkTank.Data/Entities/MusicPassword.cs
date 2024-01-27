using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class MusicPassword
    {
        public int Id { get; set; }
        public string Password { get; set; } = null!;
        public int TopicOfGameId { get; set; }

        public virtual TopicOfGame TopicOfGame { get; set; } = null!;
    }
}
