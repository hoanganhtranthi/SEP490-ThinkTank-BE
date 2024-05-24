

using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Application.DTO.Request
{
    public class CreateRoomRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int AmountPlayer { get; set; }
        [Required]
        public int TopicId { get; set; }
        [Required]
        public int AccountId { get; set; }
    }
}
