using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Services.IService;
using ThinkTank.Service.Utilities;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using Repository.Extensions;

namespace ThinkTank.Service.Services.ImpService
{
    public class ContestService : IContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        private readonly ICacheService _cacheService;
        public ContestService(IUnitOfWork unitOfWork, IMapper mapper,IFirebaseMessagingService firebaseMessagingService,ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
            _cacheService = cacheService;
        }

        public async Task<ContestResponse> CreateContest(CreateContestRequest createContestRequest)
        {
            try
            {
                //var contest = _mapper.Map<CreateContestRequest, Contest>(createContestRequest);
                var s = _unitOfWork.Repository<Contest>().Find(s => s.Name == createContestRequest.Name);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Contest has already !!!", "");
                }
                
                if (createContestRequest.StartTime > createContestRequest.EndTime || createContestRequest.StartTime < DateTime.Now || createContestRequest.EndTime < DateTime.Now)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Start Time or End Time is invalid", "");
                }

                Contest contest = new Contest();
                contest.Name = createContestRequest.Name;
                contest.Thumbnail = createContestRequest.Thumbnail;
                contest.StartTime = createContestRequest.StartTime;
                contest.EndTime = createContestRequest.EndTime;
                contest.CoinBetting = createContestRequest.CoinBetting;
                contest.Status = null;
<<<<<<< Updated upstream
                //create prize of contest
=======

                List<AssetOfContestResponse> result = new List<AssetOfContestResponse>();
                List<AssetOfContest> list = new List<AssetOfContest>();

                foreach (var type in createContestRequest.Assets)
                {
                    if(type.Value ==null || type.Value == "" || type.TypeOfAssetId <=0)
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

                    var existingAsset = _unitOfWork.Repository<AssetOfContest>().Find(s => s.Value.Equals(type.Value));
                    if (existingAsset != null)
                    {
                        throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");
                    }

                    AssetOfContest assetOfContest = new AssetOfContest();
                    assetOfContest.Value = type.Value;
                    assetOfContest.TypeOfAssetId = type.TypeOfAssetId;
                    assetOfContest.ContestId = contest.Id;
                    assetOfContest.Contest = contest;
                    AssetOfContestResponse response = new AssetOfContestResponse();
                    response.Id = assetOfContest.Id;
                    response.Value = assetOfContest.Value;
                    response.NameOfContest = contest.Name;
                    response.Type = t.Type;
                    list.Add(assetOfContest);
                    contest.AssetOfContests = list;
                    result.Add(response);
                }

>>>>>>> Stashed changes
                await _unitOfWork.Repository<Contest>().CreateAsync(contest);
                await _unitOfWork.CommitAsync();

                var t = _unitOfWork.Repository<TypeOfAssetInContest>().Find(t => t.Id == createContestRequest.TypeOfAssetId);
                if (t == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Type Of Asset In Contest Not Found!!!!!", "");
                }

                AssetOfContest assetOfContest = new AssetOfContest();
                assetOfContest.TypeOfAssetId = createContestRequest.TypeOfAssetId;
                assetOfContest.ContestId = contest.Id;
                assetOfContest.Value = createContestRequest.Value;

                await _unitOfWork.Repository<AssetOfContest>().CreateAsync(assetOfContest);
                await _unitOfWork.CommitAsync();

                var expiryTime = contest.EndTime;                
                  var id=  BackgroundJob.Schedule(() =>
                    SendNotification(contest),
                   contest.StartTime.Subtract(DateTime.Now));
                _cacheService.SetData<string>($"Contest{contest.Id}JobIdStartTime", id, expiryTime);
                 var endId=BackgroundJob.Schedule(() =>
                   UpdateStateContest(contest.Id),
                           contest.EndTime.Subtract(DateTime.Now));
                _cacheService.SetData<string>($"Contest{contest.Id}JobIdEndTime", endId, expiryTime);
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

        public async Task SendNotification(Contest contest)
        {
            try
            {
                #region send noti for account
                var accounts= _unitOfWork.Repository<Account>().GetAll().Where(a=>a.Status==true).ToList();
                var fcmTokens = accounts.Where(a => a.Fcm != null && a.Fcm != "") .Select(a => a.Fcm).ToList();
                var data = new Dictionary<string, string>()
                {
                    ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                    ["Action"] = "home",
                    ["Argument"] = JsonConvert.SerializeObject(new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        }
                    }),
                };
                if (fcmTokens.Any())
                    _firebaseMessagingService.SendToDevices(fcmTokens,
                                                           new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Contest", Body = $"\"{contest.Name}\"” is opened. Join now.", ImageUrl = $"{contest.Thumbnail}" }, data);
                #endregion
                contest.Status = true;
                await _unitOfWork.Repository<Contest>().Update(contest, contest.Id);
                foreach (var account in accounts)
                {
                    Notification notification = new Notification
                    {
                        AccountId = account.Id,
                        Avatar = contest.Thumbnail,
                        DateTime = DateTime.Now,
                        Description = $"\"{contest.Name}\"” is opened. Join now.",
                        Titile = "ThinkTank Contest"
                    };

                    await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                }
                await _unitOfWork.CommitAsync();
            }
            catch (CrudException ex)
            {
                throw ex;
            }
        }
        public async Task<ContestResponse> UpdateStateContest(int id)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>().GetAll().Include(c=>c.AccountInContests)
                      .SingleOrDefault(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }
                if(contest.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Contest {id.ToString()} has ended", "");
                contest.Status = false;

                await _unitOfWork.Repository<Contest>().Update(contest, id);
                #region send noti for account
                var accounts = _unitOfWork.Repository<AccountInContest>().GetAll().Include(a=>a.Account).Where(a => a.ContestId == contest.Id).ToList();
                var data = new Dictionary<string, string>()
                {
                    ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                    ["Action"] = "home",
                    ["Argument"] = JsonConvert.SerializeObject(new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        }
                    }),
                };
                _firebaseMessagingService.SendToTopic($"/topics/contestId{contest.Id}",
                                                       new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Contest", Body = $"\"{contest.Name}\"” is closed. You have received.", ImageUrl = $"{contest.Thumbnail}" }, data);
                if (accounts.Any())
                {
                    foreach (var account in accounts)
                    {
                        Notification notification = new Notification
                        {
                            AccountId = account.AccountId,
                            Avatar = contest.Thumbnail,
                            DateTime = DateTime.Now,
                            Description = $"\"{contest.Name}\"” is closed. You have received {account.Prize}.",
                            Titile = "ThinkTank Contest"
                        };

                        await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                        #endregion
                    }
                }
                var fcmTokens = accounts.Where(a => a.Account.Fcm != null && a.Account.Fcm != "").Select(a => a.Account.Fcm).ToList();
                if (fcmTokens.Any())
                    _firebaseMessagingService.Unsubcribe(fcmTokens, $"/topics/contestId{contest.Id}");
                await _unitOfWork.CommitAsync();
                if (contest.EndTime > DateTime.Now)
                {
                    var jobId = _cacheService.GetData<string>($"Contest{contest.Id}JobIdEndTime");
                    if (jobId != null)
                        BackgroundJob.Delete(jobId);
                }
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

        public async Task<ContestResponse> GetContestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Contest Invalid", "");
                }
                var response = _unitOfWork.Repository<Contest>().GetAll().Include(x => x.AssetOfContests).Select(x => new ContestResponse
                {
                    Name = x.Name,
                    Thumbnail = x.Thumbnail,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                                    
                    AssetOfContests = new List<AssetOfContestResponse>(x.AssetOfContests.Select(a => new AssetOfContestResponse
                    {
                        Id = a.Id,
                        Value = a.Value,
                        Type = _unitOfWork.Repository<TypeOfAssetInContest>().Find(t => t.Id == a.TypeOfAssetId).Type,
                    }))
                }).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id {id.ToString()}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Contest By ID Error!!!", ex.InnerException?.Message);
            }
        }
        public async Task<List<LeaderboardResponse>> GetLeaderboardOfContest(int id)
        {
            try
            {
                var contest = _unitOfWork.Repository<Contest>().GetAll().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                if (contest.AccountInContests.Count() > 0)
                {
                    var orderedAccounts = contest.AccountInContests.OrderByDescending(x => x.Mark).ThenBy(x => x.Duration);
                    var rank = 1;

                    foreach (var account in orderedAccounts)
                    {
                        var leaderboardContestResponse = new LeaderboardResponse
                        {
                            AccountId = account.AccountId,
                            Mark = account.Mark
                        };

                        var mark = contest.AccountInContests
                            .Where(x => x.Mark == account.Mark && x.Duration == account.Duration && x.AccountId != account.AccountId)
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of contest error!!!!!", ex.Message);
            }
        }

        public async Task<PagedResults<ContestResponse>> GetContests(ContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<ContestResponse>(request);
                var contests = _unitOfWork.Repository<Contest>().GetAll().Include(x=>x.AssetOfContests)
                                           .ProjectTo<ContestResponse>(_mapper.ConfigurationProvider)
                                           .DynamicFilter(filter)
                                           .ToList();
                if (request.ContestStatus != Helpers.Enum.StatusType.All)
                {
                    bool? status = null;
                    if (request.ContestStatus.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.ContestStatus.ToString().ToLower());
                    }
                    contests = contests.Where(a => a.Status == status).ToList();
                }
                var sort = PageHelper<ContestResponse>.Sorting(paging.SortType, contests, paging.ColName);
                var result = PageHelper<ContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get contest list error!!!!!", ex.Message);
            }
        }

        public async Task<PagedResults<ContestResponse>> GetContestsNotAsset(ContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<ContestResponse>(request);
                var contests = _unitOfWork.Repository<Contest>().GetAll()
                                           .ProjectTo<ContestResponse>(_mapper.ConfigurationProvider)
                                           .DynamicFilter(filter)
                                           .ToList();
                if (request.ContestStatus != Helpers.Enum.StatusType.All)
                {
                    bool? status = null;
                    if (request.ContestStatus.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.ContestStatus.ToString().ToLower());
                    }
                    contests = contests.Where(a => a.Status == status).ToList();
                }
                var sort = PageHelper<ContestResponse>.Sorting(paging.SortType, contests, paging.ColName);
                var result = PageHelper<ContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get contest list error!!!!!", ex.Message);
            }
        }

        public async Task<ContestResponse> GetContest(int id)
        {
            try
            {
                var contest = _unitOfWork.Repository<Contest>().GetAll().Include(x => x.AssetOfContests).SingleOrDefault(c => c.Id == id);
                return _mapper.Map<Contest, ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get list asset of contest error!!!!!", ex.Message);
            }
        }


        public async Task<ContestResponse> UpdateContest(int contestId, UpdateContestRequest request)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>()
                     .Find(c => c.Id == contestId);

                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id {contestId.ToString()}", "");

                var existingContest = _unitOfWork.Repository<Contest>().GetAll().FirstOrDefault(c => c.Name.Equals(request.Name) && c.Id != contestId);
                if (existingContest != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Name of contest has already been taken", "");
                if (request.StartTime > request.EndTime)
                    throw new CrudException(HttpStatusCode.BadRequest, "Start Time or End Time is invalid", "");
                if (contest.StartTime <= DateTime.Now)
                    throw new CrudException(HttpStatusCode.BadRequest, "The contest has already started and you cannot update it", "");
                
                var job = _cacheService.GetData<string>($"Contest{contest.Id}JobIdStartTime");
                if (job != null)
                    BackgroundJob.Delete(job);
                var jobId = _cacheService.GetData<string>($"Contest{contest.Id}JobIdEndTime");
                if (jobId != null)
                    BackgroundJob.Delete(jobId);
<<<<<<< Updated upstream
                _mapper.Map<UpdateContestRequest, Contest>(request, contest);
                contest.Id = contestId;               
=======
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdEndTime");
                }
              await  _unitOfWork.Repository<AssetOfContest>().DeleteRange(contest.AssetOfContests.ToArray());
                List<AssetOfContestResponse> result = new List<AssetOfContestResponse>();
                List<AssetOfContest> list = new List<AssetOfContest>();
                foreach (var type in request.Assets)
                {
                    if (type.Value == null || type.Value == "" || type.TypeOfAssetId <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var asset = _mapper.Map<CreateAssetOfContestRequest, AssetOfContest>(type);
                    var t = _unitOfWork.Repository<TypeOfAssetInContest>().Find(t => t.Id == type.TypeOfAssetId);
                    if (t == null)
                    {
                        throw new CrudException(HttpStatusCode.InternalServerError, "Type Of Asset In Contest Not Found!!!!!", "");
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

                    var existingAsset = _unitOfWork.Repository<AssetOfContest>().Find(s => s.Value.Equals(type.Value));
                    if (existingAsset != null)
                    {
                        throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");
                    }

                    AssetOfContest assetOfContest = new AssetOfContest();
                    assetOfContest.Value = type.Value;
                    assetOfContest.TypeOfAssetId = type.TypeOfAssetId;
                    assetOfContest.ContestId = contest.Id;
                    await _unitOfWork.Repository<AssetOfContest>().CreateAsync(assetOfContest);
                    AssetOfContestResponse response = new AssetOfContestResponse();
                    response.Id = assetOfContest.Id;
                    response.Value = assetOfContest.Value;
                    response.NameOfContest = contest.Name;
                    response.Type = t.Type;
                    list.Add(assetOfContest);
                    contest.AssetOfContests = list;
                    result.Add(response);
                }

                contest.Thumbnail = request.Thumbnail != null && request.Thumbnail != "" ? request.Thumbnail : contest.Thumbnail;
                _mapper.Map<CreateAndUpdateContestRequest, Contest>(request, contest);
                contest.Id = contestId;

>>>>>>> Stashed changes
                await _unitOfWork.Repository<Contest>().Update(contest, contestId);
                await _unitOfWork.CommitAsync();
                var expiryTime = contest.EndTime;
                var id = BackgroundJob.Schedule(() =>
                  SendNotification(contest),
                 contest.StartTime.Subtract(DateTime.Now));
                _cacheService.SetData<string>($"Contest{contest.Id}JobIdStartTime", id, expiryTime);
                BackgroundJob.Schedule(() =>
                   UpdateStateContest(contest.Id),
                           contest.EndTime.Subtract(DateTime.Now));
                _cacheService.SetData<string>($"Contest{contest.Id}JobIdEndTime", id, expiryTime);
                return _mapper.Map<Contest, ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update contest error!!!!!", ex.Message);
            }
        }

        public async Task<dynamic> DeleteContest(int id)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>()
                      .Find(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }
<<<<<<< Updated upstream

                var assets = _unitOfWork.Repository<AssetOfContest>().GetAll().Where(a => a.ContestId == id).ToList();
                foreach(var asset in assets)
                {
                    await _unitOfWork.Repository<AssetOfContest>().RemoveAsync(asset);
                }
                
=======
                if (contest.StartTime <= DateTime.Now)
                    throw new CrudException(HttpStatusCode.BadRequest, "The contest has already started and you cannot delete it", "");
               await _unitOfWork.Repository<AssetOfContest>().DeleteRange(contest.AssetOfContests.ToArray());
>>>>>>> Stashed changes
                await _unitOfWork.Repository<Contest>().RemoveAsync(contest);
                await _unitOfWork.CommitAsync();

                var job = _cacheService.GetData<string>($"Contest{contest.Id}JobIdStartTime");
                if (job != null)
                    BackgroundJob.Delete(job);
                var jobId = _cacheService.GetData<string>($"Contest{contest.Id}JobIdEndTime");
                if (jobId != null)
                    BackgroundJob.Delete(jobId);
                return new CrudException(HttpStatusCode.OK, "Delete contest success", "");
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete contest error!!!!!", ex.Message);
            }
        }
    }
}

/*public async Task<ContestResponse> GetContestByName(string name)
{
    try
    {
        if (name == null)
        {
            throw new CrudException(HttpStatusCode.BadRequest, "Name Contest Invalid", "");
        }
        var response = await _unitOfWork.Repository<Contest>().GetAsync(u => u.Name.Equals(name));

        if (response == null)
        {
            throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with name {name.ToString()}", "");
        }

        return _mapper.Map<ContestResponse>(response);
    }
    catch (CrudException ex)
    {
        throw ex;
    }
    catch (Exception ex)
    {
        throw new CrudException(HttpStatusCode.InternalServerError, "Get Contest By Name Error!!!", ex.InnerException?.Message);
    }
}
*/

