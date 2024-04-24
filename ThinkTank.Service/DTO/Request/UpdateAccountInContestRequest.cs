

namespace ThinkTank.Service.DTO.Request
{
    public class UpdateAccountInContestRequest
    {
        public decimal Duration { get; set; }
        public int Mark { get; set; }
        public int AccountId { get; set; }
        public int ContestId { get; set; }
    }
}
