using MediatR;


namespace ThinkTank.Application.Configuration.Queries
{
    public interface IQueryHandler<in TQuery, TResult> :
    IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {

    }
}
