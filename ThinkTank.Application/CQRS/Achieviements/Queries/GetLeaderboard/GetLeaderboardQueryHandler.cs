
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Achieviements.Queries.GetLeaderboard
{
    public class GetLeaderboardQueryHandler : IQueryHandler<GetLeaderboardQuery, PagedResults<LeaderboardResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetLeaderboardQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResults<LeaderboardResponse>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == request.Id);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Game Id {request.Id} not found", "");

                var achievements = _unitOfWork.Repository<Achievement>().GetAll().Include(c => c.Account).Include(c => c.Game)
                    .Where(x => x.GameId == request.Id).ToList();

                List<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                List<Achievement> achievementsList = new List<Achievement>();
                if (achievements.Count() > 0)
                {
                    //Tính tổng tất cả điểm của các account
                    achievementsList = achievements
                   .GroupBy(achievement => achievement.AccountId)
                   .Select(group => GetSumScoreOfAccount(group.Key, achievements))
                   .Where(rs => rs != null)
                   .ToList();

                    var orderedAccounts = achievementsList.Where(x => x.Mark > 0).OrderByDescending(x => x.Mark);
                    var rank = 1;

                    foreach (var achievement in orderedAccounts)
                    {
                        //Nếu bảng xếp hạng chưa có account 
                        if (responses.Count(a => a.AccountId == achievement.AccountId) == 0)
                        {

                            var leaderboardContestResponse = new LeaderboardResponse
                            {
                                AccountId = achievement.AccountId,
                                Mark = achievement.Mark,
                                Avatar = achievement.Account.Avatar,
                                FullName = achievement.Account.FullName
                            };

                            //List những ai đang đồng hạng 
                            var mark = achievementsList
                                .Where(x => x.Mark == achievement.Mark && x.AccountId != achievement.AccountId)
                                .ToList();

                            if (mark.Any())
                            {
                                //Nếu có lấy người có cùng điểm đầu tiên trong bảng xếp hạng hiện tại
                                var a = responses.SingleOrDefault(a => a.AccountId == mark.First().AccountId);
                                //Gán rank người có cùng điểm đầu tiên trong bảng xếp hạng hiện tại bằng rank của account
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
                    if (request.AccountId != null)
                        responses = responses.Where(x => x.AccountId == request.AccountId).ToList();

                }
                return PageHelper<LeaderboardResponse>.Paging(responses.ToList(), request.PagingRequest.Page,request.PagingRequest.PageSize);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of achievement error!!!!!", ex.Message);
            }
        }
        private Achievement GetSumScoreOfAccount(int id, List<Achievement> achievements)
        {
            List<Achievement> responses = new List<Achievement>();
            Account account = null;
            foreach (var achievement in achievements)
            {
                if (responses.Count(a => a.Level == achievement.Level) == 0)
                {
                    var highestScore = achievements.Where(x => x.AccountId == id && x.Level == achievement.Level).OrderByDescending(x => x.Mark).FirstOrDefault();
                    if (highestScore != null)
                    {
                        responses.Add(highestScore);
                        account = highestScore.Account;
                    }
                }
            }
            return new Achievement
            {
                AccountId = id,
                Mark = responses.Sum(x => x.Mark),
                Account = account
            };
        }
    }
}
