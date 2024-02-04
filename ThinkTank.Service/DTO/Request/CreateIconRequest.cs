using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateIconRequest
    {
        public string Name { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public int Price { get; set; }
    }
}
