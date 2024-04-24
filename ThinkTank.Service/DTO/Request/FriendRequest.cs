
using System.ComponentModel.DataAnnotations;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.Service.DTO.Request
{
    public class FriendRequest
    {
        [Required]
        public StatusType Status { get; set; }
        [Required]
        public int AccountId { get; set; }
        public string? UserCode { get; set; }
        public string? UserName { get; set; }
    }
}
