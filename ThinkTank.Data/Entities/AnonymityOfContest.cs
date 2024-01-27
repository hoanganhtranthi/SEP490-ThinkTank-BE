using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class AnonymityOfContest
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int Characteristic { get; set; }
        public string LinkImg { get; set; } = null!;
        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; } = null!;
    }
}
