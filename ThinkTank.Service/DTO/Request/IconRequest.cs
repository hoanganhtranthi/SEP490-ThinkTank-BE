
using System.ComponentModel.DataAnnotations;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.Service.DTO.Request
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
