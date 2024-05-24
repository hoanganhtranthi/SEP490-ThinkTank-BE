
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Application.DTO.Request
{
    public class AnalysisRequest
    {
        [Required]
        public int AccountId { get; set; }
        [Required]
        public int GameId { get; set; }
        public int? FilterMonth { get; set; }
        public int? FilterYear { get; set; }
    }
}
