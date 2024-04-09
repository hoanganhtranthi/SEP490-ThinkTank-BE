using AutoMapper;
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
using ThinkTank.Service.Services.IService;
using Microsoft.EntityFrameworkCore;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Utilities;
using AutoMapper.QueryableExtensions;

namespace ThinkTank.Service.Services.ImpService
{
    public class AccountInRoomService : IAccountInRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        public AccountInRoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AccountInRoomResponse> UpdateAccountInRoom(int accountInRoomId,CreateAndUpdateAccountInRoomRequest createAccountInRoomRequest)
        {
            try
            {
                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == createAccountInRoomRequest.AccountId);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {createAccountInRoomRequest.AccountId} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {createAccountInRoomRequest.AccountId} Not Available!!!!!", "");
                }                
                var accountInRoom = _unitOfWork.Repository<AccountInRoom>().Find(x => x.Id == accountInRoomId);
                if (accountInRoom == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This account in room Id {createAccountInRoomRequest.AccountId} is not found !!!", "");
                _mapper.Map<CreateAndUpdateAccountInRoomRequest, AccountInRoom>(createAccountInRoomRequest,accountInRoom);
                accountInRoom.CompletedTime = date;
                await _unitOfWork.Repository<AccountInRoom>().Update(accountInRoom,accountInRoomId);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountInRoomResponse>(accountInRoom);
                rs.Username = a.UserName;
                rs.Avatar = a.Avatar;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update Account In Room Error!!!", ex?.Message);
            }
        }

        public async Task<AccountInRoomResponse> GetAccountInRoomById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Account In Room Invalid", "");
                }
                var response = _unitOfWork.Repository<AccountInRoom>().GetAll().AsNoTracking().Include(x => x.Account)
                    .SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account in room with id {id}", "");
                }
                var rs = _mapper.Map<AccountInRoomResponse>(response);
                rs.Username = response.Account.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Account In Room By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AccountInRoomResponse>> GetAccountInRooms(AccountInRoomRequest accountInRoomRequest, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<AccountInRoomResponse>(accountInRoomRequest);
                var accountInRooms = _unitOfWork.Repository<AccountInRoom>().GetAll().AsNoTracking().Include(x => x.Account)
                    .ProjectTo<AccountInRoomResponse>(_mapper.ConfigurationProvider)
                    .DynamicFilter(filter).ToList();
                foreach (var account1 in accountInRooms)
                {
                    account1.Username = _unitOfWork.Repository<Account>().Find(x => x.Id == account1.AccountId).UserName;
                    account1.Avatar = _unitOfWork.Repository<Account>().Find(x => x.Id == account1.AccountId).Avatar;
                }
                var sort = PageHelper<AccountInRoomResponse>.Sorting(paging.SortType, accountInRooms, paging.ColName);
                var result = PageHelper<AccountInRoomResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get accounts in room list error!!!!!", ex.Message);
            }
        }
    }
}
