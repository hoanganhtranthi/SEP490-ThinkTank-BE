using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class ChallengeResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Name { get; set; }
        public string? Avatar { get; set; }
        public string? Description { get; set; }
        public int CompletedMilestone { get; set; }
        public string Unit { get; set; } = null!;
        public bool Status { get; set; }
    }
}
