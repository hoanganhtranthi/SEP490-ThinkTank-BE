using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAccountRequest
    {
        public string FullName { get; set; } = null!;
        [StringLength(20, ErrorMessage = "Username is invalid.")]
        public string UserName { get; set; } = null!;
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
         @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
         @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = null!;
        [RegularExpression("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,12}$", ErrorMessage = "Password is invalid.")]
        public string Password { get; set; } = null!;
        public string? Fcm { get; set; }

    }
}
