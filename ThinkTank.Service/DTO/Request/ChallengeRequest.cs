
using System.ComponentModel.DataAnnotations;
using static ThinkTank.Domain.Enums.Enum;

namespace ThinkTank.Application.DTO.Request
{
    public class ChallengeRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId { get; set; }
        [Required]
        public StatusType? Status { get; set; }
    }
}
