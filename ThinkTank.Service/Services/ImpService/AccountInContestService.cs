using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using System.Security.Principal;

namespace ThinkTank.Service.Services.ImpService
{
    public class AccountInContestService : IAccountInContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        private readonly IContestService _contestService;
        public AccountInContestService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService,IContestService contestService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
            _contestService = contestService;
        }

        public async Task<AccountInContestResponse> CreateAccountInContest(CreateAccountInContestRequest request)
        {
            try
            {
                var acc = _mapper.Map<CreateAccountInContestRequest, AccountInContest>(request);
                var s = _unitOfWork.Repository<AccountInContest>().Find(s => s.ContestId == request.ContestId && s.AccountId == request.AccountId);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Account in Contest has already !!!", "");
                }

                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == request.AccountId);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Account Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Account Not Available!!!!!", "");
                }
                acc.AccountId = a.Id;

                var c = _unitOfWork.Repository<Contest>().Find(c => c.Id == request.ContestId);
                if (c == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Contest Not Found!!!!!", "");
                }
                if (c.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Contest Not Available!!!!!", "");
                }
                acc.ContestId = c.Id;
                acc.CompletedTime = DateTime.Now;
                acc.Duration = request.Duration;
                acc.Mark = request.Mark;
                await _unitOfWork.Repository<AccountInContest>().CreateAsync(acc);
                var leaderboardResult = await _contestService.GetLeaderboardOfContest(acc.ContestId);
                if (leaderboardResult.Any())
                {
                    var leaderboard = leaderboardResult.SingleOrDefault(a => a.AccountId == request.AccountId);
                    if (leaderboard != null)
                    {
                        var prize = _unitOfWork.Repository<PrizeOfContest>()
                            .GetAll()
                            .SingleOrDefault(x => x.ContestId == request.ContestId && x.FromRank <= leaderboard.Rank && x.ToRank >= leaderboard.Rank);
                        if (prize != null)
                            acc.Prize = prize.Prize;
                    }
                }
                List<string> fcmTokens = new List<string>();
                fcmTokens.Add(a.Fcm);
                if (fcmTokens.Any())
                    _firebaseMessagingService.Subcribe(fcmTokens, $"/topics/contestId{c.Id}");
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountInContestResponse>(acc);
                rs.ContestName = c.Name;
                rs.UserName = a.UserName;

                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Account In Contest Error!!!", ex?.Message);
            }
        }
        public async Task<AccountInContestResponse> GetAccountInContest(AccountInContestRequest account)
        {
            try
            {
                if (account == null || account.AccountId <= 0 || account.ContestId <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Account ID or Contest ID Invalid", "");
                }
                else
                {
                    var response = _unitOfWork.Repository<AccountInContest>().GetAll().Include(c => c.Account).Include(c => c.Contest).SingleOrDefault(x => x.AccountId == account.AccountId && x.ContestId == account.ContestId);
                    if (response == null)
                    {
                        throw new CrudException(HttpStatusCode.NotFound, $"Not found contest's result with account id {account.AccountId.ToString()} and contest id {account.ContestId}", "");
                    }
                    var rs = _mapper.Map<AccountInContestResponse>(response);
                    rs.ContestName = response.Contest.Name;
                    rs.UserName = response.Account.UserName;

                    return rs;
                }
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get result's contest of account error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AccountInContestResponse>> GetAccountInContests(AccountInContestRequest request, PagingRequest paging)
        {
            try
            {
                if (request == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Account or Contest Not Found!!!!!", "");
                }
                else
                {
                    var filter = _mapper.Map<AccountInContestResponse>(request);
                    var accountInContests = _unitOfWork.Repository<AccountInContest>().GetAll().Include(x => x.Account).Include(x => x.Contest)
                    .Select(x => new AccountInContestResponse
                    {
                        Id = x.Id,
                        UserName = x.Account.UserName,
                        ContestName = x.Contest.Name,
                        CompletedTime = x.CompletedTime,
                        Duration = x.Duration,
                        Mark = x.Mark,
                        Prize = x.Prize
                    }).DynamicFilter(filter).ToList();
                    var sort = PageHelper<AccountInContestResponse>.Sorting(paging.SortType, accountInContests, paging.ColName);
                    var result = PageHelper<AccountInContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                    return result;
                }
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Contest's result list error!!!!!", ex.Message);
            }
        }

        public async Task<AccountInContestResponse> UpdateAccountInContest(int accountInContestId, UpdateAccountInContestRequest request)
        {
            try
            {
                AccountInContest acc = _unitOfWork.Repository<AccountInContest>().GetAll().Include(c => c.Contest).Include(a => a.Account)
                     .SingleOrDefault(c => c.Id == accountInContestId);

                if (acc == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account in contest with id {accountInContestId.ToString()}", "");
                _mapper.Map<UpdateAccountInContestRequest, AccountInContest>(request, acc);

                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountInContestResponse>(acc);
                rs.ContestName = acc.Contest.Name;
                rs.UserName = acc.Account.UserName;

                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update Account in Contest error!!!!!", ex.Message);
            }
        }
    }
}
