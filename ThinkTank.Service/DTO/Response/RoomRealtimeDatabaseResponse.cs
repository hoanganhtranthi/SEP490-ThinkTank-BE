using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class RoomRealtimeDatabaseResponse
    {
        public int AmountPlayer { get; set; }
        public int OwnerId { get; set; }
        public string RoomName { get; set; }
        public int TopicId { get; set; }
        public int AmountPlayerDone { get; set; }
    }
}
