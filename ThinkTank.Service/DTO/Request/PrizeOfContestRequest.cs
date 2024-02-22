using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class PrizeOfContestRequest
    {
        public int? FromRank { get; set; }
        public int? ToRank { get; set; }
    }
}
