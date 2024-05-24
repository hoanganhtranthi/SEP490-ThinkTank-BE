
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Application.DTO.Response
{
    public class AssetOfContestResponse
    {
        [Key]
        public int Id { get; set; }
        public string Value { get; set; } = null!;
        public string Answer { get; set; }
        public int? ContestId { get; set; }
        public string NameOfContest { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
