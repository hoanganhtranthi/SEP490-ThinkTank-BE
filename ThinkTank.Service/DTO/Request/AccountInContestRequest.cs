using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class AccountInContestRequest
    {
        public int AccountId { get; set; } 
        public int ContestId { get; set; }
    }
}
