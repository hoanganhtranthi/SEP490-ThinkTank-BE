

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQueryHandler : IQueryHandler<GetRoomByIdQuery, RoomResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetRoomByIdQueryHandler(IUnitOfWork unitOfWork, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<RoomResponse> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _unitOfWork.Repository<Room>().GetAll().AsNoTracking().Include(x => x.Topic).Include(x => x.Topic.Game).Select(x => new RoomResponse
                {
                    Id = x.Id,
                    TopicId = x.TopicId,
                    TopicName = x.Topic.Name,
                    Status = x.Status,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    AmountPlayer = x.AmountPlayer,
                    Name = x.Name,
                    Code = x.Code,
                    GameName = x.Topic.Game.Name,
                    AccountInRoomResponses = new List<AccountInRoomResponse>(x.AccountInRooms.Select(a => new AccountInRoomResponse
                    {
                        Id = a.Id,
                        AccountId = a.AccountId,
                        CompletedTime = a.CompletedTime,
                        Username = a.Account.UserName,
                        Duration = a.Duration,
                        IsAdmin = a.IsAdmin,
                        Avatar = a.Account.Avatar,
                        Mark = a.Mark,
                        PieceOfInformation = a.PieceOfInformation
                    }))
                }).SingleOrDefault(x => x.Id == request.Id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with id {request.Id.ToString()}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get room by id Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get room by id Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
