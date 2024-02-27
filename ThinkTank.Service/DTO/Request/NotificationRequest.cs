using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class NotificationRequest
    {
        public bool? Status { get; set; }
        public int? AccountId { get; set; }
    }
}
