using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateRoomRequest
    {
        public string Name { get; set; }
        public int AmountPlayer { get; set; }
        public int TopicId { get; set; }
        public int AccountId { get; set; }
    }
}
