using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateContestRequest
    {
        public string? Name { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? CoinBetting { get; set; }
        public int? TypeOfAssetId { get; set; }
        public string Value { get; set; } = null!;
    }
}
