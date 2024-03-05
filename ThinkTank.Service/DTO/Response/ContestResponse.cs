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
    }
}
