


using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetLeaderboardOfContest
{
    public class GetLeaderboardOfContestQueryHandler : IQueryHandler<GetLeaderboardOfContestQuery, PagedResults<LeaderboardResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetLeaderboardOfContestQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResults<LeaderboardResponse>> Handle(GetLeaderboardOfContestQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var contest = _unitOfWork.Repository<Contest>().GetAll().AsNoTracking().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == request.Id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{request.Id.ToString()}", "");
                }

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                if (contest.AccountInContests.Count() > 0)
                {
                    var orderedAccounts = contest.AccountInContests.Where(x => x.Mark != 0).OrderByDescending(x => x.Mark);
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

                        var mark = contest.AccountInContests
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
                return PageHelper<LeaderboardResponse>.Paging(responses.ToList(),request.PagingRequest.Page, request.PagingRequest.PageSize);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of contest error!!!!!", ex.Message);
            }
        }
    }
}
