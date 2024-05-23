using System;
using System.Collections.Generic;

namespace ThinkTank.Domain.Entities
{
    public partial class AssetOfContest
    {
        public int Id { get; set; }
        public string Value { get; set; } = null!;
        public int? ContestId { get; set; }
        public int? TypeOfAssetId { get; set; }

        public virtual Contest? Contest { get; set; }
        public virtual TypeOfAssetInContest? TypeOfAsset { get; set; }
    }
}
