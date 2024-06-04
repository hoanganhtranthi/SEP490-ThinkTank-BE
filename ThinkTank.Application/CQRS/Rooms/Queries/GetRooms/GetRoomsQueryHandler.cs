

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

namespace ThinkTank.Application.CQRS.Rooms.Queries.GetRooms
{
    public class GetRoomsQueryHandler : IQueryHandler<GetRoomsQuery, PagedResults<RoomResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetRoomsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResults<RoomResponse>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var filter = _mapper.Map<RoomResponse>(request.RoomRequest);
                var response = _unitOfWork.Repository<Room>().GetAll()
                    .AsNoTracking().Include(x => x.Topic).Include(x => x.Topic.Game).Select(x => new RoomResponse
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
                            Duration = a.Duration,
                            Avatar = a.Account.Avatar,
                            Username = a.Account.UserName,
                            IsAdmin = a.IsAdmin,
                            Mark = a.Mark,
                            PieceOfInformation = a.PieceOfInformation
                        }))
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<RoomResponse>.Sorting(request.PagingRequest.SortType, response, request.PagingRequest.ColName);
                var result = PageHelper<RoomResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get rooms list error!!!!!", ex.Message);
            }
        }
    }
}
