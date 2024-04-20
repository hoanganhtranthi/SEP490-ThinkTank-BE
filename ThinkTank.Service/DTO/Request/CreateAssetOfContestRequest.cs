using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAssetOfContestRequest
    {
        [Required]
        public string Value { get; set; } = null!;
        public int? TypeOfAssetId { get; set; }
    }
}
