using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class ReportResponse
    {
        public int Id { get; set; }
        public string? Description { get; set; } = null!;
        [IntAttribute]
        public int? AccountId1 { get; set; }
        [IntAttribute]
        public int? AccountId2 { get; set; }
        public string? Title { get; set; } = null!;
        public string? UserName1 { get; set; }
        public string? UserName2 { get; set; }
        public DateTime? DateReport { get; set; }
    }
}
