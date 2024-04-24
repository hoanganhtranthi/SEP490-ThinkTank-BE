using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAccountIn1vs1Service
    {
        Task<PagedResults<AccountIn1vs1Response>> GetAccount1vs1s(AccountIn1vs1Request request, PagingRequest paging);
        Task<bool> RemoveRoom1vs1InRealtimeDatabase(string room1vs1Id, int delayTime);
        Task<AccountIn1vs1Response> CreateAccount1vs1(CreateAndUpdateAccountIn1vs1Request createAccount1vs1Request);
        Task<AccountIn1vs1Response> UpdateAccount1vs1(CreateAndUpdateAccountIn1vs1Request updateAccount1vs1Request);
        Task<AccountIn1vs1Response> GetAccount1vs1ById(int id);
        Task<dynamic> FindAccountTo1vs1(int id, int coin, int gameId);
        Task<bool> RemoveAccountFromQueue(int id, int coin, int gameId, string uniqueId, int delay);
        Task<dynamic> CreateRoomPlayCountervailingWithFriend(int gameId, int accountId1, int accountId2);
        Task<bool> GetToStartRoom(string room1vs1Id, bool isUser1, int time, int progressTime);
    }
}
