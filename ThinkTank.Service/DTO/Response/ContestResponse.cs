using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class ContestResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Name { get; set; }
        public string? Thumbnail { get; set; }
        [DateRangeAttribute]
        public DateTime? StartTime { get; set; }
        [DateRangeAttribute]
        public DateTime? EndTime { get; set; }
        public bool? Status { get; set; }
        [IntAttribute]
        public int? GameId { get; set; }
        public decimal? PlayTime { get; set; }
        public string?  GameName { get; set; }
        public int? AmoutPlayer { get; set; }
        public int? CoinBetting { get; set; }
        public virtual ICollection<AssetOfContestResponse> AssetOfContests { get; set; }

    }
}
