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
        public AccountInRoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AccountInRoomResponse> UpdateAccountInRoom(CreateAndUpdateAccountInRoomRequest createAccountInRoomRequest)
        {
            try
            {
                var accInRoom = _mapper.Map<CreateAndUpdateAccountInRoomRequest, AccountInRoom>(createAccountInRoomRequest);
                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == createAccountInRoomRequest.AccountId);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {createAccountInRoomRequest.AccountId} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {createAccountInRoomRequest.AccountId} Not Available!!!!!", "");
                }                
                var room = _unitOfWork.Repository<Room>().GetAll().Include(c=>c.AccountInRooms).SingleOrDefault(c => c.Id == createAccountInRoomRequest.RoomId);
                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Room Not Found!!!!!", "");
                }
                if (room.AccountInRooms.SingleOrDefault(x => x.AccountId == createAccountInRoomRequest.AccountId) == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"This account id {createAccountInRoomRequest.AccountId} is not in the room id {createAccountInRoomRequest.RoomId} !!!", "");
                accInRoom.CompletedTime = DateTime.Now;
                await _unitOfWork.Repository<AccountInRoom>().Update(accInRoom, createAccountInRoomRequest.AccountId);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountInRoomResponse>(accInRoom);
                rs.Username = a.UserName;
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
                var response = _unitOfWork.Repository<AccountInRoom>().GetAll().Include(x => x.Account)
                    .SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account in room with id {id.ToString()}", "");
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
                var accountInRooms = _unitOfWork.Repository<AccountInRoom>().GetAll().Include(x => x.Account)
                    .ProjectTo<AccountInRoomResponse>(_mapper.ConfigurationProvider)
                    .DynamicFilter(filter).ToList();
                foreach (var account1 in accountInRooms)
                    account1.Username = _unitOfWork.Repository<Account>().Find(x => x.Id == account1.AccountId).UserName;
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
