using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email is required !")]
        public string Email { get; set; }
        [Required(ErrorMessage = " New Password is required !")]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Username is invalid")]
        public string Username { get; set; }
    }
}
