

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Notifications.Commands.UpdateStatusNotification
{
    public class UpdateStatusNotificationCommandHandler : ICommandHandler<UpdateStatusNotificationCommand, NotificationResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;
        public UpdateStatusNotificationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _slackService = slackService;
        }

        public async Task<NotificationResponse> Handle(UpdateStatusNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var notification = _unitOfWork.Repository<Notification>().GetAll()
                    .Include(x => x.Account).FirstOrDefault(u => u.Id == request.Id);

                if (notification == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found notification with id{request.Id}", "");
                }

                notification.Status = true;
                await _unitOfWork.Repository<Notification>().Update(notification, request.Id);
                await _unitOfWork.CommitAsync();

                var rs = _mapper.Map<NotificationResponse>(notification);
                rs.Username = notification.Account.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Update status notification error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Update status notification error!!!!!", ex.Message);
            }
        }
    }
}
