using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Anonymou
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int Characteristic { get; set; }
        public string LinkImg { get; set; } = null!;
        public int TopicOfGameId { get; set; }

        public virtual TopicOfGame TopicOfGame { get; set; } = null!;
    }
}
