
using System.ComponentModel.DataAnnotations;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class TopicResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Name { get; set; }
        [IntAttribute]
        public int? GameId { get; set; }
        public virtual ICollection<AssetResponse> Assets { get; set; }
    }
}
