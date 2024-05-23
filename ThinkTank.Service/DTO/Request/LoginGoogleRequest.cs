
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Application.DTO.Request
{
    public class LoginGoogleRequest
    {
        public string GoogleId { get; set; }
        public string FCM { get; set; }
        public string Avatar { get; set; }
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
      @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}
