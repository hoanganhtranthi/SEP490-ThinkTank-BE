using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAccountIn1vs1Request
    {
        public DateTime StartTime { get; set; }
        public int Coin { get; set; }
        public int WinnerId { get; set; }
        public int AccountId1 { get; set; }
        public int AccountId2 { get; set; }
        public int GameId { get; set; }
        public string RoomOfAccountIn1vs1Id { get; set; }
    }
}
