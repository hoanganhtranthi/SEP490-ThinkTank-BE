

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, PagedResults<NotificationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetNotificationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResults<NotificationResponse>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var filter = _mapper.Map<NotificationResponse>(request.NotificationRequest);
                var notifications = _unitOfWork.Repository<Notification>().GetAll().AsNoTracking().Include(a => a.Account).Select(x => new NotificationResponse
                {
                    AccountId = x.AccountId,
                    Title = x.Title,
                    DateNotification = x.DateNotification,
                    Description = x.Description,
                    Id = x.Id,
                    Avatar = x.Avatar,
                    Status = x.Status,
                    Username = x.Account.UserName
                })
                    .DynamicFilter(filter).ToList();
                var sort = PageHelper<NotificationResponse>.Sorting(request.PagingRequest.SortType, notifications, request.PagingRequest.ColName);
                var result = PageHelper<NotificationResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get notification list error!!!!!", ex.Message);
            }
        }
    }
}
