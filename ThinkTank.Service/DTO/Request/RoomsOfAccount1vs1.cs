using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class RoomsOfAccount1vs1
    {
        public string Id { get; set; }
        public int AccountId1 { get; set; }
        public int AccountId2 { get; set; }
        public decimal TimeId1 { get; set; }
        public decimal TimeId2 { get; set; }
        public string Message { get; set; }
    }
}
