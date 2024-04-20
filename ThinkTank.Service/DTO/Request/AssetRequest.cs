using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class AssetRequest
    {
        public int? TopicId { get; set; }
        public int? GameId { get; set; }
        public int? Version { get; set; }
    }
}
