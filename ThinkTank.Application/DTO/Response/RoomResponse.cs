
using System.ComponentModel.DataAnnotations;
using ThinkTank.Domain.Commons;

namespace ThinkTank.Application.DTO.Response
{
    public class RoomResponse
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        [StringAttribute]
        public string? Code { get; set; } = null!;
        public int? AmountPlayer { get; set; }
        [DateRangeAttribute]
        public DateTime? StartTime { get; set; }
        [DateRangeAttribute]
        public DateTime? EndTime { get; set; }
        [BooleanAttribute]
        public bool? Status { get; set; }
        [IntAttribute]
        public int? TopicId { get; set; }
        public string? TopicName { get; set; }
        public string? GameName { get; set; }
        public List<AccountInRoomResponse> AccountInRoomResponses { get; set; }
    }
}
