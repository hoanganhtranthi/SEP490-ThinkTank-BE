using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class AccountInRoomRequest
    {
        public bool? IsAdmin { get; set; }
        public int? AccountId { get; set; }
        public int? RoomId { get; set; }
    }
}
