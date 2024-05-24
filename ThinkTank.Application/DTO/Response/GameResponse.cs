
using System.ComponentModel.DataAnnotations;
using ThinkTank.Domain.Commons;

namespace ThinkTank.Application.DTO.Response
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