using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class FlipCardAndImagesWalkthroughOfContest
    {
        public int Id { get; set; }
        public string LinkImg { get; set; } = null!;
        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; } = null!;
    }
}
