using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAccountInContestRequest
    {
        public decimal Duration { get; set; }
        public int Mark { get; set; }
        public int AccountId { get; set; }
        public int ContestId { get; set; }
    }
}
