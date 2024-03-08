using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;

namespace ThinkTank.Service.DTO.Request
{
    public class TypeOfAssetRequest
    {
        public string? Type { get; set; } = null!;
        public int? TopicId { get; set; }
        public int? GameId { get; set; }
        public int? Version { get; set; }
    }
}
