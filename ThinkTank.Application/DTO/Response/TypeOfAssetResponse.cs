
using System.ComponentModel.DataAnnotations;
using ThinkTank.Domain.Commons;

namespace ThinkTank.Application.DTO.Response
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
