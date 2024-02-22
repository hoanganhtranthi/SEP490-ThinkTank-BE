using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class ResourceOfContestRequest
    {
        [Required]
        public int ContestId { get; set; } = 0!;
    }
}
