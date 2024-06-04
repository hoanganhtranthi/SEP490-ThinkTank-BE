

namespace ThinkTank.Application.DTO.Response
{
    public class AdminDashboardResponse
    {
        public AccountResponse AccountResponse { get; set; }
        public int TotalContest { get; set; }
        public int TotalLevel { get; set; }
        public int TotalBadge { get; set; }
        public List<ChallengeResponse> ChallengeResponses { get; set; }
        public double PercentOfFlipcard { get; set; }
        public double PercentOfMusicPassword { get; set; }
        public double PercentOfImagesWalkthrough { get; set; }
        public double PercentOfFindTheAnonymous { get; set; }
    }
}
