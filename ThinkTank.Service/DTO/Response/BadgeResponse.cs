using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class BadgeResponse
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Avatar { get; set; }
        public string? Description { get; set; }
        public int CompletedLevel { get; set; }
        public int CompletedMilestone { get; set; }
        public bool Status { get; set; }
        
    }
}
