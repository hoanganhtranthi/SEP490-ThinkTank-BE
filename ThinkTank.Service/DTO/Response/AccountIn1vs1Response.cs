using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class AccountIn1vs1Response
    {
        public int Id { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Coin { get; set; }
        public int? WinnerId { get; set; }
        public int? AccountId1 { get; set; }
        public int? AccountId2 { get; set; }
        public int? GameId { get; set; }
        public string? GameName { get; set; }
        public string? Username1 { get; set; }
        public string? Username2 { get; set; }
    }
}
