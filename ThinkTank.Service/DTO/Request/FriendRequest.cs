
using System.ComponentModel.DataAnnotations;
using static ThinkTank.Domain.Enums.Enum;

namespace ThinkTank.Application.DTO.Request
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
