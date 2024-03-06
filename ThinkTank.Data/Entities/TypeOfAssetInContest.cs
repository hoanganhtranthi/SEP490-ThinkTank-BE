using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class TypeOfAssetInContest
    {
        public TypeOfAssetInContest()
        {
            AssetOfContests = new HashSet<AssetOfContest>();
        }

        public int Id { get; set; }
        public string Type { get; set; } = null!;

        public virtual ICollection<AssetOfContest> AssetOfContests { get; set; }
    }
}
