

namespace ThinkTank.Application.DTO.Request
{
    public class RoomRequest
    {
        public string? Code { get; set; } = null!;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool? Status { get; set; }
        public int? TopicId { get; set; }
    }
}
