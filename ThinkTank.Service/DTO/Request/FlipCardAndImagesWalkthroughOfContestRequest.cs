using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class FlipCardAndImagesWalkthroughOfContestRequest
    {
        public string LinkImg { get; set; } = null!;
        public int ContestId { get; set; }
    }
}
