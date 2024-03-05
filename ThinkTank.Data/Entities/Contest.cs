using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Contest
    {
        public Contest()
        {
            AccountInContests = new HashSet<AccountInContest>();
            AnonymityOfContests = new HashSet<AnonymityOfContest>();
            FlipCardAndImagesWalkthroughOfContests = new HashSet<FlipCardAndImagesWalkthroughOfContest>();
            MusicPasswordOfContests = new HashSet<MusicPasswordOfContest>();
            PrizeOfContests = new HashSet<PrizeOfContest>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Thumbnail { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool? Status { get; set; }

        public virtual ICollection<AccountInContest> AccountInContests { get; set; }
        public virtual ICollection<AnonymityOfContest> AnonymityOfContests { get; set; }
        public virtual ICollection<FlipCardAndImagesWalkthroughOfContest> FlipCardAndImagesWalkthroughOfContests { get; set; }
        public virtual ICollection<MusicPasswordOfContest> MusicPasswordOfContests { get; set; }
        public virtual ICollection<PrizeOfContest> PrizeOfContests { get; set; }
    }
}
