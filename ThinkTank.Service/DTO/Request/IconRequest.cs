
using System.ComponentModel.DataAnnotations;
using static ThinkTank.Domain.Enums.Enum;

namespace ThinkTank.Application.DTO.Request
{
    public class IconRequest
    {
        [Required]
        public StatusIconType StatusIcon { get; set; }
        public int? AccountId { get; set; }
        public string? Name { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }

    }
}
