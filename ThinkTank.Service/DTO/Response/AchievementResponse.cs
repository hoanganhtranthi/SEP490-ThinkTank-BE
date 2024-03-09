using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int? TopicId { get; set; }
        [IntAttribute]
        public int? PieceOfInformation { get; set; }
        public string? TopicName { get; set; }
        public string? GameName { get; set; }
    }
}
