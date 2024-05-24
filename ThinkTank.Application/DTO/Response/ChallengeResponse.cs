

namespace ThinkTank.Application.DTO.Response
{
    public class ChallengeResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Avatar { get; set; }
        public string? Description { get; set; }
        public int? CompletedMilestone { get; set; }
        public string? Unit { get; set; }
        public int? CompletedLevel { get; set; }
        public bool? Status { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? MissionsImg { get; set; } 
    }
}
