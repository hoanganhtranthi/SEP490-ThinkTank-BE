
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAssetOfContestRequest
    {
        [Required]
        public string Value { get; set; } = null!;
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int TypeOfAssetId { get; set; }
    }
}
