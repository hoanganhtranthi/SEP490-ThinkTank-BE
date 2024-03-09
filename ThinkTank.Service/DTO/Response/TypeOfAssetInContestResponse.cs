using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;

namespace ThinkTank.Service.DTO.Response
{
    public class TypeOfAssetInContestResponse
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;

        public virtual ICollection<AssetOfContestResponse> AssetOfContests { get; set; }
    }
}
