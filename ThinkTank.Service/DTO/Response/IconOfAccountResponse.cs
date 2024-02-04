using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class IconOfAccountResponse
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; }
        public int AccountId { get; set; }
        public string UserName { get; set; }
    }
}
