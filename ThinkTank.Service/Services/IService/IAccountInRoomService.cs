using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAccountInRoomService
    {
        Task<PagedResults<AccountInRoomResponse>> GetAccountInRooms(AccountInRoomRequest accountInRoomRequest, PagingRequest paging);
        Task<AccountInRoomResponse> UpdateAccountInRoom(int accountInRoomId,CreateAndUpdateAccountInRoomRequest createAccountInRoomRequest);
        Task<AccountInRoomResponse> GetAccountInRoomById(int id);

    }
}
