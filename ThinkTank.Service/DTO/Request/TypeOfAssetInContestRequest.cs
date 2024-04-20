using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class TypeOfAssetInContestRequest
    {
        public string? Type { get; set; }
        public int? ContestId { get; set; }
    }
}
