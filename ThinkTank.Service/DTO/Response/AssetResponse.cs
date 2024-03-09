using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class AssetResponse
    {
        public int Id { get; set; }
        public string? Value { get; set; }
        [IntAttribute]
        public int? TopicId { get; set; }
        public int? GameId { get; set; }
        public string? GameName { get; set; }
        public string? TopicName { get; set; }
        public int? Version { get; set; }
    }
}
