
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Service.DTO.Request
{
    public class ResetPasswordRequest
    {
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
         @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
         @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = null!;
        [StringLength(20, ErrorMessage = "Username is invalid.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot have spaces")]
        public string Username { get; set; }=null!;
    }
}
