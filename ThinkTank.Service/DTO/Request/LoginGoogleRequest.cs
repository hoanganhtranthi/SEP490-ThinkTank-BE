using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class LoginGoogleRequest
    {
        public string GoogleId { get; set; }
        public string FCM { get; set; }
        public string Avatar { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}
