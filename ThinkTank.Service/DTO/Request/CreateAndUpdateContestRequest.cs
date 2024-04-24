
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAndUpdateContestRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Thumbnail { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        [Range(100, 300, ErrorMessage = "Only positive number allowed")]
        public int CoinBetting { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int GameId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public decimal PlayTime { get; set; }
        [Required]
        public List<CreateAssetOfContestRequest> Assets { get; set; }
    }
}
