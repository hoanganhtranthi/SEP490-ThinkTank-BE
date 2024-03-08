using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class AchievementRequest
    {
        public int? Level { get; set; }
        public int? AccountId { get; set; }
        public int? TopicId { get; set; }
        public int? PieceOfInformation { get; set; }
    }
}
