
using System.ComponentModel.DataAnnotations;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class NotificationResponse
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; } 
        public string? Description { get; set; } 
        public DateTime? DateNotification { get; set; }
        public string? Avatar { get; set; }
        [BooleanAttribute]
        public bool? Status { get; set; }
        [IntAttribute]
        public int? AccountId { get; set; }
        public string? Username { get; set; }
    }
}
