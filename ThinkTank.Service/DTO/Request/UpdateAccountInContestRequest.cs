using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class UpdateAccountInContestRequest
    {
        public DateTime CompletedTime { get; set; }
        public decimal Duration { get; set; }
        public int Mark { get; set; }
        public int Prize { get; set; }
    }
}
