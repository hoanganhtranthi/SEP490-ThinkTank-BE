using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateTypeOfAssetRequest
    {
        public string Type { get; set; } = null!;
        public virtual ICollection<CreateAssetRequest> Assets { get; set; }
    }
}
