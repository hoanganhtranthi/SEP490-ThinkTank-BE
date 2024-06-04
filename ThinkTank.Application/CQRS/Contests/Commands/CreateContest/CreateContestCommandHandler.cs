
using AutoMapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using FirebaseAdmin.Messaging;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.CQRS.Contests.Commands.CreateContest
{
    public class CreateContestCommandHandler : ICommandHandler<CreateContestCommand, ContestResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService;
        private readonly INotificationService _notificationService;
        private readonly IBadgesService _badgesService;
        public CreateContestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService,IBadgesService badgesService
            ,  IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _notificationService = notificationService;
            _badgesService = badgesService;
            this.firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<ContestResponse> Handle(CreateContestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateAndUpdateContest.Name == null || request.CreateAndUpdateContest.Name == "" || request.CreateAndUpdateContest.Thumbnail == null || request.CreateAndUpdateContest.Thumbnail == ""
                    || request.CreateAndUpdateContest.StartTime == null || request.CreateAndUpdateContest.EndTime == null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var contest = _mapper.Map<CreateAndUpdateContestRequest, Contest>(request.CreateAndUpdateContest);

                if (request.CreateAndUpdateContest.Assets == null || request.CreateAndUpdateContest.Assets.Count() == 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Assets Of Contest cannot be null", "");

                var s = _unitOfWork.Repository<Contest>().Find(s => s.Name == request.CreateAndUpdateContest.Name);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Contest has already !!!", "");
                }

                var game = _unitOfWork.Repository<Game>().Find(s => s.Id == request.CreateAndUpdateContest.GameId);
                if (game == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Game {request.CreateAndUpdateContest.GameId} not found !!!", "");
                }

                if (contest.StartTime > contest.EndTime || request.CreateAndUpdateContest.StartTime < date || request.CreateAndUpdateContest.EndTime < date)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Start Time or End Time is invalid", "");
                }

                if (game.Name.Equals("Flip Card"))
                {
                    var allowedAssetCounts = new HashSet<int> { 3, 4, 6, 8, 10, 12, 14 };
                    int assetCount = request.CreateAndUpdateContest.Assets.Count();

                    if (!allowedAssetCounts.Contains(assetCount))
                    {
                        throw new CrudException(HttpStatusCode.NotFound, "The number of assets for the flip card game must be 3, 4, 6, 8, 10, 12, or 14", "");
                    }
                }
            
                contest.Name = request.CreateAndUpdateContest.Name;
                contest.Thumbnail = request.CreateAndUpdateContest.Thumbnail;
                contest.StartTime = request.CreateAndUpdateContest.StartTime;
                contest.EndTime = request.CreateAndUpdateContest.EndTime;
                contest.CoinBetting = request.CreateAndUpdateContest.CoinBetting;
                contest.GameId = request.CreateAndUpdateContest.GameId;
                contest.Status = null;
                List<AssetOfContest> list = new List<AssetOfContest>();


                foreach (var type in request.CreateAndUpdateContest.Assets)
                {
                    if (type.Value == null || type.Value == "" || type.TypeOfAssetId <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var asset = _mapper.Map<CreateAssetOfContestRequest, AssetOfContest>(type);

                    var t = _unitOfWork.Repository<TypeOfAssetInContest>().Find(t => t.Id == type.TypeOfAssetId);
                    if (t == null)
                    {
                        throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset In Contest Not Found!!!!!", "");
                    }

                    if (game.Name.Equals("Flip Card") || game.Name.Equals("Images Walkthrough"))
                    {
                        if (t.Type.Equals("Description+ImgLink") || t.Type.Equals("AudioLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset In Contest Invalid!!!!!", "");
                        }
                        else
                        {
                            if (asset.Value.Contains(";") || asset.Value.Contains(".mp3"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset In Contest Invalid!!!!!", "");
                            }
                        }
                    }
                    else if (game.Name.Equals("Music Password"))
                    {
                        if (t.Type.Equals("Description+ImgLink") || t.Type.Equals("ImgLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset In Contest Invalid!!!!!", "");
                        }
                        else
                        {
                            if (!asset.Value.Contains(".mp3"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset In Contest Invalid!!!!!", "");
                            }
                        }
                    }
                    else
                    {
                        if (t.Type.Equals("ImgLink") || t.Type.Equals("AudioLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset In Contest Invalid!!!!!", "");
                        }
                        else
                        {
                            if (!asset.Value.Contains(";"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset In Contest Invalid!!!!!", "");
                            }
                        }
                    }
                    AssetOfContest assetOfContest = new AssetOfContest();
                    assetOfContest.Value = type.Value;
                    assetOfContest.TypeOfAssetId = type.TypeOfAssetId;
                    assetOfContest.ContestId = contest.Id;
                    assetOfContest.Contest = contest;
                    list.Add(assetOfContest);
                    contest.AssetOfContests = list;
                }

                await _unitOfWork.Repository<Contest>().CreateAsync(contest);
                await _unitOfWork.CommitAsync();

                #region set background service 
                var startId = BackgroundJob.Schedule(() =>
                 SendNotificationStartContest(contest.Id, $"\"{contest.Name}\"” is opened. Join now. Please exit the app and re-enter to participate in the contest."),
                 contest.StartTime.Subtract(date));

                await firebaseRealtimeDatabaseService.SetAsync<string>($"Contest{contest.Id}JobIdStartTime", startId);

                var endId = BackgroundJob.Schedule(() =>
                  UpdateStateContest(contest.Id),
                         contest.EndTime.Subtract(date));
                await firebaseRealtimeDatabaseService.SetAsync<string>($"Contest{contest.Id}JobIdEndTime", endId);
                #endregion

                return _mapper.Map<ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Contest Error!!!", ex?.Message);
            }
        }
        public async Task<ContestResponse> UpdateStateContest(int id)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>().GetAll().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == id);

                contest.Status = false;

                var jobEndId = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdEndTime").Result;
                if (jobEndId != null)
                {
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdEndTime");
                }

                var jobStartId = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdStartTime").Result;
                if (jobStartId != null)
                {
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdStartTime");
                }

                await _unitOfWork.Repository<Contest>().Update(contest, id);

                #region send noti for all accounts take part in contest
                var accounts = _unitOfWork.Repository<AccountInContest>().GetAll().Include(a => a.Account).Where(a => a.ContestId == contest.Id && a.Account.Status == true).ToList();
               
                var fcmTokens = accounts.Where(x => x.Account.Fcm != null && x.Account.Fcm != "").Select(x => x.Account.Fcm).ToList();
                
                await _notificationService.SendNotification(fcmTokens, $"\"{contest.Name}\"” is closed.Thank you for participating in the contest.", "ThinkTank Contest", contest.Thumbnail, accounts.Select(x => x.AccountId).ToList());
                #endregion

                #region send notification top 3
                var leaderboard = await GetLeaderboardOfContest(id);

                foreach (var contestant in leaderboard.Take(3))
                {
                    var account = await _unitOfWork.Repository<Account>().FindAsync(x => x.Id == contestant.AccountId);

                    int rewardPercentage = 0;
                    switch (contestant.Rank)
                    {
                        case 1:
                            rewardPercentage = 50;
                            break;
                        case 2:
                            rewardPercentage = 30;
                            break;
                        case 3:
                            rewardPercentage = 20;
                            break;
                    }
                    int reward = (int)Math.Round((decimal)(contest.CoinBetting * contest.AccountInContests.Count * (rewardPercentage / 100.0m)));

                    account.Coin = account.Coin + reward;
                    await _unitOfWork.Repository<Account>().Update(account, account.Id);
                    fcmTokens.Clear();
                    if (account.Status == true)
                    {
                        if (account.Fcm != null)
                            fcmTokens.Add(account.Fcm);
                        await _notificationService.SendNotification(fcmTokens, $"Congratulations! You won top {contestant.Rank} in the contest \"{contest.Name}\" and received {reward} ThinkTank coins”"
                            , "ThinkTank Contest", contest.Thumbnail, account.Id);

                        await _badgesService.GetBadge(account, "The Tycoon");
                    }
                    #endregion
                }

                await _unitOfWork.CommitAsync();
                return _mapper.Map<Contest, ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update status's contest error!!!!!", ex.Message);
            }
        }
        private async Task<List<LeaderboardResponse>> GetLeaderboardOfContest(int contestId)
        {
                var contest = _unitOfWork.Repository<Contest>().GetAll().AsNoTracking().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == contestId);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{contestId.ToString()}", "");
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
                return responses.ToList();
            }

        public async Task SendNotificationStartContest(int id, string message)
        {
            try
            {
                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == id);
                var accounts = _unitOfWork.Repository<Account>().GetAll().Where(a => a.Status == true).ToList();
                var fcmTokens = accounts.Where(a => a.Fcm != null && a.Fcm != "").Select(a => a.Fcm).ToList();
                await _notificationService.SendNotification(fcmTokens, message, "ThinkTank Contest", contest.Thumbnail, accounts.Select(x => x.Id).ToList());
                contest.Status = true;
                await _unitOfWork.Repository<Contest>().Update(contest, contest.Id);
                await _unitOfWork.CommitAsync();
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Send Notification To Start Contest Error!!!!!", ex.Message);
            }
        }
    }
    }
