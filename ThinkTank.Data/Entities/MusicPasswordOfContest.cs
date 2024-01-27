using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class MusicPasswordOfContest
    {
        public int Id { get; set; }
        public string Password { get; set; } = null!;
        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; } = null!;
    }
}
