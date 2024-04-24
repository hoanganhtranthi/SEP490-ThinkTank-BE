

namespace ThinkTank.Service.DTO.Request
{
    public class CreateBadgeRequest
    {
        public int CompletedLevel { get; set; }
        public int AccountId { get; set; }
        public int ChallengeId { get; set; }
    }
}
