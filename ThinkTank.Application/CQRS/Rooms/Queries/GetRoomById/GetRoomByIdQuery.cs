
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQuery : IGetTByIdQuery<RoomResponse>
    {
        public GetRoomByIdQuery(int id) : base(id)
        {
        }
    }
}
