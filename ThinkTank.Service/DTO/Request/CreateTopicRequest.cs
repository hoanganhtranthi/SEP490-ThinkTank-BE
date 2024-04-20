using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateTopicRequest
    {
        public string Name { get; set; }
        public int GameId { get; set; }
    }
}
