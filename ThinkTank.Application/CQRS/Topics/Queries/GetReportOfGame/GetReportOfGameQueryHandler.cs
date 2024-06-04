
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.CQRS.Topics.Queries.GetReportOGame;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Topics.Queries.GetReportOfGame
{
    public class GetReportOfGameQueryHandler : IQueryHandler<GetReportOfGameQuery, GameReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime date;
        public GetReportOfGameQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
        }

        public async Task<GameReportResponse> Handle(GetReportOfGameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var currentDateMonth = date.Month;

                var totalSinglePlayer = _unitOfWork.Repository<Achievement>()
                    .GetAll().AsNoTracking()
                    .Where(x => x.CompletedTime.Month == currentDateMonth)
                    .Select(x => x.AccountId)
                    .Distinct()
                    .Count();

                var totalMultiplayerMode = _unitOfWork.Repository<AccountInRoom>()
                    .GetAll().AsNoTracking()
                    .Where(x => x.CompletedTime.Value.Month == currentDateMonth)
                    .Select(x => x.AccountId)
                    .Distinct()
                    .Count();


                var total1vs1Mode = _unitOfWork.Repository<AccountIn1vs1>()
                    .GetAll().AsNoTracking().Count();


                var totalContest = _unitOfWork.Repository<Contest>()
                    .GetAll().AsNoTracking()
                    .Where(x => x.StartTime.Month == currentDateMonth)
                    .Count();

                var totalRoom = _unitOfWork.Repository<Room>()
                    .GetAll().Include(x => x.AccountInRooms).AsNoTracking()
                    .Where(x => x.StartTime.Value.Month == currentDateMonth)
                    .Count();

                var totalUser = _unitOfWork.Repository<Account>()
                    .GetAll().AsNoTracking()
                    .Count();

                var totalNewbieUser = _unitOfWork.Repository<Account>()
                    .GetAll().AsNoTracking()
                    .Where(x => x.RegistrationDate.Value.Month == currentDateMonth)
                    .Count();

                var total = total1vs1Mode + totalMultiplayerMode + totalSinglePlayer;
                var percent1vs1Mode = total1vs1Mode != 0 ? (double)total1vs1Mode / total * 100 : 0.0;
                var percentMultiplayerMode = totalMultiplayerMode != 0 ? (double)totalMultiplayerMode / total * 100 : 0.0;
                var percentSinglePlayer = totalSinglePlayer != 0 ? (double)totalSinglePlayer / total * 100 : 0.0;

                return new GameReportResponse
                {
                    TotalSinglePlayerMode = percentSinglePlayer,
                    Total1vs1Mode = percent1vs1Mode,
                    TotalMultiplayerMode = percentMultiplayerMode,
                    TotalContest = totalContest,
                    TotalRoom = totalRoom,
                    TotalUser = totalUser,
                    TotalNewbieUser = totalNewbieUser,
                };

            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Report Game  Error", ex.InnerException?.Message);
            }
        }
    }
}
