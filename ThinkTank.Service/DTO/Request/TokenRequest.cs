
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Application.DTO.Request
{
    public class TokenRequest
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
