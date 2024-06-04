using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Friends.Queries.GetFriendById
{
    public class GetFriendByIdQuery : IGetTByIdQuery<FriendResponse>
    {
        public GetFriendByIdQuery(int id) : base(id)
        {
        }
    }
}
