

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Icons.Queries.GetIcons
{
    public class GetIconsQuery:IGetTsQuery<PagedResults<IconResponse>>
    {
        public IconRequest IconRequest { get; }
        public GetIconsQuery(PagingRequest pagingRequest,IconRequest iconRequest) : base(pagingRequest)
        {
            IconRequest = iconRequest;
        }
        
    }
}
