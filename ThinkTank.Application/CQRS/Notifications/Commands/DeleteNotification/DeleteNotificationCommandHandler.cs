

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Notifications.Commands.DeleteNotification
{
    public class DeleteNotificationCommandHandler : ICommandHandler<DeleteNotificationCommand, List<NotificationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DeleteNotificationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<NotificationResponse>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new List<NotificationResponse>();
                foreach (var id in request.Ids)
                {
                    if (id <= 0)
                    {
                        throw new CrudException(HttpStatusCode.BadRequest, "Id Notification Invalid", "");
                    }
                    var notification = _unitOfWork.Repository<Notification>().GetAll()
                        .Include(x => x.Account).FirstOrDefault(u => u.Id == id);

                    if (notification == null)
                    {
                        throw new CrudException(HttpStatusCode.NotFound, $"Not found notification with id{id}", "");
                    }

                    _unitOfWork.Repository<Notification>().Delete(notification);
                    await _unitOfWork.CommitAsync();

                    var rs = _mapper.Map<NotificationResponse>(notification);
                    rs.Username = notification.Account.UserName;
                    result.Add(rs);
                }
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete list notifications error!!!!!", ex.Message);
            }
        }
    }
}
