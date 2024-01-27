using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class FlipCardAndImagesWalkthrough
    {
        public int Id { get; set; }
        public string LinkImg { get; set; } = null!;
        public int TopicOfGameId { get; set; }

        public virtual TopicOfGame TopicOfGame { get; set; } = null!;
    }
}
