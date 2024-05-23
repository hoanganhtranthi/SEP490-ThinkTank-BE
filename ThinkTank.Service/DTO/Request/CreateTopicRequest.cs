

using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Application.DTO.Request
{
    public class CreateTopicRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int GameId { get; set; }
    }
}
