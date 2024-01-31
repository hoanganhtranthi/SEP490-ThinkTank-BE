using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class MusicPasswordRequest
    {
        public string Password { get; set; } = null!;
        public int TopicOfGameId { get; set; }
    }
}
