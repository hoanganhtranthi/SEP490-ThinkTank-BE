

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Rooms.Queries.GetLeaderboardOfRoom
{
    public class GetLeaderboardOfRoomQueryHandler : IQueryHandler<GetLeaderboardOfRoomQuery, List<LeaderboardResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetLeaderboardOfRoomQueryHandler(IUnitOfWork unitOfWork, ISlackService slacService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slacService;
        }

        public async Task<List<LeaderboardResponse>> Handle(GetLeaderboardOfRoomQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var room = _unitOfWork.Repository<Room>().GetAll().AsNoTracking().Include(c => c.AccountInRooms)
                      .SingleOrDefault(c => c.Code == request.RoomCode);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with code {request.RoomCode}", "");
                }

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                if (room.AccountInRooms.Count() > 0)
                {
                    var orderedAccounts = room.AccountInRooms.OrderByDescending(x => x.Mark);
                    var rank = 1;

                    foreach (var account in orderedAccounts)
                    {
                        var acc = _unitOfWork.Repository<Account>().Find(x => x.Id == account.AccountId);
                        var leaderboardContestResponse = new LeaderboardResponse
                        {
                            AccountId = account.AccountId,
                            Mark = account.Mark,
                            Avatar = acc.Avatar,
                            FullName = acc.FullName
                        };

                        var mark = room.AccountInRooms
                            .Where(x => x.Mark == account.Mark && x.AccountId != account.AccountId)
                            .ToList();

                        if (mark.Any())
                        {
                            var a = responses.SingleOrDefault(a => a.AccountId == mark.First().AccountId);
                            leaderboardContestResponse.Rank = a?.Rank ?? rank;// a != null: leaderboardContestResponse.Rank = a.Rank va nguoc lai a==null : leaderboardContestResponse.Rank = rank
                        }
                        else
                        {
                            leaderboardContestResponse.Rank = rank;
                        }

                        responses.Add(leaderboardContestResponse);
                        rank++;
                    }

                }
                return responses.ToList();
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get leaderboard of room error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of room error!!!!!", ex.Message);
            }
        }
    }
}
