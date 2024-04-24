

namespace ThinkTank.Service.DTO.Request
{
    public class LoginRequest
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Fcm { get; set; } = null!;
    }
}
