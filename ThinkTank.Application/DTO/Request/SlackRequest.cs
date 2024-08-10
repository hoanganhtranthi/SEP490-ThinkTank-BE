using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Application.DTO.Request
{
    public class SlackRequest
    {
        public string channel { get; set; }
        public string? thread_ts { get; set; }
        public string? ts { get; set; }
        public List<object> blocks { get; set; }
    }
}
