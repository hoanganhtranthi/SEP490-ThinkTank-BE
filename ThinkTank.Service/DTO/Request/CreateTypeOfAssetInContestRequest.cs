using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateTypeOfAssetInContestRequest
    {
        [Required]
        public string Type { get; set; } = null!;

    }
}
