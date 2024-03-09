using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class AssetOfContestResponse
    {
        [Key]
        public int Id { get; set; }
        public string Value { get; set; } = null!;
        public int? ContestId { get; set; }
        public string NameOfContest { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
