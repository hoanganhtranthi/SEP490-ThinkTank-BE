using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreatePrizeOfContestRequest
    {
        public int FromRank { get; set; }
        public int ToRank { get; set; }
        public int Prize { get; set; }
        public int ContestId { get; set; }
    }
}
