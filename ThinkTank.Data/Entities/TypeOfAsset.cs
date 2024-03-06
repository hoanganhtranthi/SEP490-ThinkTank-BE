using System;
using System.Collections.Generic;

namespace ThinkTank.Data.Entities
{
    public partial class TypeOfAsset
    {
        public TypeOfAsset()
        {
            Assets = new HashSet<Asset>();
        }

        public int Id { get; set; }
        public string Type { get; set; } = null!;

        public virtual ICollection<Asset> Assets { get; set; }
    }
}
