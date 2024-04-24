
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Service.DTO.Request
{
    public class TokenRequest
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
