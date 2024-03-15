using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class TypeOfAssetResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Type { get; set; } = null!;

        public virtual ICollection<AssetResponse> Assets { get; set; }
    }
}
