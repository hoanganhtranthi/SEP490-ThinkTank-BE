
using System.ComponentModel.DataAnnotations;
using static ThinkTank.Domain.Enums.Enum;

namespace ThinkTank.Application.DTO.Request
{
    public class TopicRequest
    {
        public string? Name { get; set; }
        public int? GameId { get; set; }
        [Required]
        public StatusTopicType IsHavingAsset { get; set; }
    }
}
