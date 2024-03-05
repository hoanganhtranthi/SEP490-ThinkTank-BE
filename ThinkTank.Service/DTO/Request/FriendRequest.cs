using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.Service.DTO.Request
{
    public class FriendRequest
    {
        public FriendType? Status { get; set; }
        public int? AccountId { get; set; }
        public string? UserName { get; set; }
        public string? UserCode { get; set; }
    }
}
