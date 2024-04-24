
using System.ComponentModel.DataAnnotations;
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
