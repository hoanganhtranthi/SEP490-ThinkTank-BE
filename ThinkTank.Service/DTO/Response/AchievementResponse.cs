
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class AchievementResponse
    {
        public int Id { get; set; }
        public DateTime? CompletedTime { get; set; }
        public decimal? Duration { get; set; }
        public int? Mark { get; set; }
        [IntAttribute]
        public int? Level { get; set; }
        [IntAttribute]
        public int? AccountId { get; set; }
        public string? Username { get; set; }
        [IntAttribute]
        public int? GameId { get; set; }
        [IntAttribute]
        public int? PieceOfInformation { get; set; }
        public string? GameName { get; set; }
    }
}
