using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAchievementRequest
    {
        public decimal Duration { get; set; }
        public int Mark { get; set; }
        public int Level { get; set; }
        public int AccountId { get; set; }
        public int GameId { get; set; }
        public int PieceOfInformation { get; set; }
    }
}
