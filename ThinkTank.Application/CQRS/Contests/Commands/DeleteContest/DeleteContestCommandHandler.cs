

using AutoMapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Contests.Commands.DeleteContest
{
    public class DeleteContestCommandHandler : ICommandHandler<DeleteContestCommand, ContestResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly ISlackService _slackService;
        private readonly IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService;
        public DeleteContestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            this.firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
            _slackService = slackService;
        }

        public async Task<ContestResponse> Handle(DeleteContestCommand request, CancellationToken cancellationToken)
        {
            try
            {

                Contest contest = _unitOfWork.Repository<Contest>()
                      .GetAll().Include(x => x.AssetOfContests).SingleOrDefault(c => c.Id == request.Id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{request.Id.ToString()}", "");
                }

                if (contest.StartTime <= date)
                    throw new CrudException(HttpStatusCode.BadRequest, "The contest has already started and you cannot delete it", "");

                await _unitOfWork.Repository<AssetOfContest>().DeleteRange(contest.AssetOfContests.ToArray());

                await _unitOfWork.Repository<Contest>().RemoveAsync(contest);
                await _unitOfWork.CommitAsync();

                var startJob = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdStartTime").Result;
                if (startJob != null)
                {
                    BackgroundJob.Delete(startJob);
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdStartTime");
                }

                var endJob = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdEndTime").Result;
                if (endJob != null)
                {
                    BackgroundJob.Delete(endJob);
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdEndTime");
                }

                return _mapper.Map<ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Delete contest error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete contest error!!!!!", ex.Message);
            }
        }
    }
}
