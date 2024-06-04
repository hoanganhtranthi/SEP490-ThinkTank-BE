

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Rooms.Queries.GetRooms
{
    public class GetRoomsQuery:IGetTsQuery<PagedResults<RoomResponse>>
    {
        public RoomRequest RoomRequest { get; }
        public GetRoomsQuery(PagingRequest pagingRequest, RoomRequest roomRequest) : base(pagingRequest)
        {
            RoomRequest = roomRequest;
        }

    }
}
