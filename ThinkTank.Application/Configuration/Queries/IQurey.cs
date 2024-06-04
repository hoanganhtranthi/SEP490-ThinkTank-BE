using MediatR;


namespace ThinkTank.Application.Configuration.Queries
{

    public interface IQuery<out TResult> : IRequest<TResult>
    {

    }
}
