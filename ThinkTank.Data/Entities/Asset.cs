using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class Asset
    {
        public int Id { get; set; }
        public string Value { get; set; } = null!;
        public int? TopicId { get; set; }
        public int? TypeOfAssetId { get; set; }
        public int Version { get; set; }
        public virtual Topic? Topic { get; set; }
        public virtual TypeOfAsset? TypeOfAsset { get; set; }
    }
}
