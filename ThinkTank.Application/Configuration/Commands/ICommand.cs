using MediatR;

namespace ThinkTank.Application.Configuration.Commands
{
    public interface ICommand : IRequest
    {
    }

    public interface ICommand<TResponse> : IRequest<TResponse>
    {
    }
}
