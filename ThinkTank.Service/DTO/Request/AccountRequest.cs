using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class AccountRequest
    {
        public string? Code { get; set; } 
        public string? FullName { get; set; } 
        public string? UserName { get; set; }
    }
}
