using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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

namespace ThinkTank.Service.Services.ImpService
{
    public class FriendService : IFriendService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FriendService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FriendResponse> CreateFriend(CreateFriendRequest createFriendRequest)
        {
            try
            {
                if(createFriendRequest.AccountId1==createFriendRequest.AccountId2)
                    throw new CrudException(HttpStatusCode.BadRequest, "Add friend Invalid !!!", "");

                var friend = _mapper.Map<CreateFriendRequest, Friend>(createFriendRequest);
                var s = _unitOfWork.Repository<Account>().Find(s => s.Id == createFriendRequest.AccountId1);
                if (s == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {createFriendRequest.AccountId1} is not found !!!", "");
                }
                var cus = _unitOfWork.Repository<Account>().Find(s => s.Id == createFriendRequest.AccountId2);
                if (cus == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {createFriendRequest.AccountId2} is not found !!!", "");
                }
                var acc = _unitOfWork.Repository<Friend>().Find(x => x.AccountId1 == createFriendRequest.AccountId1 && x.AccountId2==createFriendRequest.AccountId2
                || x.AccountId1==createFriendRequest.AccountId2 && x.AccountId2==createFriendRequest.AccountId1);
                if (acc != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This friendship has already !!!", "");

                friend.Status = false;
                await _unitOfWork.Repository<Friend>().CreateAsync(friend);
                await _unitOfWork.CommitAsync();
                var rs= _mapper.Map<FriendResponse>(friend);
                rs.UserName1 = s.UserName;
                rs.UserName2 = cus.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Friend Error!!!", ex?.Message);
            }
        }

        public async Task<FriendResponse> DeleteFriendship(int id)
        {
            try
            {
                Friend friend = _unitOfWork.Repository<Friend>().GetAll().Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (friend == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id{id.ToString()}", "");
                }
                _unitOfWork.Repository<Friend>().Delete(friend);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<FriendResponse>(friend);
                rs.UserName1 = friend.AccountId1Navigation.UserName;
                rs.UserName2 = friend.AccountId2Navigation.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete friendship error!!!!!", ex.Message);
            }
        }

        public async Task<FriendResponse> GetFriendById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Friendship Invalid", "");
                }
                var response =  _unitOfWork.Repository<Friend>().GetAll().Include(x=>x.AccountId1Navigation).Include(x=>x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id {id.ToString()}", "");
                }

                var rs= _mapper.Map<FriendResponse>(response);
                rs.UserName1 = response.AccountId1Navigation.UserName;
                rs.UserName2 = response.AccountId2Navigation.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Friendship By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public  async Task<PagedResults<FriendResponse>> GetFriends(FriendRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<FriendResponse>(request);
                var friends = _unitOfWork.Repository<Friend>().GetAll().Include(a=>a.AccountId1Navigation)
                    .Include(a=>a.AccountId2Navigation).Select(x=>new FriendResponse
                {
                    Id = x.Id,
                    AccountId1=x.AccountId1,
                    AccountId2=x.AccountId2,
                    Status=x.Status,
                    UserName1=x.AccountId1Navigation.UserName,
                    UserName2=x.AccountId2Navigation.UserName,
                }) .DynamicFilter(filter).ToList();
                var sort = PageHelper<FriendResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<FriendResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get friendship list error!!!!!", ex.Message);
            }
        }

        public async Task<FriendResponse> GetToUpdateStatus(int id)
        {
            try
            {
                Friend friend = _unitOfWork.Repository<Friend>().GetAll().Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (friend == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id{id.ToString()}", "");
                }
                friend.Status = true;
                await _unitOfWork.Repository<Friend>().Update(friend, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<FriendResponse>(friend);
                rs.UserName1 = friend.AccountId1Navigation.UserName;
                rs.UserName2 = friend.AccountId2Navigation.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Accept friend error!!!!!", ex.Message);
            }
        }
    }
}
