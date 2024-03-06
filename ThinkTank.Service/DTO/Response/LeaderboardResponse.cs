using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class LeaderboardResponse
    {
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public int Mark { get; set; }
        public int Rank { get; set; }
    }
}
