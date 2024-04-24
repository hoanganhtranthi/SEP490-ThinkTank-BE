
using System.ComponentModel.DataAnnotations;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class GameResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Name { get; set; }
        public int? AmoutPlayer { get; set; }
        public virtual ICollection<TopicResponse> Topics { get; set; }
    }
}