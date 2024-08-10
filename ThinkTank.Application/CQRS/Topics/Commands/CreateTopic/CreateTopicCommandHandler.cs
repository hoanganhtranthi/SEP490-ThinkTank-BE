

using AutoMapper;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Topics.Commands.CreateTopic
{
    public class CreateTopicCommandHandler : ICommandHandler<CreateTopicCommand, TopicResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;
        public CreateTopicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _slackService = slackService;
        }
        
        public async Task<TopicResponse> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateTopicRequest.GameId <= 0 || request.CreateTopicRequest.Name == null || request.CreateTopicRequest.Name == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var topic = _mapper.Map<CreateTopicRequest, Topic>(request.CreateTopicRequest);

                var existingTopic = _unitOfWork.Repository<Topic>().Find(s => s.Name == request.CreateTopicRequest.Name && s.GameId == request.CreateTopicRequest.GameId);
                if (existingTopic != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $" Topic Name {request.CreateTopicRequest.Name} has already !!!", "");
                }
                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == request.CreateTopicRequest.GameId);
                if (game == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $" Game Id {request.CreateTopicRequest.GameId} is not found !!!", "");

                await _unitOfWork.Repository<Topic>().CreateAsync(topic);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<TopicResponse>(topic);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Create Topic Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Topic Error!!!", ex?.Message);
            }
        }
    }
}
