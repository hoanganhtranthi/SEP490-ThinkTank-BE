
using System.ComponentModel.DataAnnotations;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class AccountInContestResponse
    {
        [Key]
        public int Id { get; set; }
        [IntAttribute]
        public int? ContestId { get; set; }
        [IntAttribute]
        public int? AccountId { get; set; }
        public string? UserName { get; set; }
        public string? ContestName { get; set; }
        public DateTime? CompletedTime { get; set; }
        public decimal? Duration { get; set; }
        public int? Mark { get; set; }
        public int? Prize { get; set; }
        public string? Avatar { get; set; }
        
    }
}
