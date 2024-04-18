using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.Service.DTO.Request
{
    public class TopicRequest
    {
        public string? Name { get; set; }
        public int? GameId { get; set; }
        [Required]
        public StatusTopicType IsHavingAsset { get; set; }
    }
}
