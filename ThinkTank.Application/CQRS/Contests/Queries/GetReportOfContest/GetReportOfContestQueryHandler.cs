
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetReportOfContest
{
    public class GetReportOfContestQueryHandler : IQueryHandler<GetReportOfContestQuery, ContestReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime date;
        public GetReportOfContestQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
        }

        public async Task<ContestReportResponse> Handle(GetReportOfContestQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var contests = _unitOfWork.Repository<Contest>()
                .GetAll()
                .Include(x => x.AccountInContests).Include(x => x.Game).AsNoTracking()
                .OrderByDescending(x => x.AccountInContests.Count())
                .ToList();

                var listBestContest = contests.Where(x => x.StartTime.Month == date.Month && x.AccountInContests.Count() == contests.FirstOrDefault().AccountInContests.Count()).ToList();

                var resultPercentAverageScore = new Dictionary<int, double>();
                foreach (var contest in listBestContest)
                {
                    if (contest != null)
                    {
                        var totalScore = _unitOfWork.Repository<AccountInContest>().GetAll().Where(x => x.ContestId == contest.Id).Sum(x => x.Mark);
                        var totalAccountInContest = _unitOfWork.Repository<AccountInContest>().GetAll().Where(x => x.ContestId == contest.Id).Count();
                        var percentAverageScore = totalScore > 0 && totalAccountInContest > 0 ? (double)(totalScore / totalAccountInContest) : 0;
                        resultPercentAverageScore.Add(contest.Id, percentAverageScore);
                    }
                }
                return new ContestReportResponse
                {
                    BestContestes = listBestContest.Select(x => new BestContestes
                    {
                        NameTopContest = x.Name,
                        PercentAverageScore = resultPercentAverageScore.SingleOrDefault(a => a.Key == x.Id).Value,
                    }).ToList(),
                    Contests = contests.Select(x => new ContestResponse
                    {
                        Id = x.Id,
                        EndTime = x.EndTime,
                        StartTime = x.StartTime,
                        Name = x.Name,
                        Status = x.Status,
                        Thumbnail = x.Thumbnail,
                        GameId = x.GameId,
                        PlayTime = x.PlayTime,
                        GameName = x.Game.Name,
                        CoinBetting=x.CoinBetting,
                        AmoutPlayer = x.AccountInContests.Count()
                    }).ToList(),
                };

            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Report Of Contest error!!!!!", ex.Message);
            }
        }
    }
}
