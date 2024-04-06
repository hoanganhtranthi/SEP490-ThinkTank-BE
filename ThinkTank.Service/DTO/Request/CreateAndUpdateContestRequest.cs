using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAndUpdateContestRequest
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        [Range(50, 200, ErrorMessage = "Only positive number allowed")]
        public int CoinBetting { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int GameId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public decimal PlayTime { get; set; }
        public List<CreateAssetOfContestRequest> Assets { get; set; }
    }
}
