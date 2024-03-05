using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateReportRequest
    {
        public string? Description { get; set; } = null!;
        public int? AccountId1 { get; set; }
        public int? AccountId2 { get; set; }
        public string? Titile { get; set; } = null!;
    }
}
