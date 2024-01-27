using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateFriendRequest
    {
        public int AccountId1 { get; set; }
        public int AccountId2 { get; set; }
    }
}
