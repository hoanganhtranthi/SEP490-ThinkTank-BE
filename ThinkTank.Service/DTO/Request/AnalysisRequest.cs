using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class AnalysisRequest
    {
        [Required]
        public int AccountId { get; set; }
        [Required]
        public int GameId { get; set; }
        public int? FilterMonth { get; set; }
        public int? FilterYear { get; set; }
    }
}
