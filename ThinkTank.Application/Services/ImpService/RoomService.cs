﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Services.ImpService
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); //chỉ cho phép một luồng truy cập vào critical section tại một thời điểm
        private readonly DateTime date;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        public RoomService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }
        public async Task<RoomResponse> CreateRoom(CreateRoomRequest createRoomRequest)
        {
            try
            {
                if (createRoomRequest.Name == null || createRoomRequest.Name == "" || createRoomRequest.AccountId <= 0 || createRoomRequest.TopicId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var room = _mapper.Map<CreateRoomRequest, Room>(createRoomRequest);
                
                var topic = _unitOfWork.Repository<Topic>().GetAll().AsNoTracking()
                    .Include(a=>a.Game).SingleOrDefault(a => a.Id == createRoomRequest.TopicId);

                if (topic == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Topic Id {createRoomRequest.TopicId} Not Found!!!!!", "");
                }
                
                
                if (room.AmountPlayer < 2 || room.AmountPlayer > 8)
                    throw new CrudException(HttpStatusCode.BadRequest, "Amout Player Is Invalid", "");
                
                room.StartTime = null;
                room.Code = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                AccountInRoom accountInRoom = new AccountInRoom();               
                
                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == createRoomRequest.AccountId);
                
                if(account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {createRoomRequest.AccountId} Not Found!!!!!", "");
                
                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {createRoomRequest.AccountId} Not Available!!!!!", "");
                }
                
                accountInRoom.AccountId = createRoomRequest.AccountId;
                accountInRoom.IsAdmin = true;
                accountInRoom.Room = room;
                room.AccountInRooms.Add(accountInRoom);
                room.Status = null;
                
                await _unitOfWork.Repository<Room>().CreateAsync(room);             
                await _unitOfWork.CommitAsync();
                
                var rs = _mapper.Map<RoomResponse>(room);
                rs.GameName=topic.Game.Name;
                rs.TopicName=topic.Name;

                var accountInRoomResponse = _mapper.Map<AccountInRoomResponse>(accountInRoom);
                accountInRoomResponse.Avatar = account.Avatar;
                accountInRoomResponse.Username = account.UserName;
                rs.AccountInRoomResponses = new List<AccountInRoomResponse>();
                rs.AccountInRoomResponses.Add(accountInRoomResponse);
               
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Room Error!!!", ex?.Message);
            }
        }
        public async Task<RoomResponse> UpdateRoom(string roomCode,List<CreateAndUpdateAccountInRoomRequest> createAccountInRoomRequests)
        {
            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (roomCode == null || roomCode == "")
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x => x.Topic.Game).Include(x => x.AccountInRooms).SingleOrDefault(x => x.Code == roomCode);

                    if (room == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Room Code {roomCode} Not Found!!!!!", "");

                    if (room.Status == false)
                        throw new CrudException(HttpStatusCode.BadRequest, "The room has ended so you can't update", "");

                    if (room.StartTime != null && room.StartTime < date)
                        throw new CrudException(HttpStatusCode.BadRequest, $"Accounts in this room party have already started playing so cannot be canceled", "");

                    List<AccountInRoom> list = new List<AccountInRoom>();
                    list = room.AccountInRooms.ToList();

                    foreach (var accountInRoom in createAccountInRoomRequests)
                    {
                        if (accountInRoom.AccountId <= 0 || accountInRoom.Duration < 0 || accountInRoom.Mark < 0 || accountInRoom.PieceOfInformation < 0)
                            throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                        var account = _mapper.Map<AccountInRoom>(accountInRoom);

                        var acc = _unitOfWork.Repository<Account>().Find(x => x.Id == accountInRoom.AccountId);
                        if (acc == null)
                            throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountInRoom.AccountId} Not Found!!!!!", "");

                        if (acc.Status.Equals(false))
                        {
                            throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountInRoom.AccountId} Not Available!!!!!", "");
                        }

                        var accInRoom = _unitOfWork.Repository<AccountInRoom>().Find(x => x.AccountId == accountInRoom.AccountId && x.RoomId == room.Id);
                        if (accInRoom != null)
                            throw new CrudException(HttpStatusCode.BadRequest, $"The account id {accInRoom.AccountId} that joined this room", "");

                        account.Room = room;
                        account.IsAdmin = false;
                        account.RoomId = room.Id;
                        list.Add(account);
                    }
                    room.AccountInRooms = list;

                    if (room.AccountInRooms.Count() > room.AmountPlayer)
                        throw new CrudException(HttpStatusCode.BadRequest, "The number of participants does not match the amout player", "");

                    await _unitOfWork.Repository<Room>().Update(room, room.Id);
                    await _unitOfWork.CommitAsync();

                    var rs = _mapper.Map<RoomResponse>(room);
                    rs.GameName = room.Topic.Game.Name;
                    rs.TopicName = room.Topic.Name;
                    rs.AccountInRoomResponses = _mapper.Map<List<AccountInRoomResponse>>(room.AccountInRooms);

                    foreach (var acc in rs.AccountInRoomResponses)
                    {
                        acc.Avatar = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).Avatar;
                        acc.Username = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).UserName;
                    }


                    return rs;
                }
                finally
                {
                    _semaphore.Release();

                    //try...finally:    
                    // để đảm bảo rằng tài nguyên được giải phóng sau khi không còn cần thiết kể cả khi có ngoại lệ xảy ra
                    // Điều này giúp tránh được deadlock vì tài nguyên sẽ luôn được giải phóng dù có lỗi xảy ra trong quá trình thực thi hay không.
                }

            }

            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Member To Room Error!!!", ex?.Message);
            }
        }
        public async Task<List<LeaderboardResponse>> GetLeaderboardOfRoom(string roomCode)
        {
            try
            {
                if (roomCode == null || roomCode == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var room = _unitOfWork.Repository<Room>().GetAll().AsNoTracking().Include(c => c.AccountInRooms)
                      .SingleOrDefault(c => c.Code == roomCode);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with code {roomCode}", "");
                }
            
                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                if (room.AccountInRooms.Count() > 0)
                {
                    var orderedAccounts = room.AccountInRooms.OrderByDescending(x => x.Mark);
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

                        var mark = room.AccountInRooms
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
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of room error!!!!!", ex.Message);
            }
        }
        public async Task<RoomResponse> GetRoomById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Invalid", "");
                }
                var response = _unitOfWork.Repository<Room>().GetAll().AsNoTracking().Include(x => x.Topic).Include(x => x.Topic.Game).Select(x => new RoomResponse
                {
                    Id = x.Id,
                    TopicId = x.TopicId,
                    TopicName = x.Topic.Name,
                    Status = x.Status,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    AmountPlayer = x.AmountPlayer,
                    Name=x.Name,
                    Code=x.Code,
                    GameName = x.Topic.Game.Name,
                    AccountInRoomResponses = new List<AccountInRoomResponse>(x.AccountInRooms.Select(a => new AccountInRoomResponse
                    {
                        Id = a.Id,
                        AccountId = a.AccountId,
                        CompletedTime = a.CompletedTime,
                        Username=a.Account.UserName,
                        Duration = a.Duration,
                        IsAdmin = a.IsAdmin,
                        Avatar=a.Account.Avatar,
                        Mark = a.Mark,
                        PieceOfInformation = a.PieceOfInformation
                    }))
                }).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with id {id.ToString()}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get room by id Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<RoomResponse>> GetRooms(RoomRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<RoomResponse>(request);
                var response = _unitOfWork.Repository<Room>().GetAll()
                    .AsNoTracking().Include(x => x.Topic).Include(x => x.Topic.Game).Select(x => new RoomResponse
                {
                    Id = x.Id,
                    TopicId = x.TopicId,
                    TopicName = x.Topic.Name,
                    Status = x.Status,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    AmountPlayer = x.AmountPlayer,
                    Name=x.Name,
                    Code = x.Code,
                    GameName = x.Topic.Game.Name,
                    AccountInRoomResponses =new List<AccountInRoomResponse>(x.AccountInRooms.Select(a=> new AccountInRoomResponse
                    {
                        Id=a.Id,
                        AccountId=a.AccountId,
                        CompletedTime=a.CompletedTime,
                        Duration=a.Duration,
                        Avatar=a.Account.Avatar,
                        Username = a.Account.UserName,
                        IsAdmin =a.IsAdmin,
                        Mark=a.Mark,
                        PieceOfInformation=a.PieceOfInformation
                    }))
                }).DynamicFilter(filter).ToList();               
                var sort = PageHelper<RoomResponse>.Sorting(paging.SortType, response, paging.ColName);
                var result = PageHelper<RoomResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get rooms list error!!!!!", ex.Message);
            }
        }
       public async Task<RoomResponse> DeleteRoom(int roomId, int accountId)
        {
            try
            {
                if (roomId <= 0 || accountId <=0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");
                }

               var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x=>x.AccountInRooms)
                    .Include(x => x.Topic.Game).FirstOrDefault(u => u.Id == roomId);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with id{roomId.ToString()}", "");
                }

                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == accountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} Not Found!!!!!", "");

                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId} Not Available!!!!!", "");
                }

                if (room.AccountInRooms.SingleOrDefault(x => x.IsAdmin == true).AccountId != accountId)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Only the admin role has the right to cancel rooms", "");
                
                if( room.StartTime != null && room.StartTime < date)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Accounts in this room party have already started playing so cannot be canceled", "");

                await _unitOfWork.Repository<AccountInRoom>().DeleteRange(room.AccountInRooms.ToArray());
                await _unitOfWork.Repository<Room>().RemoveAsync(room);
                await _unitOfWork.CommitAsync();
                
                var rs = _mapper.Map<RoomResponse>(room);
                rs.TopicName = room.Topic.Name;
                rs.GameName = room.Topic.Game.Name;
                rs.AccountInRoomResponses = _mapper.Map<List<AccountInRoomResponse>>(room.AccountInRooms);
                
                foreach (var acc in rs.AccountInRoomResponses)
                {
                    acc.Avatar = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).Avatar;
                    acc.Username = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).UserName;
                }
                return rs ;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Cancel room error!!!!!", ex.Message);
            }
        }
        public async Task<RoomResponse> LeaveRoom(int roomId, int accountId)
        {
            try
            {
                if (roomId <= 0 || accountId <=0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Information Invalid", "");
                }

                var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x => x.AccountInRooms)
                    .Include(x => x.Topic.Game).FirstOrDefault(u => u.Id == roomId);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with id {roomId}", "");
                }

                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == accountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} Not Found!!!!!", "");

                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId} Not Available!!!!!", "");
                }

                var accountInRoom = _unitOfWork.Repository<AccountInRoom>().Find(x => x.AccountId == accountId && x.RoomId==roomId);
                if (accountInRoom == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"This account does not participate in the room {roomId}", "");
                
                if (room.StartTime != null && room.StartTime < date)
                    throw new CrudException(HttpStatusCode.BadRequest, $"The room has started so {accountId} can't leave", "");
                
                _unitOfWork.Repository<AccountInRoom>().Delete(accountInRoom);
                await _unitOfWork.CommitAsync();
                
                var rs = _mapper.Map<RoomResponse>(room);
                rs.TopicName = room.Topic.Name;
                rs.GameName = room.Topic.Game.Name;
                rs.AccountInRoomResponses = _mapper.Map<List<AccountInRoomResponse>>(room.AccountInRooms);
                
                foreach (var acc in rs.AccountInRoomResponses)
                {
                    acc.Avatar = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).Avatar;
                    acc.Username = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).UserName;
                }
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Leave room error!!!!!", ex.Message);
            }
        }   
        public async Task<RoomResponse> GetToStartRoom(string roomCode, int accountId, int time)
        {
            try
            {
                if (roomCode ==null || roomCode =="" || accountId <=0 || time <0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Information Invalid", "");
                }

                var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x => x.AccountInRooms)
                    .Include(x => x.Topic.Game).FirstOrDefault(u => u.Code == roomCode);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with code {roomCode}", "");
                }

                if(room.AccountInRooms.Count() <2 || room.AccountInRooms.Count() > room.AmountPlayer)
                    throw new CrudException(HttpStatusCode.BadRequest, "The number of participants does not match the amout player", "");
                
                if(room.AccountInRooms.SingleOrDefault(x=>x.AccountId==accountId && x.IsAdmin==true)==null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId} does not have permission to start this room id", "");

                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == accountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} Not Found!!!!!", "");

                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId} Not Available!!!!!", "");
                }

                room.StartTime = date;
                room.Status = true;

                var roomRealtimeDatabase = await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<RoomRealtimeDatabaseResponse>($"room/{roomCode}");
                Thread.Sleep(time*1000 + 10000);

                room.Status = false;
                room.EndTime = date.AddMilliseconds(time*1000+10000);

                await _unitOfWork.Repository<Room>().UpdateDispose(room, room.Id);
                await _unitOfWork.CommitAsync();


                if (roomRealtimeDatabase != null)
                {
                    roomRealtimeDatabase.AmountPlayerDone = room.AccountInRooms.Count();
                    await _firebaseRealtimeDatabaseService.SetAsyncOfFlutterRealtimeDatabase<int>($"room/{roomCode}/AmountPlayerDone", roomRealtimeDatabase.AmountPlayerDone);
                }
                var rs = _mapper.Map<RoomResponse>(room);
                rs.TopicName = room.Topic.Name;
                rs.GameName = room.Topic.Game.Name;
                rs.AccountInRoomResponses = _mapper.Map<List<AccountInRoomResponse>>(room.AccountInRooms);

                foreach (var acc in rs.AccountInRoomResponses)
                {
                    acc.Avatar = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).Avatar;
                    acc.Username = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).UserName;
                }
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Start Room To Play error!!!!!", ex.Message);
            }
        }

        public async Task<bool> RemoveRoomPartyInRealtimeDatabase(string roomCode, int delayTime)
        {
            try
            {
                if (roomCode == null || roomCode == "" ||delayTime <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Information Invalid", "");
                }

                var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x => x.AccountInRooms)
                   .Include(x => x.Topic.Game).FirstOrDefault(u => u.Code == roomCode);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with code {roomCode}", "");
                }

                var roomRealtimeDatabase = await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<RoomRealtimeDatabaseResponse>($"room/{roomCode}");
                Thread.Sleep(delayTime * 1000);

                if (roomRealtimeDatabase == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Room Id {roomCode} has already deleted", "");

                return await _firebaseRealtimeDatabaseService.RemoveDataFlutterRealtimeDatabase($"room/{roomCode}");
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Remove Room in realtime database error!!!!!", ex.Message);
            }
        }
    }
}