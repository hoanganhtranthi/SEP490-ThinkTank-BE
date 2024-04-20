using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class AccountIn1vs1Request
    {
        public int? WinnerId { get; set; }
        public int? AccountId1 { get; set; }
        public int? AccountId2 { get; set; }
        public int? GameId { get; set; }
    }
}
