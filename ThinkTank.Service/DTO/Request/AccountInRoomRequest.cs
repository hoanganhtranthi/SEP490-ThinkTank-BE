

namespace ThinkTank.Application.DTO.Request
{
    public class AccountInRoomRequest
    {
        public bool? IsAdmin { get; set; }
        public int? AccountId { get; set; }
        public int? RoomId { get; set; }
    }
}
