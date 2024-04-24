

using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateTopicRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int GameId { get; set; }
    }
}
