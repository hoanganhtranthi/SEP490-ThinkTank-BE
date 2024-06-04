

using ThinkTank.Application.DTO.Request;

namespace ThinkTank.Application.Configuration.Queries
{
    public class IGetTsQuery<T>:IQuery<T>where T : class
    {
        public PagingRequest PagingRequest { get; }
        public IGetTsQuery(PagingRequest pagingRequest)
        {
            PagingRequest = pagingRequest;
        }
    }
}
