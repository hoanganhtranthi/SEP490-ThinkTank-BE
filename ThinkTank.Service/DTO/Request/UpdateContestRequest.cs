using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class UpdateContestRequest
    {
        public string? Name { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? CoinBetting { get; set; }
    }
}
